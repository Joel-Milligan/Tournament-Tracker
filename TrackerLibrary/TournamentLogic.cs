using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary
{
    public static class TournamentLogic
    {
        public static void CreateRounds(TournamentModel model)
        {
            // Order list randomly
            List<TeamModel> randomisedTeams = RandomiseTeamOrder(model.EnteredTeams);

            // Check if it is big enough and if not, add in byes
            int rounds = FindNumberOfRounds(randomisedTeams.Count);
            int byes = NumberOfByes(rounds, randomisedTeams.Count);

            // Create our first round of matchups - Have all the information
            model.Rounds.Add(CreateFirstRound(byes, randomisedTeams));

            // Create every round after that - Can create pyramid by not competing teams
            CreateOtherRounds(model, rounds);
        }

        private static List<TeamModel> RandomiseTeamOrder(List<TeamModel> teams)
        {
            return teams.OrderBy(x => Guid.NewGuid()).ToList();
        }

        private static int FindNumberOfRounds(int teamCount)
        {
            double log = Math.Log(teamCount, 2);
            return (int)Math.Ceiling(log);
        }

        private static int NumberOfByes(int rounds, int numberOfTeams)
        {
            int totalTeams = (int)Math.Pow(2, rounds);
            return totalTeams - numberOfTeams;
        }

        private static List<MatchupModel> CreateFirstRound(int byes, List<TeamModel> teams)
        {
            List<MatchupModel> output = new();
            MatchupModel currentMatchup = new();

            foreach (TeamModel team in teams)
            {
                currentMatchup.Entries.Add(new MatchupEntryModel { TeamCompeting = team });

                if (byes > 0 || currentMatchup.Entries.Count == 2)
                {
                    currentMatchup.MatchupRound = 1;
                    output.Add(currentMatchup);
                    currentMatchup = new();

                    if (byes > 0)
                    {
                        byes -= 1;
                    }
                }
            }

            return output;
        }
 
        private static void CreateOtherRounds(TournamentModel model, int rounds)
        {
            int round = 2;
            List<MatchupModel> previousRound = model.Rounds[0];
            List<MatchupModel> currentRound = new();
            MatchupModel currentMatchup = new();

            while (round <= rounds)
            {
                foreach (MatchupModel matchup in previousRound)
                {
                    currentMatchup.Entries.Add(new MatchupEntryModel { ParentMatchup = matchup });

                    if (currentMatchup.Entries.Count == 2)
                    {
                        currentMatchup.MatchupRound = round;
                        currentRound.Add(currentMatchup);
                        currentMatchup = new();
                    }
                }

                model.Rounds.Add(currentRound);
                
                previousRound = currentRound;
                currentRound = new();
                round += 1;
            }
        }

        public static void UpdateTournamentResults(TournamentModel model)
        {
            List<MatchupModel> toScore = new();

            foreach (List<MatchupModel> round in model.Rounds)
            {
                foreach (MatchupModel matchup in round)
                {
                    bool byeMatchup = matchup.Entries.Count == 1;
                    bool scoredWithoutWinner = matchup.Winner is null && matchup.Entries.Any(x => x.Score != 0);

                    if (scoredWithoutWinner || byeMatchup)
                    {
                        toScore.Add(matchup);
                    }
                }
            }

            MarkWinnerInMatchups(toScore);
            AdvanceWinners(toScore, model);

            toScore.ForEach(x => GlobalConfig.Connection.UpdateMatchup(x));
        }

        private static void MarkWinnerInMatchups(List<MatchupModel> models)
        {
            string greaterWins = ConfigurationManager.AppSettings["greaterWins"];

            foreach (MatchupModel matchup in models)
            {
                if (matchup.Entries.Count == 1)
                {
                    matchup.Winner = matchup.Entries[0].TeamCompeting;
                    continue;
                }

                if (greaterWins == "0")
                {
                    // 0 means low score wins
                    if (matchup.Entries[0].Score < matchup.Entries[1].Score)
                    {
                        matchup.Winner = matchup.Entries[0].TeamCompeting;
                    }
                    else if (matchup.Entries[1].Score < matchup.Entries[0].Score)
                    {
                        matchup.Winner = matchup.Entries[1].TeamCompeting;
                    }
                    else
                    {
                        throw new Exception("We do not allow ties in this application.");
                    }
                }
                else
                {
                    if (matchup.Entries[0].Score > matchup.Entries[1].Score)
                    {
                        matchup.Winner = matchup.Entries[0].TeamCompeting;
                    }
                    else if (matchup.Entries[1].Score > matchup.Entries[0].Score)
                    {
                        matchup.Winner = matchup.Entries[1].TeamCompeting;
                    }
                    else
                    {
                        throw new Exception("We do not allow ties in this application.");
                    }
                }
            }
        }

        private static void AdvanceWinners(List<MatchupModel> models, TournamentModel tournament)
        {
            foreach (MatchupModel matchup in models)
            {
                foreach (List<MatchupModel> round in tournament.Rounds)
                {
                    foreach (MatchupModel rm in round)
                    {
                        foreach (MatchupEntryModel me in rm.Entries)
                        {
                            if (me.ParentMatchup?.Id == matchup.Id)
                            {
                                me.TeamCompeting = matchup.Winner;
                                GlobalConfig.Connection.UpdateMatchup(rm);
                            }
                        }
                    }
                } 
            }
        }
    }
}
