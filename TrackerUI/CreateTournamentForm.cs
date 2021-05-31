﻿using System;
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
    public partial class CreateTournamentForm : Form
    {
        List<TeamModel> availableTeams = GlobalConfig.Connection.GetTeam_All();
        List<TeamModel> selectedTeams = new();

        public CreateTournamentForm()
        {
            InitializeComponent();
            WireUpLists();
        }

        private void WireUpLists()
        {
            selectTeamDropDown.DataSource = null;
            selectTeamDropDown.DataSource = availableTeams;
            selectTeamDropDown.DisplayMember = "TeamName";

            tournamentPlayersListBox.DataSource = null;
            tournamentPlayersListBox.DataSource = selectedTeams;
            tournamentPlayersListBox.DisplayMember = "TeamName";
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
    }
}
