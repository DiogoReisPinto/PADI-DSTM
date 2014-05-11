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
using System.Runtime.Serialization.Formatters;
using System.Collections;

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
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = 8086;
            //props["timeout"] = "4000";
            TcpChannel channel = new TcpChannel(props, null, provider);
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
                string[] values = new String[2];
                if (server.Value > 2000000)
                {
                    values[0] =server.Key;
                    values[1] = "UNAVAILABLE";
                }
                else
                {
                    values[0] = server.Key;
                    values[1] = Convert.ToString(server.Value);
                }
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
            int val = DSTMLib.getServerLoad(url);
            log("THE VALUE IS: " + val);
            ServerLocationLoad.SelectedItems[0].SubItems[1].Text = Convert.ToString(val);
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
            ServerLocationLoad.SelectedItems[0].SubItems[1].Text = "UNAVAILABLE";
        }

        private void FailBtn_Click(object sender, EventArgs e)
        {
            string url = ServerLocationLoad.SelectedItems[0].Text;
            log("Crashing server at " + url);
            DSTMLib.Fail(url);
            ServerLocationLoad.SelectedItems[0].SubItems[1].Text = "UNAVAILABLE";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            log("Dumping Information on slaves");
            DSTMLib.Status();
        }

        
    }
}
