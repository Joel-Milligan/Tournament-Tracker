using System;
using System.Windows.Forms;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
    public partial class CreatePrizeForm : Form
    {
        IPrizeRequester callingForm;
        public CreatePrizeForm(IPrizeRequester caller)
        {
            InitializeComponent();

            callingForm = caller;
        }

        private void createPrizeButton_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                PrizeModel model = new(
                    placeNameValue.Text, 
                    placeNumberValue.Text, 
                    prizeAmountValue.Text, 
                    prizePercentageValue.Text
                );

                // Save prize into data source
                GlobalConfig.Connection.CreatePrize(model);

                // Send prize information back to calling form and close.
                callingForm.PrizeComplete(model);
                this.Close();
            }
            else
            {
                MessageBox.Show("This form has invalid information. Please check it and try again.");
            }
        }

        private bool ValidateForm()
        {
            bool output = true;

            // If the place number a number?
            if (int.TryParse(placeNumberValue.Text, out int placeNumber) == false)
            {
                output = false;
            }

            // Is the place number above or equal to 1?
            if (placeNumber < 1)
            {
                output = false;
            }

            // Is there a place name?
            if (placeNameValue.Text.Length == 0)
            {
                output = false;
            }

            bool prizeAmountValid = double.TryParse(prizeAmountValue.Text, out double prizeAmount);
            bool prizePercentageValid = int.TryParse(prizePercentageValue.Text, out int prizePercentage);
            
            if (!prizeAmountValid || !prizePercentageValid )
            {
                output = false;
            }

            if (prizeAmount <= 0 && prizePercentage <= 0)
            {
                output = false;
            }

            if (prizePercentage < 0 || prizePercentage > 100)
            {
                output = false;
            }

            return output;
        }
    }
}
