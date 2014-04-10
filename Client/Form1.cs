using System;
using PADIDSTM;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class ClientForm : Form
    {
        private bool transaction = false;

        PadInt pi;

        public ClientForm()
        {
            InitializeComponent();
            Log.ScrollBars = ScrollBars.Vertical; 
            DSTMLib.Init();
        }

        private void StartEndTransaction_Click(object sender, EventArgs e)
        {
            if (!transaction)
            {
                DSTMLib.TxBegin();
                StartEndTransaction.Text = "End Transaction";
                transaction = true;
                log("Transaction started");
            }
            else
            {
                DSTMLib.TxCommit();
                StartEndTransaction.Text = "Begin Transaction";
                transaction = false;
                log("Transaction commited");
                setAccessButtons(false);
            }
            setPadIntButtons();
        }

        private void log(string logMessage)
        {
            Log.Text += logMessage + "\r\n";
        }

        private void setPadIntButtons()
        {
            CreatePadInt.Enabled = transaction;
            AccessPadInt.Enabled = transaction;
            PadIntID.Enabled = transaction;
        }

        private void CreatePadInt_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PadIntID.Text))
            {
                int id = Convert.ToInt32(PadIntID.Text);
                PadInt newPad = DSTMLib.CreatePadInt(id);
                if(newPad!=null)
                    log("PadInt " + id + " created");
                else
                    log("PadInt " + id + " already exists");
                PadIntID.Clear();
                setAccessButtons(false);
            }
            else
            {
                log("Please specify the ID of the PadInt");
            }
        }
    

        private void PadIntID_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
            && !char.IsDigit(e.KeyChar)
            && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void AccessPadInt_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PadIntID.Text))
            {
                int id = Convert.ToInt32(PadIntID.Text);
                pi = DSTMLib.AccessPadInt(id);
                if (pi != null)
                {
                    log("Accessing PadInt " + id);
                    PadIntID.Clear();
                    setAccessButtons(true);
                }
                else {
                    log("Cannot access PadInt " + id);
                }
            }
            else
            {
                log("Please specify the ID of the PadInt");
            }
        }

        private void setAccessButtons(bool accessing)
        {
            PadIntRead.Enabled = accessing;
            PadIntWrite.Enabled = accessing;
            WriteValue.Enabled = accessing;
        }

        private void PadIntRead_Click(object sender, EventArgs e)
        {
            log("Read value " + pi.Read());
        }

        private void PadIntWrite_Click(object sender, EventArgs e)
        {
            if (WriteValue.Text == "")
                log("Please specify a value to write");
            else
            {
                int value = Convert.ToInt32(WriteValue.Text);
                pi.Write(value);
                log("Wrote value " + value);
            }
        }
    }
}
