using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerLibrary.Models
{
    public class TournamentModel
    {
        public event EventHandler<DateTime> OnTournamentComplete;

        /// <summary>
        /// Unique identifier for this tournament
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the tournament.
        /// </summary>
        public string TournamentName { get; set; }

        /// <summary>
        /// The fee that each team must pay to enter the tournament.
        /// </summary>
        public decimal EntryFee { get; set; }

        /// <summary>
        /// The full list of teams that are entered into the tournament (payed their fee).
        /// </summary>
        public List<TeamModel> EnteredTeams { get; set; } = new();

        /// <summary>
        /// The list of prizes that can be won in the tournament.
        /// </summary>
        public List<PrizeModel> Prizes { get; set; } = new();

        /// <summary>
        /// The list of rounds in the tournaments.
        /// </summary>
        public List<List<MatchupModel>> Rounds { get; set; } = new();

        public void CompleteTournament()
        {
            OnTournamentComplete?.Invoke(this, DateTime.Now);
        }
    }
}
