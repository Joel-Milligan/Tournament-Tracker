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
