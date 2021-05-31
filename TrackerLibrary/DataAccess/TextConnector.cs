using TrackerLibrary.Models;
using TrackerLibrary.DataAccess.TextHelpers;
using System.Collections.Generic;
using System.Linq;

namespace TrackerLibrary.DataAccess
{
    public class TextConnector : IDataConnection
    {
        private const string PrizesFile = "PrizeModels.csv";
        private const string PeopleFile = "PersonModels.csv";

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

            return model;
        }

        public List<PersonModel> GetPerson_All()
        {
            return PeopleFile.FullFilePath().LoadFile().ConvertToPersonModels();
        }
    }
}
