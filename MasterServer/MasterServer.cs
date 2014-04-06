using PADIDSTM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MasterServer
{
    public class MasterServer
    {
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            TcpChannel channel = new TcpChannel(8086);
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(RemoteMaster),
                "RemoteMaster",
                WellKnownObjectMode.Singleton);

        }
    }

    public class RemoteMaster : MarshalByRefObject, IMaster
    {
        private int serverID=0;
        private Dictionary<int, string> serversLocation = new Dictionary<int, string>();
        private Dictionary<int, int> padIntLocation = new Dictionary<int, int>();

        public string DiscoverPadInt(int uid) {
            string newString = "teste";
            return newString;
        }

        public int GetTS()
        {
            return 1;
        }

        public bool registerSlave(String url)
        {
            serversLocation.Add(++serverID, url);
            return true;
        }

        public void ConfirmWrite(int uid, string serverID)
        {
            return;
        }
    }
}
