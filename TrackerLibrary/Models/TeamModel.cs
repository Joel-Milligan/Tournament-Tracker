using System.Collections.Generic;

namespace TrackerLibrary.Models
{
    public class TeamModel
    {
        /// <summary>
        /// Unique identifier of this team.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the team.
        /// </summary>
        public string TeamName { get; set; }
        
        /// <summary>
        /// The members that compose this team.
        /// </summary>
        public List<PersonModel> TeamMembers { get; set; } = new();

        
    }
}
