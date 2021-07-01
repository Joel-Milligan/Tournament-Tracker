using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
    public partial class TournamentViewerForm : Form
    {
        TournamentModel _tournament;
        List<int> _rounds = new();
        List<MatchupModel> _matchups = new();

        public TournamentViewerForm(TournamentModel tournament)
        {
            InitializeComponent();

            _tournament = tournament;

            LoadFormData();

            LoadRounds();
        }

        private void LoadFormData()
        {
            tournamentName.Text = _tournament.TournamentName;
        }

        private void WireUpRoundsLists()
        {
            roundDropDown.DataSource = null;
            roundDropDown.DataSource = _rounds;
        }

        private void WireUpMatchupsLists()
        {
            matchupListBox.DataSource = null;
            matchupListBox.DataSource = _matchups;
            matchupListBox.DisplayMember = nameof(MatchupModel.DisplayName);
        }

        private void LoadRounds()
        {
            _rounds.Add(1);
            int currRound = 1;

            foreach (List<MatchupModel> matchups in _tournament.Rounds)
            {
                if (matchups.First().MatchupRound > currRound)
                {
                    currRound = matchups.First().MatchupRound;
                    _rounds.Add(currRound);
                }
            }

            WireUpRoundsLists();
        }

        private void roundDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMatchups((int)roundDropDown.SelectedItem);
        }

        private void LoadMatchups(int round)
        {
            foreach (List<MatchupModel> matchups in _tournament.Rounds)
            {
                if (matchups.First().MatchupRound == round)
                {
                    List<MatchupModel> newMatchups = new();

                    foreach (MatchupModel matchup in matchups)
                    {
                        if (matchup.Winner is null || !unplayedOnlyCheckbox.Checked)
                        {
                            newMatchups.Add(matchup);
                        }
                    }

                    _matchups = newMatchups;
                }
            }

            WireUpMatchupsLists();
            DisplayMatchupInfo();
        }

        private void DisplayMatchupInfo()
        {
            bool isVisible = _matchups.Count > 0;
            
            teamOneName.Visible = isVisible;
            teamOneScoreLabel.Visible = isVisible;
            teamOneScoreValue.Visible = isVisible;
            teamTwoName.Visible = isVisible;
            teamTwoScoreLabel.Visible = isVisible;
            teamTwoScoreValue.Visible = isVisible;
            versusLabel.Visible = isVisible;
            scoreButton.Visible = isVisible;
        }

        private void LoadMatchup()
        {
            MatchupModel matchup = (MatchupModel)matchupListBox.SelectedItem;

            // TODO: This loop is disgusting
            for (int i = 0; i < matchup?.Entries.Count; i++)
            {
                if (i == 0)
                {
                    if (matchup.Entries[0].TeamCompeting is not null)
                    {
                        teamOneName.Text = matchup.Entries[0].TeamCompeting.TeamName;
                        teamOneScoreValue.Text = matchup.Entries[0].Score.ToString();

                        teamTwoName.Text = "<bye>";
                        teamTwoScoreValue.Text = "0";
                    }
                    else
                    {
                        teamOneName.Text = "Not Yet Set";
                        teamOneScoreValue.Text = "";
                    }
                }

                if (i == 1)
                {
                    if (matchup.Entries[1].TeamCompeting is not null)
                    {
                        teamTwoName.Text = matchup.Entries[1].TeamCompeting.TeamName;
                        teamTwoScoreValue.Text = matchup.Entries[1].Score.ToString();
                    }
                    else
                    {
                        teamTwoName.Text = "Not Yet Set";
                        teamTwoScoreValue.Text = "";
                    }
                }
            }
        }

        private void matchupListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMatchup();
        }

        private void unplayedOnlyCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            LoadMatchups((int)roundDropDown.SelectedItem);
        }

        private void scoreButton_Click(object sender, EventArgs e)
        {
            MatchupModel matchup = (MatchupModel)matchupListBox.SelectedItem;

            double teamOneScore = 0;
            double teamTwoScore = 0;

            // TODO: This loop is disgusting
            for (int i = 0; i < matchup?.Entries.Count; i++)
            {
                if (i == 0)
                {
                    if (matchup.Entries[0].TeamCompeting is not null)
                    {
                        bool scoreValid = double.TryParse(teamOneScoreValue.Text, out teamOneScore);

                        if (scoreValid)
                        {
                            matchup.Entries[0].Score = teamOneScore; 
                        }
                        else
                        {
                            MessageBox.Show("Please enter a valid score for team 1.");
                            return;
                        }
                    }
                }

                if (i == 1)
                {
                    if (matchup.Entries[1].TeamCompeting is not null)
                    {
                        bool scoreValid = double.TryParse(teamTwoScoreValue.Text, out teamTwoScore);

                        if (scoreValid)
                        {
                            matchup.Entries[1].Score = teamTwoScore;
                        }
                        else
                        {
                            MessageBox.Show("Please enter a valid score for team 2.");
                            return;
                        }
                    }
                }
            }

            if (teamOneScore > teamTwoScore)
            {
                matchup.Winner = matchup.Entries[0].TeamCompeting;
            }
            else if (teamTwoScore > teamOneScore)
            {
                matchup.Winner = matchup.Entries[1].TeamCompeting;
            }
            else
            {
                MessageBox.Show("I do not handle tie games.");
            }

            foreach (List<MatchupModel> round in _tournament.Rounds)
            {
                foreach (MatchupModel rm in round)
                {
                    foreach (MatchupEntryModel me in rm.Entries)
                    {
                        if (me.ParentMatchup?.Id == matchup.Id)
                        {
                            me.TeamCompeting = matchup.Winner;
                            GlobalConfig.Connection.UpdateMatchup(rm);
                        }
                    }
                }
            }

            LoadMatchups((int)roundDropDown.SelectedItem);

            // TODO - Text connector implementation of this has a bug
            GlobalConfig.Connection.UpdateMatchup(matchup);
        }
    }
}
