using System;
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
            GlobalConfig.InitConnections(DatabaseType.TextFile);
            
            // Entrypoint of the application.
            Application.Run(new CreateTournamentForm());

            //TODO: Real entry point = Application.Run(new TournamentDashboardForm());
        }
    }
}
