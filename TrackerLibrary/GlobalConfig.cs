using System.Collections.Generic;
using TrackerLibrary.DataAccess;
using System.Configuration;

namespace TrackerLibrary
{
    public static class GlobalConfig
    {
        public const string PrizesFile = "PrizeModels.csv";
        public const string PeopleFile = "PersonModels.csv";
        public const string TeamsFile = "TeamModels.csv";
        public const string TournamentFile = "TournamentModels.csv";
        public const string MatchupsFile = "MatchupModels.csv";
        public const string MatchupEntriesFile = "MatchupEntryModels.csv";

        public static IDataConnection Connection { get; private set; }

        /// <summary>
        /// Initialises the data connections, with choices of database and/or text file.
        /// </summary>
        /// <param name="database">Indicates if using a database for persistance.</param>
        /// <param name="textFiles">Indicates if using a text file for persistance.</param>
        public static void InitConnections(DatabaseType connectionType)
        {
            switch (connectionType)
            {
                case DatabaseType.Sql:
                    Connection = new SqlConnector();
                    break;
                case DatabaseType.TextFile:
                    Connection = new TextConnector();
                    break;
                default:
                    break;
            }
        }

        public static string CnnString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }

        public static string AppKeyLookup(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
