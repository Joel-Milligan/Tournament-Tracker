using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerLibrary.Models
{
    public class MatchupModel
    {
        /// <summary>
        /// Unique identifier for this matchup
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The entries (one per team) in this matchup.
        /// </summary>
        public List<MatchupEntryModel> Entries { get; set; } = new();
        
        /// <summary>
        /// The team that won this matchup.
        /// </summary>
        public TeamModel Winner { get; set; }

        /// <summary>
        /// The round that this matchup is part of.
        /// </summary>
        public int MatchupRound { get; set; }
    }
}
