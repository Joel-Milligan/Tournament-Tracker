namespace TrackerLibrary
{
    public class MatchupEntryModel
    {
        /// <summary>
        /// Represents one of the teams in the parent matchup.
        /// </summary>
        public TeamModel TeamCompeting { get; set; }

        /// <summary>
        /// Represents the score of the competing team in this matchup.
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// The matchup that this entry belongs to.
        /// </summary>
        public MatchupModel ParentMatchup { get; set; }
    }
}