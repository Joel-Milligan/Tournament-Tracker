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
                // TODO: Create the SQL connection
            }

            if (textFiles)
            {
                // TODO: Create the text connection
            }
        }
    }
}
