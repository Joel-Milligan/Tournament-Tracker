using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
    public partial class CreateTournamentForm : Form, IPrizeRequester, ITeamRequester
    {
        List<TeamModel> availableTeams = GlobalConfig.Connection.GetTeam_All();
        List<TeamModel> selectedTeams = new();

        List<PrizeModel> selectedPrizes = new();

        public CreateTournamentForm()
        {
            InitializeComponent();
            WireUpLists();
        }

        private void WireUpLists()
        {
            selectTeamDropDown.DataSource = null;
            selectTeamDropDown.DataSource = availableTeams;
            selectTeamDropDown.DisplayMember = nameof(TeamModel.TeamName);

            tournamentPlayersListBox.DataSource = null;
            tournamentPlayersListBox.DataSource = selectedTeams;
            tournamentPlayersListBox.DisplayMember = nameof(TeamModel.TeamName);

            prizeListBox.DataSource = null;
            prizeListBox.DataSource = selectedPrizes;
            prizeListBox.DisplayMember = nameof(PrizeModel.PlaceName);
        }

        private void addTeamButton_Click(object sender, EventArgs e)
        {
            TeamModel t = (TeamModel)selectTeamDropDown.SelectedItem;

            if (t is not null)
            {
                availableTeams.Remove(t);
                selectedTeams.Add(t);

                WireUpLists();
            }
        }

        private void deleteTeamButton_Click(object sender, EventArgs e)
        {
            TeamModel p = (TeamModel)tournamentPlayersListBox.SelectedItem;

            if (p is not null)
            {
                selectedTeams.Remove(p);
                availableTeams.Add(p);

                WireUpLists();
            }
        }

        private void createPrizeButton_Click(object sender, EventArgs e)
        {
            // Open the CreatePrizeForm
            CreatePrizeForm form = new(this);
            form.Show();
        }

        /// <summary>
        /// Called by the create prize form when a prize is created, so this form can add it to the selected Prizes list box.
        /// </summary>
        /// <param name="model">The prize information</param>
        public void PrizeComplete(PrizeModel model)
        {
            // Take the PrizeModel and put it into out list of selected prizes
            selectedPrizes.Add(model);
            WireUpLists();
        }

        private void createNewTeamLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Open the CreateTeamForm
            CreateTeamForm form = new(this);
            form.Show();
        }

        public void TeamComplete(TeamModel model)
        {
            // Take the TeamModel and put it into out list of selected teams
            selectedTeams.Add(model);
            WireUpLists();
        }

        private void deletePrizeButton_Click(object sender, EventArgs e)
        {
            PrizeModel p = (PrizeModel)prizeListBox.SelectedItem;

            if (p is not null)
            {
                selectedPrizes.Remove(p);
                WireUpLists();
                // TODO: This could be deleting from the data source, but this could cause a multitude of issues.
            }
        }

        private void createTournamentButton_Click(object sender, EventArgs e)
        {
            // Validate data
            bool feeAcceptable = decimal.TryParse(entryFeeValue.Text, out decimal fee);

            if (!feeAcceptable)
            {
                MessageBox.Show(
                    "You need to enter a valid Entry Fee.", 
                    "Invalid Fee", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                return;
            }

            // Populate the tournament model
            TournamentModel tournament = new();

            tournament.TournamentName = tournamentNameValue.Text;
            tournament.EntryFee = fee;
            tournament.Prizes = selectedPrizes;
            tournament.EnteredTeams = selectedTeams;

            TournamentLogic.CreateRounds(tournament);

            // Save the tournament to the data source.
            GlobalConfig.Connection.CreateTournament(tournament);
        }
    }
}
