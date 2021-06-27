﻿using TrackerLibrary.Models;
using TrackerLibrary.DataAccess.TextHelpers;
using System.Collections.Generic;
using System.Linq;

namespace TrackerLibrary.DataAccess
{
    public class TextConnector : IDataConnection
    {
        private const string PrizesFile = "PrizeModels.csv";
        private const string PeopleFile = "PersonModels.csv";
        private const string TeamsFile = "TeamModels.csv";
        private const string TournamentFile = "TournamentModels.csv";
        private const string MatchupsFile = "MatchupModels.csv";
        private const string MatchupEntriesFile = "MatchupEntryModels.csv";

        /// <summary>
        /// Saves a new prize to the Prize text file
        /// </summary>
        /// <param name="model">The prize information.</param>
        /// <returns>The prize information, including the unique identifier.</returns>
        public PrizeModel CreatePrize(PrizeModel model)
        {
            // Load the text file and convert the text to List<PrizeModel>
            List<PrizeModel> prizes = PrizesFile.FullFilePath().LoadFile().ConvertToPrizeModels();

            // Find the max ID, then create the new ID
            int currentId = 1;
            
            if (prizes.Count != 0)
            {
                currentId = prizes.OrderByDescending(p => p.Id).First().Id + 1;
            }

            model.Id = currentId;

            // Add the new record with the new ID
            prizes.Add(model);

            // Convert the prizes to List<string> and save it to the text file
            prizes.SaveToPrizeFile(PrizesFile);

            // TODO: Make this void
            return model;
        }

        /// <summary>
        /// Saves a new person to the People text file
        /// </summary>
        /// <param name="model">The person's information.</param>
        /// <returns>The person information, including the unique identifier.</returns>
        public PersonModel CreatePerson(PersonModel model)
        {
            // Load the text file and convert the text to List<PersonModel>
            List<PersonModel> people = GetPerson_All();

            // Find the max ID, then create the new ID
            int currentId = 1;

            if (people.Count != 0)
            {
                currentId = people.OrderByDescending(p => p.Id).First().Id + 1;
            }

            model.Id = currentId;

            // Add the new record with the new ID
            people.Add(model);

            // Convert the people to List<string> and save it to the text file
            people.SaveToPersonFile(PeopleFile);

            // TODO: Make this void
            return model;
        }

        /// <summary>
        /// Returns all people in the text file.
        /// </summary>
        /// <returns>List of all person models from the file.</returns>
        public List<PersonModel> GetPerson_All()
        {
            return PeopleFile.FullFilePath().LoadFile().ConvertToPersonModels();
        }

        /// <summary>
        /// Saves the new team to the text file.
        /// </summary>
        /// <param name="model">Team information.</param>
        /// <returns>The team that got saved to the file.</returns>
        public TeamModel CreateTeam(TeamModel model)
        {
            List<TeamModel> teams = GetTeam_All();

            // Find the max ID, then create the new ID
            int currentId = 1;

            if (teams.Count > 0)
            {
                currentId = teams.OrderByDescending(p => p.Id).First().Id + 1;
            }

            model.Id = currentId;

            // Add the new record with the new ID
            teams.Add(model);

            // Convert the people to List<string> and save it to the text file
            teams.SaveToTeamsFile(TeamsFile);

            // TODO: Make this void
            return model;
        }

        public List<TeamModel> GetTeam_All()
        {
            return TeamsFile.FullFilePath().LoadFile().ConvertToTeamModels(PeopleFile);
        }

        /// <summary>
        /// Saves the new tournament to the text file.
        /// </summary>
        /// <param name="model">Tournament information.</param>
        public void CreateTournament(TournamentModel model)
        {
            List<TournamentModel> tournaments = TournamentFile
                .FullFilePath()
                .LoadFile()
                .ConvertToTournamentModels(TeamsFile, PeopleFile, PrizesFile);

            int currentId = 1;

            if (tournaments.Count > 0)
            {
                currentId = tournaments.OrderByDescending(x => x.Id).First().Id + 1;
            }

            model.Id = currentId;

            model.SaveRoundsToFile(MatchupsFile);

            tournaments.Add(model);

            tournaments.SaveToTournamentsFile(TournamentFile);
        }

        public List<TournamentModel> GetTournament_All()
        {
            return TournamentFile
                .FullFilePath()
                .LoadFile()
                .ConvertToTournamentModels(TeamsFile, PeopleFile, PrizesFile);
        }
    }
}
