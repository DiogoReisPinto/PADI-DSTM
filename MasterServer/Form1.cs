using System;
using PADIDSTM;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MasterServer
{
    delegate void delLog(string logEntry);
    delegate void delServerLoad(Dictionary<string, int> serversLoad);
    delegate void delUpdatePadInt(Dictionary<int, string[]> padIntLocation);

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            TcpChannel channel = new TcpChannel(8086);
            ChannelServices.RegisterChannel(channel, true);
            RemoteMaster master = new RemoteMaster(this);
            DSTMLib.masterServ = master; 
            string start = "Starting Master Server on port: " + 8086;
            log(start);

            RemotingServices.Marshal(
                master,
                "RemoteMaster",
                typeof(RemoteMaster));
            string end = "Master Server started";
            log(end);
        }


        public void log(string logEntry)
        {
            Log.AppendText(logEntry + "\r\n");
        }

        public void updateServerLoad(Dictionary<string, int> serversLoad)
        {
            this.ServerLocationLoad.Items.Clear();
            foreach (KeyValuePair<string, int> server in serversLoad)
            {
                string[] values = {server.Key, Convert.ToString(server.Value)};
                var listItem = new ListViewItem(values);
                this.ServerLocationLoad.Items.Add(listItem);
            }
        }

        public void updatePadInts(Dictionary<int, string[]> padIntLocation)
        {
            this.PadIntLocation.Items.Clear();
            foreach (KeyValuePair<int, string[]> padint in padIntLocation)
            {
                string[] locations = padint.Value;
                string[] padints1 = { Convert.ToString(padint.Key), padint.Value[0] };
                string[] padints2 = { Convert.ToString(padint.Key), padint.Value[1] };
                var listItem1 = new ListViewItem(padints1);
                var listItem2 = new ListViewItem(padints2);
                this.PadIntLocation.Items.Add(listItem1);
                this.PadIntLocation.Items.Add(listItem2);
            }
        }

        private void ServerLocationLoad_OnListViewItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            RecoverBtn.Enabled = (ServerLocationLoad.SelectedItems.Count > 0);
            FreezeBtn.Enabled = (ServerLocationLoad.SelectedItems.Count > 0);
            FailBtn.Enabled = (ServerLocationLoad.SelectedItems.Count > 0);
        }

        private void RecoverBtn_Click(object sender, EventArgs e)
        {
            string url = ServerLocationLoad.SelectedItems[0].Text;
            log("Recovering server at " + url);
            DSTMLib.Recover(url);
        }

        private void ServerLocationLoad_SelectedIndexChanged(object sender, EventArgs e)
        {
            RecoverBtn.Enabled = (ServerLocationLoad.SelectedItems.Count > 0);
            FreezeBtn.Enabled = (ServerLocationLoad.SelectedItems.Count > 0);
            FailBtn.Enabled = (ServerLocationLoad.SelectedItems.Count > 0);
        }

        private void FreezeBtn_Click(object sender, EventArgs e)
        {
            string url = ServerLocationLoad.SelectedItems[0].Text;
            log("Delaying server at " + url);
            DSTMLib.Freeze(url);
        }

        private void FailBtn_Click(object sender, EventArgs e)
        {
            string url = ServerLocationLoad.SelectedItems[0].Text;
            log("Crashing server at " + url);
            DSTMLib.Fail(url);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            log("Dumping Information on slaves");
            DSTMLib.Status();
        }

        
    }
}
