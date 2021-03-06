using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TrackerLibrary.Models;

namespace TrackerLibrary.DataAccess
{
    public class SqlConnector : IDataConnection
    {
        private const string dbName = "Tournaments";

        /// <summary>
        /// Saves a new person to the database
        /// </summary>
        /// <param name="model">The person's information.</param>
        /// <returns>The person information, including the unique identifier.</returns>
        public void CreatePerson(PersonModel model)
        {
            using IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(dbName));
            DynamicParameters p = new();

            p.Add("@FirstName", model.FirstName);
            p.Add("@LastName", model.LastName);
            p.Add("@EmailAddress", model.EmailAddress);
            p.Add("@PhoneNumber", model.PhoneNumber);
            p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("dbo.spPeople_Insert", p, commandType: CommandType.StoredProcedure);

            model.Id = p.Get<int>("@id");
        }

        /// <summary>
        /// Saves a new prize to the database
        /// </summary>
        /// <param name="model">The prize information.</param>
        /// <returns>The prize information, including the unique identifier.</returns>
        public void CreatePrize(PrizeModel model)
        {
            using IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(dbName));
            DynamicParameters p = new();

            p.Add("@PlaceNumber", model.PlaceNumber);
            p.Add("@PlaceName", model.PlaceName);
            p.Add("@PrizeAmount", model.PrizeAmount);
            p.Add("@PrizePercentage", model.PrizePercent);
            p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("dbo.spPrizes_Insert", p, commandType: CommandType.StoredProcedure);

            model.Id = p.Get<int>("@id");
        }

        /// <summary>
        /// Saves the new team to the database.
        /// </summary>
        /// <param name="model">Team information.</param>
        /// <returns>The team that got saved to the database.</returns>
        public void CreateTeam(TeamModel model)
        {
            using IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(dbName));
            DynamicParameters p = new();

            p.Add("@TeamName", model.TeamName);
            p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            // TODO: Validate there isn't an empty team name.
            connection.Execute("dbo.spTeams_Insert", p, commandType: CommandType.StoredProcedure);

            model.Id = p.Get<int>("@id");

            foreach (PersonModel tm in model.TeamMembers)
            {
                p = new();
                p.Add("@TeamId", model.Id);
                p.Add("@PersonId", tm.Id);

                connection.Execute("dbo.spTeamMembers_Insert", p, commandType: CommandType.StoredProcedure);
            }
        }

        /// <summary>
        /// Saves the new tournament to the database.
        /// </summary>
        /// <param name="model">Tournament information.</param>
        /// <returns>The tournament that got saved to the database.</returns>
        public void CreateTournament(TournamentModel model)
        {
            using IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(dbName));

            SaveTournament(model, connection);
            SaveTournamentPrizes(model, connection);
            SaveTournamentEntries(model, connection);
            SaveTournamentRounds(model, connection);
            TournamentLogic.UpdateTournamentResults(model);
        }
        
        private static void SaveTournament(TournamentModel model, IDbConnection connection)
        {
            DynamicParameters p = new();

            p.Add("@TournamentName", model.TournamentName);
            p.Add("@EntryFee", model.EntryFee);
            p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("dbo.spTournaments_Insert", p, commandType: CommandType.StoredProcedure);

            model.Id = p.Get<int>("@id");
        }

        private static void SaveTournamentPrizes(TournamentModel model, IDbConnection connection)
        {
            foreach (PrizeModel prize in model.Prizes)
            {
                DynamicParameters p = new();
                p.Add("@TournamentId", model.Id);
                p.Add("@PrizeId", prize.Id);

                connection.Execute("dbo.spTournamentPrizes_Insert", p, commandType: CommandType.StoredProcedure);
            }
        }

        private static void SaveTournamentEntries(TournamentModel model, IDbConnection connection)
        {
            foreach (TeamModel team in model.EnteredTeams)
            {
                DynamicParameters p = new();
                p.Add("@TournamentId", model.Id);
                p.Add("@TeamId", team.Id);

                connection.Execute("dbo.spTournamentEntries_Insert", p, commandType: CommandType.StoredProcedure);
            }
        }

        private static void SaveTournamentRounds(TournamentModel model, IDbConnection connection)
        {
            // Loop through the rounds, then the matchups inside the round
            foreach (List<MatchupModel> round in model.Rounds)
            {
                foreach (MatchupModel matchup in round)
                {
                    // Save the matchup
                    DynamicParameters p = new();

                    p.Add("@TournamentId", model.Id);
                    p.Add("@MatchupRound", matchup.MatchupRound);
                    p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                    connection.Execute("dbo.spMatchups_Insert", p, commandType: CommandType.StoredProcedure);

                    matchup.Id = p.Get<int>("@id");

                    // Loop through the entries and save them
                    foreach (MatchupEntryModel matchupEntry in matchup.Entries)
                    {
                        p = new();

                        p.Add("@MatchupId", matchup.Id);


                        if (matchupEntry.ParentMatchup is null)
                        {
                            p.Add("@ParentMatchupId", null);
                        }
                        else
                        {
                            p.Add("@ParentMatchupId", matchupEntry.ParentMatchup.Id);
                        }

                        if (matchupEntry.TeamCompeting is null)
                        {
                            p.Add("@TeamCompetingId", null);
                        }
                        else
                        {
                            p.Add("@TeamCompetingId", matchupEntry.TeamCompeting.Id);
                        }

                        p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                        connection.Execute("dbo.spMatchupEntries_Insert", p, commandType: CommandType.StoredProcedure);
                    }

                }
            }
        }

        /// <summary>
        /// Returns all people in the database.
        /// </summary>
        /// <returns>List of all person models from the database.</returns>
        public List<PersonModel> GetPerson_All()
        {
            List<PersonModel> output;

            using (IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(dbName)))
            {
                // This isn't just a simple return, for debugging purposes.
                output = connection.Query<PersonModel>("dbo.spPeople_GetAll").ToList();
            }

            return output;
        }

        /// <summary>
        /// Returns all teams in the database.
        /// </summary>
        /// <returns>List of all team models from the database.</returns>
        public List<TeamModel> GetTeam_All()
        {
            List<TeamModel> output;

            using (IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(dbName)))
            {
                output = connection.Query<TeamModel>("dbo.spTeam_GetAll").ToList();

                foreach (TeamModel team in output)
                {
                    DynamicParameters p = new();
                    p.Add("@TeamId", team.Id);

                    team.TeamMembers = connection.Query<PersonModel>("dbo.spTeamMembers_GetByTeam", p, commandType: CommandType.StoredProcedure).ToList();
                }
            }

            return output;
        }

        public List<TournamentModel> GetTournament_All()
        {
            List<TournamentModel> output;

            using (IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(dbName)))
            {
                output = connection.Query<TournamentModel>("dbo.spTournaments_GetAll").ToList();
                DynamicParameters p = new();

                foreach (TournamentModel t in output)
                {
                    // Populate Prizes
                    p = new();
                    p.Add("@TournamentId", t.Id);

                    t.Prizes = connection.Query<PrizeModel>("dbo.spPrizes_GetByTournament", p, commandType: CommandType.StoredProcedure).ToList();

                    // Populate Teams
                    p = new();
                    p.Add("@TournamentId", t.Id);

                    t.EnteredTeams = connection.Query<TeamModel>("dbo.spTeam_GetByTournament", p , commandType: CommandType.StoredProcedure).ToList();

                    foreach (TeamModel team in t.EnteredTeams)
                    {
                        p = new();
                        p.Add("@TeamId", team.Id);

                        team.TeamMembers = connection.Query<PersonModel>("dbo.spTeamMembers_GetByTeam", p, commandType: CommandType.StoredProcedure).ToList();
                    }

                    // Populate Rounds
                    p = new();
                    p.Add("@TournamentId", t.Id);
                    List<MatchupModel> matchups = connection.Query<MatchupModel>("dbo.spMatchups_GetByTournament", p, commandType: CommandType.StoredProcedure).ToList();

                    foreach (MatchupModel matchup in matchups)
                    {
                        p = new();
                        p.Add("@MatchupId", matchup.Id);

                        matchup.Entries = connection.Query<MatchupEntryModel>("dbo.spMatchupEntries_GetByMatchup", p, commandType: CommandType.StoredProcedure).ToList();

                        List<TeamModel> allTeams = GetTeam_All();

                        // Populate each matchup (1 model)
                        if (matchup.WinnerId > 0)
                        {
                            matchup.Winner = allTeams.Where(x => x.Id == matchup.WinnerId).First();
                        }

                        // Populate each entry (2 models)
                        foreach (MatchupEntryModel entry in matchup.Entries)
                        {
                            if (entry.TeamCompetingId > 0)
                            {
                                entry.TeamCompeting = allTeams.Where(x => x.Id == entry.TeamCompetingId).First();
                            }

                            if (entry.ParentMatchupId > 0)
                            {
                                entry.ParentMatchup = matchups.Where(x => x.Id == entry.ParentMatchupId).First();
                            }
                        }
                    }

                    List<MatchupModel> currRow = new();
                    int currRound = 1;

                    foreach (MatchupModel matchup in matchups)
                    {
                        if (matchup.MatchupRound > currRound)
                        {
                            t.Rounds.Add(currRow);
                            currRow = new();
                            currRound += 1;
                        }

                        currRow.Add(matchup);
                    }

                    t.Rounds.Add(currRow);
                }
            }

            return output;
        }

        public void UpdateMatchup(MatchupModel model)
        {
            using IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(dbName));
            DynamicParameters p = new();
            
            if (model.Winner is not null)
            {
                p.Add("@id", model.Id);
                p.Add("@WinnerId", model.Winner.Id);

                connection.Execute("dbo.spMatchups_Update", p, commandType: CommandType.StoredProcedure); 
            }

            foreach (MatchupEntryModel entry in model.Entries)
            {
                if (entry.TeamCompeting is not null)
                {
                    p = new();
                    p.Add("@id", entry.Id);
                    p.Add("@TeamCompetingId", entry.TeamCompeting.Id);
                    p.Add("@Score", entry.Score);

                    connection.Execute("dbo.spMatchupEntries_Update", p, commandType: CommandType.StoredProcedure); 
                }
            }
        }

        public void CompleteTournament(TournamentModel model)
        {
            using IDbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(dbName));

            DynamicParameters p = new();
            p.Add("@id", model.Id);

            connection.Execute("dbo.spTournaments_Complete", p, commandType: CommandType.StoredProcedure);
        }
    }
}
