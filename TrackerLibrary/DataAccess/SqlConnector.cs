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
        public PersonModel CreatePerson(PersonModel model)
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

            return model;
        }

        /// <summary>
        /// Saves a new prize to the database
        /// </summary>
        /// <param name="model">The prize information.</param>
        /// <returns>The prize information, including the unique identifier.</returns>
        public PrizeModel CreatePrize(PrizeModel model)
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

            return model;
        }

        /// <summary>
        /// Saves the new team to the database.
        /// </summary>
        /// <param name="model">Team information.</param>
        /// <returns>The team that got saved to the database.</returns>
        public TeamModel CreateTeam(TeamModel model)
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

            return model;
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
    }
}
