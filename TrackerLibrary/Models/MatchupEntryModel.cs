namespace TrackerLibrary.Models
{
    public class MatchupEntryModel
    {
        /// <summary>
        /// Unique identifier for this matchup entry
        /// </summary>
        public int Id { get; set; }

        public int TeamCompetingId { get; set; }

        /// <summary>
        /// Represents one of the teams in the parent matchup.
        /// </summary>
        public TeamModel TeamCompeting { get; set; }

        /// <summary>
        /// Represents the score of the competing team in this matchup.
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// The unique identifier for the parent matchup (team).
        /// </summary>
        public int ParentMatchupId { get; set; }

        /// <summary>
        /// The matchup that this entry belongs to.
        /// </summary>
        public MatchupModel ParentMatchup { get; set; }
    }
}