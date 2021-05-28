using System.Collections.Generic;
using TrackerLibrary.DataAccess;
using System.Configuration;

namespace TrackerLibrary
{
    public static class GlobalConfig
    {
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
                    // TODO: Set up the SQL connector properly
                    Connection = new SqlConnector();
                    break;
                case DatabaseType.TextFile:
                    // TODO: Set up the text connector properly
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
    }
}
