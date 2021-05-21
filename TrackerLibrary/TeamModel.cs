using System.Collections.Generic;

namespace TrackerLibrary
{
    public class TeamModel
    {
        /// <summary>
        /// The members that compose this team.
        /// </summary>
        public List<PersonModel> TeamMembers { get; set; } = new();

        /// <summary>
        /// The name of the team.
        /// </summary>
        public string TeamName { get; set; }
    }
}
