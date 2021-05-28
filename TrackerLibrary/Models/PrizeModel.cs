using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerLibrary.Models
{
    public class PrizeModel
    {
        /// <summary>
        /// Unique identifier for this prize
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// The placement that this prize is awarded to. (1 equals first, 2 equals second, etc.)
        /// </summary>
        public int PlaceNumber { get; set; }

        /// <summary>
        /// The custom name of this placement, such as "Champion" or "Runner Up"
        /// </summary>
        public string PlaceName { get; set; }

        /// <summary>
        /// The amount of money this prize is.
        /// </summary>
        public decimal PrizeAmount { get; set; }

        /// <summary>
        /// The percent of prize pool that this prize awards.
        /// </summary>
        public double PrizePercent { get; set; }

        public PrizeModel() {}

        public PrizeModel(string placeName, string placeNumber, string prizeAmount, string prizePercentage)
        {
            PlaceName = placeName;

            int.TryParse(placeNumber, out int placeNumberValue);
            PlaceNumber = placeNumberValue;

            decimal.TryParse(prizeAmount, out decimal prizeAmountValue);
            PrizeAmount = prizeAmountValue;

            double.TryParse(prizePercentage, out double prizePercentageValue);
            PrizePercent = prizePercentageValue;
        }
    }
}
