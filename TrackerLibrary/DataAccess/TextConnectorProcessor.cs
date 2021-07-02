using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary.DataAccess.TextHelpers
{
    public static class TextConnectorProcessor
    {
        /// <summary>
        /// Return the file path to the given file, as defined in the configuration.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>Full path of the given file name.</returns>
        public static string FullFilePath(this string fileName)
        {
            return $"{ ConfigurationManager.AppSettings["filePath"] }\\{ fileName }";
        }

        /// <summary>
        /// Load the file at the given path into as a list of strings.
        /// </summary>
        /// <param name="path">Path of the file you wish to load.</param>
        /// <returns>List of lines from the file.</returns>
        public static List<string> LoadFile(this string path)
        {
            if (!File.Exists(path))
            {
                return new();
            }

            return File.ReadAllLines(path).ToList();
        }

        /// <summary>
        /// Converts a list of strings into PrizeModels.
        /// </summary>
        /// <param name="lines">A list of strings loaded from CSV.</param>
        /// <returns>List of PrizeModels parsed from each line in the CSV</returns>
        public static List<PrizeModel> ConvertToPrizeModels(this List<string> lines)
        {
            List<PrizeModel> output = new();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                PrizeModel p = new();

                // TODO: This will crash if it isn't of the correct type.
                p.Id = int.Parse(cols[0]); 
                p.PlaceNumber = int.Parse(cols[1]);
                p.PlaceName = cols[2];
                p.PrizeAmount = decimal.Parse(cols[3]);
                p.PrizePercent = double.Parse(cols[4]);
                output.Add(p);
            }

            return output;
        }

        /// <summary>
        /// Saves the list of prize models to the specified file
        /// </summary>
        /// <param name="models">List of prize models to save.</param>
        /// <param name="fileName">Filename to save the models to.</param>
        public static void SaveToPrizeFile(this List<PrizeModel> models)
        {
            List<string> lines = new();

            foreach (PrizeModel p in models)
            {
                lines.Add($"{ p.Id },{ p.PlaceNumber },{ p.PlaceName },{ p.PrizeAmount },{ p.PrizePercent }");
            }

            File.WriteAllLines(GlobalConfig.PrizesFile.FullFilePath(), lines);
        }

        /// <summary>
        /// Converts a list of strings into PersonModels.
        /// </summary>
        /// <param name="lines">A list of strings loaded from CSV.</param>
        /// <returns>List of PersonModels parsed from each line in the CSV</returns>
        public static List<PersonModel> ConvertToPersonModels(this List<string> lines)
        {
            List<PersonModel> output = new();

            foreach (string line in lines)
            {
                // TODO: If there are commas in the data, this will cause problems (maybe last names)
                string[] cols = line.Split(',');

                PersonModel p = new();

                // TODO: This will crash if it isn't an int.
                p.Id = int.Parse(cols[0]);
                p.FirstName = cols[1];
                p.LastName = cols[2];
                p.EmailAddress = cols[3];
                p.PhoneNumber = cols[4];

                output.Add(p);
            }

            return output;
        }

        /// <summary>
        /// Saves the list of person models to the specified file
        /// </summary>
        /// <param name="models">List of person models to save.</param>
        /// <param name="fileName">Filename to save the models to.</param>
        public static void SaveToPersonFile(this List<PersonModel> models)
        {
            List<string> lines = new();

            foreach (PersonModel p in models)
            {
                lines.Add($"{ p.Id },{ p.FirstName },{ p.LastName },{ p.EmailAddress },{ p.PhoneNumber }");
            }

            File.WriteAllLines(GlobalConfig.PeopleFile.FullFilePath(), lines);
        }

        /// <summary>
        /// Converts a list of strings into TeamModels.
        /// </summary>
        /// <param name="lines">A list of strings loaded from CSV.</param>
        /// <returns>List of TeamModels parsed from each line in the CSV</returns>
        public static List<TeamModel> ConvertToTeamModels(this List<string> lines)
        {
            List<TeamModel> output = new();
            List<PersonModel> people = GlobalConfig.PeopleFile.FullFilePath().LoadFile().ConvertToPersonModels();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                TeamModel t = new();
                
                t.Id = int.Parse(cols[0]);
                t.TeamName = cols[1];

                string[] personIds = cols[2].Split('|');
                foreach (string id in personIds)
                {
                    t.TeamMembers.Add(people.Where(x => x.Id == int.Parse(id)).First());
                }
                
                output.Add(t);
            }

            return output;
        }

        /// <summary>
        /// Saves the list of team models to the specified file
        /// </summary>
        /// <param name="models">List of team models to save.</param>
        /// <param name="fileName">Filename to save the models to.</param>
        public static void SaveToTeamsFile(this List<TeamModel> models)
        {
            List<string> lines = new();

            foreach (TeamModel t in models)
            {
                lines.Add($"{ t.Id },{ t.TeamName },{ ConvertPeopleListToString(t.TeamMembers) }");
            }

            File.WriteAllLines(GlobalConfig.TeamsFile.FullFilePath(), lines);
        }

        private static string ConvertPeopleListToString(List<PersonModel> people)
        {
            string output = "";

            if (people.Count == 0)
            {
                return "";
            }

            foreach (PersonModel p in people)
            {
                output += $"{p.Id}|";
            }

            // Remove trailing pipe character.
            output = output[0..^1];

            return output;
        }

        /// <summary>
        /// Converts a list of strings into TournamentModels.
        /// </summary>
        /// <param name="lines">A list of strings loaded from CSV.</param>
        /// <returns>List of TournamentModels parsed from each line in the CSV</returns>
        public static List<TournamentModel> ConvertToTournamentModels(this List<string> lines)
        {
            // Format - id,TournamentName,EntryFee,(Entered Teams - id|id|id),(Prizes - id|id|id),(Rounds - id^id^id|id^id^id|id^id^id)
            List<TournamentModel> output = new();
            List<TeamModel> teams = GlobalConfig.TeamsFile.FullFilePath().LoadFile().ConvertToTeamModels();
            List<PrizeModel> prizes = GlobalConfig.PrizesFile.FullFilePath().LoadFile().ConvertToPrizeModels();
            List<MatchupModel> matchups = GlobalConfig.MatchupsFile.FullFilePath().LoadFile().ConvertToMatchupModels();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                TournamentModel tournament = new();

                tournament.Id = int.Parse(cols[0]);
                tournament.TournamentName = cols[1];
                tournament.EntryFee = decimal.Parse(cols[2]);

                string[] teamIds = cols[3].Split('|');
                foreach (string id in teamIds)
                {
                    tournament.EnteredTeams.Add(teams.Where(x => x.Id == int.Parse(id)).First());
                }

                if (cols[4].Length > 0)
                {
                    string[] prizeIds = cols[4].Split('|');
                    foreach (string id in prizeIds)
                    {
                        tournament.Prizes.Add(prizes.Where(x => x.Id == int.Parse(id)).First());
                    }
                }

                string[] rounds = cols[5].Split('|');

                foreach (string round in rounds)
                {
                    List<MatchupModel> ms = new();
                    string[] msText = round.Split('^');

                    foreach (string matchupModelTextId in msText)
                    {
                        ms.Add(matchups.Where(x => x.Id == int.Parse(matchupModelTextId)).First());
                    }

                    tournament.Rounds.Add(ms);
                }
                output.Add(tournament);
            }
            return output;
        }

        /// <summary>
        /// Saves the list of tournament models to the specified file
        /// </summary>
        /// <param name="models">List of tournament models to save.</param>
        /// <param name="fileName">Filename to save the models to.</param>
        public static void SaveToTournamentsFile(this List<TournamentModel> models)
        {
            List<string> lines = new();

            foreach (TournamentModel tournament in models)
            {
                lines.Add($"{ tournament.Id },{ tournament.TournamentName },{ tournament.EntryFee },{ ConvertTeamListToString(tournament.EnteredTeams) },{ ConvertPrizeListToString(tournament.Prizes) },{ ConvertRoundListToString(tournament.Rounds) }");
            }

            File.WriteAllLines(GlobalConfig.TournamentFile.FullFilePath(), lines);
        }

        private static string ConvertTeamListToString(List<TeamModel> teams)
        {
            string output = "";

            if (teams.Count == 0)
            {
                return "";
            }

            foreach (TeamModel team in teams)
            {
                output += $"{team.Id}|";
            }

            // Remove trailing pipe character.
            output = output[0..^1];

            return output;
        }

        private static string ConvertPrizeListToString(List<PrizeModel> prizes)
        {
            string output = "";

            if (prizes.Count == 0)
            {
                return "";
            }

            foreach (PrizeModel prize in prizes)
            {
                output += $"{prize.Id}|";
            }

            // Remove trailing pipe character.
            output = output[0..^1];

            return output;
        }

        private static string ConvertRoundListToString(List<List<MatchupModel>> rounds)
        {
            string output = "";

            if (rounds.Count == 0)
            {
                return "";
            }

            foreach (List<MatchupModel> round in rounds)
            {
                output += $"{ ConvertMatchupListToString(round) }|";
            }

            // Remove trailing pipe character.
            output = output[0..^1];

            return output;
        }

        private static string ConvertMatchupListToString(List<MatchupModel> matchups)
        {
            string output = "";

            if (matchups.Count == 0)
            {
                return "";
            }

            foreach (MatchupModel matchup in matchups)
            {
                output += $"{ matchup.Id }^";
            }

            // Remove trailing pipe character.
            output = output[0..^1];

            return output;
        }

        public static void SaveRoundsToFile(this TournamentModel model)
        {
            foreach (List<MatchupModel> round in model.Rounds)
            {
                foreach (MatchupModel matchup in round)
                {
                    matchup.SaveMatchupToFile();
                }
            }
        }

        private static void SaveMatchupToFile(this MatchupModel matchup)
        {
            List<MatchupModel> matchups = GlobalConfig.MatchupsFile.FullFilePath().LoadFile().ConvertToMatchupModels();

            int currentId = 1;

            if (matchups.Count > 0)
            {
                currentId = matchups.OrderByDescending(x => x.Id).First().Id + 1;
            }

            matchup.Id = currentId;

            matchups.Add(matchup);

            foreach (MatchupEntryModel entry in matchup.Entries)
            {
                entry.SaveEntryToFile();
            }

            List<string> lines = new();

            foreach (MatchupModel m in matchups)
            {
                string winner = "";
                if (m.Winner is not null)
                {
                    winner = m.Winner.Id.ToString();
                }

                lines.Add($"{ m.Id },{ ConvertMatchupEntryListToString(m.Entries) },{ winner },{ m.MatchupRound }");
            }

            File.WriteAllLines(GlobalConfig.MatchupsFile.FullFilePath(), lines);
        }

        private static string ConvertMatchupEntryListToString(List<MatchupEntryModel> entries)
        {
            string output = "";

            if (entries.Count == 0)
            {
                return "";
            }

            foreach (MatchupEntryModel e in entries)
            {
                output += $"{ e.Id }|";
            }

            output = output[0..^1];
            
            return output;
        }

        private static void SaveEntryToFile(this MatchupEntryModel entry)
        {
            List<MatchupEntryModel> entries = GlobalConfig.MatchupEntriesFile.FullFilePath().LoadFile().ConvertToMatchupEntryModels();

            int currentId = 1;

            if (entries.Count > 0)
            {
                currentId = entries.OrderByDescending(x => x.Id).First().Id + 1;
            }

            entry.Id = currentId;
            entries.Add(entry);

            List<string> lines = new();

            foreach (MatchupEntryModel e in entries)
            {
                string parent = string.Empty;
                if (e.ParentMatchup is not null)
                {
                    parent = e.ParentMatchup.Id.ToString();
                }

                string teamCompeting = string.Empty;
                if (e.TeamCompeting != null)
                {
                    teamCompeting = e.TeamCompeting.Id.ToString();
                }

                lines.Add($"{ e.Id },{ teamCompeting },{ e.Score },{ parent }");
            }

            File.WriteAllLines(GlobalConfig.MatchupEntriesFile.FullFilePath(), lines);
        }

        public static void UpdateMatchupToFile(this MatchupModel matchup)
        {
            List<MatchupModel> matchups = GlobalConfig.MatchupsFile.FullFilePath().LoadFile().ConvertToMatchupModels();

            MatchupModel oldMatchup = new();

            foreach (MatchupModel matchupModel in matchups)
            {
                if (matchupModel.Id == matchup.Id)
                {
                    oldMatchup = matchupModel;
                }
            }

            matchups.Remove(oldMatchup);

            matchups.Add(matchup);

            foreach (MatchupEntryModel entry in matchup.Entries)
            {
                entry.UpdateEntryToFile();
            }

            List<string> lines = new();

            foreach (MatchupModel m in matchups)
            {
                string winner = "";
                if (m.Winner is not null)
                {
                    winner = m.Winner.Id.ToString();
                }

                lines.Add($"{ m.Id },{ ConvertMatchupEntryListToString(m.Entries) },{ winner },{ m.MatchupRound }");
            }

            File.WriteAllLines(GlobalConfig.MatchupsFile.FullFilePath(), lines);
        }

        private static void UpdateEntryToFile(this MatchupEntryModel entry)
        {
            List<MatchupEntryModel> entries = GlobalConfig.MatchupEntriesFile.FullFilePath().LoadFile().ConvertToMatchupEntryModels();

            MatchupEntryModel oldEntry = new();

            foreach (MatchupEntryModel entryModel in entries)
            {
                if (entryModel.Id == entry.Id)
                {
                    oldEntry = entry;
                }
            }

            entries.Remove(oldEntry);

            entries.Add(entry);

            List<string> lines = new();

            foreach (MatchupEntryModel e in entries)
            {
                string parent = string.Empty;
                if (e.ParentMatchup is not null)
                {
                    parent = e.ParentMatchup.Id.ToString();
                }

                string teamCompeting = string.Empty;
                if (e.TeamCompeting != null)
                {
                    teamCompeting = e.TeamCompeting.Id.ToString();
                }

                lines.Add($"{ e.Id },{ teamCompeting },{ e.Score },{ parent }");
            }

            File.WriteAllLines(GlobalConfig.MatchupEntriesFile.FullFilePath(), lines);
        }

        public static List<MatchupModel> ConvertToMatchupModels(this List<string> lines) 
        {
            List<MatchupModel> output = new();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                MatchupModel matchup = new();

                matchup.Id = int.Parse(cols[0]);
                matchup.Entries = ConvertStringToMatchupEntryModels(cols[1]);

                if (cols[2] == string.Empty)
                {
                    matchup.Winner = null;
                }
                else
                {
                    matchup.Winner = LookupTeamById(int.Parse(cols[2]));
                }

                matchup.MatchupRound = int.Parse(cols[3]);

                output.Add(matchup);
            }

            return output;
        }

        private static List<MatchupEntryModel> ConvertStringToMatchupEntryModels(string input)
        {
            string[] ids = input.Split('|');
            List<string> entries = GlobalConfig.MatchupEntriesFile.FullFilePath().LoadFile();
            List<string> matchingEntries = new();

            foreach (string id in ids)
            {
                foreach (string entry in entries)
                {
                    string[] cols = entry.Split(',');

                    if (cols[0] == id)
                    {
                        matchingEntries.Add(entry);
                    }
                }
            }

            return matchingEntries.ConvertToMatchupEntryModels();
        }

        private static List<MatchupEntryModel> ConvertToMatchupEntryModels(this List<string> lines)
        {
            List<MatchupEntryModel> output = new();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                MatchupEntryModel m = new();

                m.Id = int.Parse(cols[0]);

                if (cols[1] == string.Empty)
                {
                    m.TeamCompeting = null;
                }
                else
                {
                    m.TeamCompeting = LookupTeamById(int.Parse(cols[1]));
                }

                m.Score = double.Parse(cols[2]);

                if (int.TryParse(cols[3], out int parentId))
                {
                    m.ParentMatchup = LookupMatchupById(parentId);
                }
                else
                {
                    m.ParentMatchup = null;
                }

                output.Add(m);
            }
            return output;
        }

        private static TeamModel LookupTeamById(int id)
        {
            List<string> teams = GlobalConfig.TeamsFile.FullFilePath().LoadFile();

            foreach (string team in teams)
            {
                string[] cols = team.Split(',');
                if (cols[0] == id.ToString())
                {
                    List<string> matchingTeams = new();
                    matchingTeams.Add(team);
                    return matchingTeams.ConvertToTeamModels().First();
                }
            }

            return null;
        }

        private static MatchupModel LookupMatchupById(int id)
        {
            List<string> matchups = GlobalConfig.MatchupsFile.FullFilePath().LoadFile();
            foreach (string matchup in matchups)
            {
                string[] cols = matchup.Split(',');
                if (cols[0] == id.ToString())
                {
                    List<string> matchingMatchups = new();
                    matchingMatchups.Add(matchup);
                    return matchingMatchups.ConvertToMatchupModels().First();
                }
            }
            
            return null;
        }
    }
}
