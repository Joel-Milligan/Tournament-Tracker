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
            int startingRound = model.CheckCurrentRound();
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
            int endingRound = model.CheckCurrentRound();

            if (endingRound > startingRound)
            {
                model.AlertUsersToNewRound();
            }
        }

        public static void AlertUsersToNewRound(this TournamentModel model)
        {
            int currentRoundNumber = model.CheckCurrentRound();
            List<MatchupModel> currentRound = model.Rounds.Where(x => x.First().MatchupRound == currentRoundNumber).First();

            foreach (MatchupModel matchup in currentRound)
            {
                foreach (MatchupEntryModel matchupEntry in matchup.Entries)
                {
                    foreach (PersonModel person in matchupEntry.TeamCompeting.TeamMembers)
                    {
                        string teamName = matchupEntry.TeamCompeting.TeamName;
                        MatchupEntryModel opponent = matchup.Entries.Where(x => x.TeamCompeting != matchupEntry.TeamCompeting).FirstOrDefault();
                        AlertPersonToNewRound(person, teamName, opponent);
                    }
                }
            }
        }

        private static void AlertPersonToNewRound(PersonModel person, string teamName, MatchupEntryModel opponent)
        {
            if (person.EmailAddress.Length == 0)
            {
                return;
            }

            string to = person.EmailAddress;

            string subject;

            StringBuilder body = new();

            if (opponent is not null)
            {
                subject = $"You have a new matchup with {opponent.TeamCompeting.TeamName}";
                
                body.AppendLine("<h1>You have a new matchup</h1>");
                body.Append("<strong>Competitor: </strong>");
                body.AppendLine(opponent.TeamCompeting.TeamName);
                body.AppendLine();
                body.AppendLine("Have a great time!");
                body.AppendLine("~Tournament Tracker");
            }
            else
            {
                subject = $"You have a bye week this round";

                body.AppendLine("<h1>A bi!</h1>");
                body.AppendLine("Enjoy your round off!");
                body.AppendLine("~Tournament Tracker</h1>");
            }

            EmailLogic.SendEmail(to, subject, body.ToString());
        }

        private static int CheckCurrentRound(this TournamentModel model)
        {
            int output = 1;

            foreach (List<MatchupModel> round in model.Rounds)
            {
                if (round.All(x => x.Winner is not null))
                {
                    output += 1;
                }
                else
                {
                    return output;
                }
            }

            CompleteTournament(model);
            return output - 1;
        }

        private static void CompleteTournament(TournamentModel tournament)
        {
            GlobalConfig.Connection.CompleteTournament(tournament);
            TeamModel winners = tournament.Rounds.Last().First().Winner;
            TeamModel runnerUp = tournament.Rounds.Last().First().Entries.Where(x => x.TeamCompeting != winners).First().TeamCompeting;

            decimal winnerPrize = 0;
            decimal runnerUpPrize = 0;

            if (tournament.Prizes.Count > 0)
            {
                decimal totalIncome = tournament.EnteredTeams.Count * tournament.EntryFee;

                PrizeModel firstPlacePrize = tournament.Prizes.Where(x => x.PlaceNumber == 1).FirstOrDefault();
                PrizeModel secondPlacePrize = tournament.Prizes.Where(x => x.PlaceNumber == 2).FirstOrDefault();

                if (firstPlacePrize is not null)
                {
                    winnerPrize = firstPlacePrize.CalculatePrizePayout(totalIncome);
                }

                if (secondPlacePrize is not null)
                {
                    runnerUpPrize = secondPlacePrize.CalculatePrizePayout(totalIncome);
                }
            }

            // Send Email to all tournament
            string subject;

            StringBuilder body = new();

            subject = $"In { tournament.TournamentName }, { winners.TeamName } has won!";

            body.AppendLine("<h1>We have a WINNER!</h1>");
            body.AppendLine("<p>Congratulations to our winner on a great tournament.</p>");
            body.AppendLine("<br />");

            if (winnerPrize > 0)
            {
                body.AppendLine($"<p>{ winners.TeamName } will receive ${ winnerPrize }</p>");
            }

            if (runnerUpPrize > 0)
            {
                body.AppendLine($"<p>{ runnerUp.TeamName } will receive ${ runnerUpPrize }</p>");
            }

            body.AppendLine("<p>Thanks for a great tournament everyone!</p>");
            body.AppendLine("~Tournament Tracker");

            List<string> bcc = new();

            foreach (TeamModel team in tournament.EnteredTeams)
            {
                foreach (PersonModel person in team.TeamMembers)
                {
                    if (person.EmailAddress.Length > 0)
                    {
                        bcc.Add(person.EmailAddress);
                    }
                }
            }

            EmailLogic.SendEmail(new(), bcc, subject, body.ToString());

            tournament.CompleteTournament();
        }

        private static decimal CalculatePrizePayout(this PrizeModel prize, decimal totalIncome)
        {
            decimal output;

            if (prize.PrizeAmount > 0)
            {
                output = prize.PrizeAmount;
            }
            else
            {
                output = decimal.Multiply(totalIncome, Convert.ToDecimal(prize.PrizePercent / 100));
            }

            return output;
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
