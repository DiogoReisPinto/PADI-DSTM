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
        private Dictionary<string, int> serversLoad = new Dictionary<string, int>();
        private Dictionary<int, string> padIntLocation = new Dictionary<int, string>();

        public string GetLocationNewPadInt(int uid)
        {
            string urlServerDest=null;
            //CALL TO THE LOAD BALANCER ALGORITHM 
            string k = serversLoad.Keys.First();
            urlServerDest = k;
            return urlServerDest;
        }

        public string DiscoverPadInt(int uid)
        {
            string url = null;
            foreach (KeyValuePair<int, string> entry in padIntLocation)
            {
                if (entry.Key == uid)
                    url = entry.Value;
            }
            return url;
        }

        public string GetTS(int uid)
        {
            //uid of slave server for tie-breaker
            string timestamp = TimeStamp.GetTimestamp(DateTime.Now) + uid;
            return timestamp;
        }

        public bool registerSlave(String url)
        {
            serversLoad.Add(url, 0);
            return true;
        }

        public void RegisterNewPadInt(int uid, string serverURL)
        {
            padIntLocation.Add(uid, serverURL);
        }

      
    }

    public static class TimeStamp
    {
        public static String GetTimestamp(this DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }
    }
}
