using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary.DataAccess.TextHelpers
{
    public static class TextConnectorProcessor
    {
        /// <summary>
        /// Return the file path to the given file, as defined in the configuration.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>Full path of the given file name.</returns>
        public static string FullFilePath(this string fileName)
        {
            return $"{ ConfigurationManager.AppSettings["filePath"] }\\{ fileName }";
        }

        /// <summary>
        /// Load the file at the given path into as a list of strings.
        /// </summary>
        /// <param name="path">Path of the file you wish to load.</param>
        /// <returns>List of lines from the file.</returns>
        public static List<string> LoadFile(this string path)
        {
            if (!File.Exists(path))
            {
                return new();
            }

            return File.ReadAllLines(path).ToList();
        }

        /// <summary>
        /// Converts a list of strings into PrizeModels.
        /// </summary>
        /// <param name="lines">A list of strings loaded from CSV.</param>
        /// <returns>List of PrizeModels parsed from each line in the CSV</returns>
        public static List<PrizeModel> ConvertToPrizeModels(this List<string> lines)
        {
            List<PrizeModel> output = new();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                PrizeModel p = new();

                // TODO: This will crash if it isn't an int.
                p.Id = int.Parse(cols[0]); 
                p.PlaceNumber = int.Parse(cols[1]);
                p.PlaceName = cols[2];
                p.PrizeAmount = decimal.Parse(cols[3]);
                p.PrizePercent = double.Parse(cols[4]);
                output.Add(p);
            }

            return output;
        }

        /// <summary>
        /// Saves the list of prize models to the specified file
        /// </summary>
        /// <param name="models">List of prize models to save.</param>
        /// <param name="fileName">Filename to save the models to.</param>
        public static void SaveToPrizeFile(this List<PrizeModel> models, string fileName)
        {
            List<string> lines = new();

            foreach (PrizeModel p in models)
            {
                lines.Add($"{ p.Id },{ p.PlaceNumber },{ p.PlaceName },{ p.PrizeAmount },{ p.PrizePercent }");
            }

            File.WriteAllLines(fileName.FullFilePath(), lines);
        }
    }
}
