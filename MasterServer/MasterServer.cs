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

        public string GetLocationNewPadInt(int uid)
        {
            string urlServerDest=null;
            //CALL TO THE LOAD BALANCER ALGORITHM TO CHECK WHAT IS THE BEST SERV
            //FOR NOW IS JUST USING THE FIRST REGISTERED SLAVE
            foreach (KeyValuePair<int, string> entry in serversLocation)
            {
                if (entry.Key ==1)
                    urlServerDest = entry.Value;
            }
            
            return urlServerDest;
        }

        public string GetTS(int uid)
        {
            //uid of slave server for tie-breaker
            string timestamp = GetTimestamp(DateTime.Now) + uid;
            return timestamp;
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

        public static String GetTimestamp(this DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }
    }
}
