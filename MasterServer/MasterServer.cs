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
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());


        }
    }

    public class RemoteMaster : MarshalByRefObject, IMaster
    {
        private Dictionary<string, int> serversLoad = new Dictionary<string, int>();
        private Dictionary<int, string> padIntLocation = new Dictionary<int, string>();
        private int transactionID = 0;
        private Object tIDLock = new Object();
        private Object padIntLocationLock = new Object();
        private Form1 form;

        public RemoteMaster(Form1 form)
        {
            this.form = form;
        }

        public string GetLocationNewPadInt(int uid)
        {
            string urlServerDest = null;
            //CALL TO THE LOAD BALANCER ALGORITHM 
            lock (padIntLocationLock)
            {
                urlServerDest = DiscoverPadInt(uid);
                if (urlServerDest == null)
                {
                    urlServerDest = getBestSlave();
                    padIntLocation.Add(uid, "UNDEFINED");
                }
                else {
                    urlServerDest = null;
                }
            }
            return urlServerDest;
        }

        private string getBestSlave()
        {
            string url = null;
            int maxLoad = Int32.MaxValue;
            foreach (string slaveUrl in serversLoad.Keys) {
                int load = serversLoad[slaveUrl];
                if (load < maxLoad) {
                    url = slaveUrl;
                    maxLoad = load;
                }
            }
            return url;
        }

        public string DiscoverPadInt(int uid)
        {
            string url = null;
            foreach (KeyValuePair<int, string> entry in padIntLocation)
            {
                if (entry.Key == uid)
                    url=entry.Value;
            }
            return url;
        }

        public string GetTS(int uid)
        {
            //uid of slave server for tie-breaker
            string timestamp = TimeStamp.GetTimestamp(DateTime.Now) + "#" + uid;
            return timestamp;
        }

        public bool registerSlave(String url)
        {
            form.Invoke(new delLog(form.log), new Object[] { "Slave Server connecting at: " + url });
            serversLoad.Add(url, 0);
            form.Invoke(new delServerLoad(form.updateServerLoad), new Object[] { this.serversLoad });
            return true;
        }

        public void RegisterNewPadInt(int uid, string serverURL)
        {
            padIntLocation[uid]= serverURL;
            serversLoad[serverURL]++;
            Console.WriteLine("REGISTER New PAD LOCATION: "+padIntLocation[uid]);
            updateForm();
        }

        public int getTransactionID()
        {
            int tID;
            lock (tIDLock)
            {
                tID = transactionID++;
            }
            return tID;
        }

        public void callStatusOnSlaves()
        {
            foreach (string slave in serversLoad.Keys)
            {
                ISlave server = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               slave);
                server.status();
            }
        }

        public void removeUID(List<int> UIDsToRemove)
        {
            foreach (int id in UIDsToRemove)
            {
                string url = padIntLocation[id];
                serversLoad[url]--;
                padIntLocation.Remove(id);
                ISlave server = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url);
                server.removePadInt(id);
            }

            updateForm();
           
        }

        private void updateForm()
        {
            form.Invoke(new delUpdatePadInt(form.updatePadInts), new Object[] { this.padIntLocation });
            form.Invoke(new delServerLoad(form.updateServerLoad), new Object[] { this.serversLoad });
        }

        public override object InitializeLifetimeService()
        {

            return null;

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
