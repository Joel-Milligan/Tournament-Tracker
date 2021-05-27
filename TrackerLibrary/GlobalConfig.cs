using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerLibrary
{
    public static class GlobalConfig
    {
        public static List<IDataConnection> Connections { get; private set; } = new();

        /// <summary>
        /// Initialises the data connections, with choices of database and/or text file.
        /// </summary>
        /// <param name="database">Indicates if using a database for persistance.</param>
        /// <param name="textFiles">Indicates if using a text file for persistance.</param>
        public static void InitConnections(bool database, bool textFiles)
        {
            if (database)
            {
                // TODO: Set up the SQL connector properly
                SqlConnector sql = new();
                Connections.Add(sql);
            }

            if (textFiles)
            {
                // TODO: Set up the text connector properly
                TextConnection text = new();
                Connections.Add(text);
            }
        }
    }
}
