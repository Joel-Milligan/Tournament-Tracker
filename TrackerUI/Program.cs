using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrackerLibrary;

namespace TrackerUI
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialise the database connections
            GlobalConfig.InitConnections(DatabaseType.Sql);
            
            // Entrypoint of the application.
            Application.Run(new CreateTeamForm());

            //TODO: Real entry point = Application.Run(new TournamentDashboardForm());
        }
    }
}
