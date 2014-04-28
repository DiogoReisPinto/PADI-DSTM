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
        private Dictionary<int, string[]> padIntLocation = new Dictionary<int, string[]>();
        private int transactionID = 0;
        private Object tIDLock = new Object();
        private Object padIntLocationLock = new Object();
        private Form1 form;

        public RemoteMaster(Form1 form)
        {
            this.form = form;
        }

        public string[] GetLocationNewPadInt(int uid)
        {
            string[] urlServerDest = null;
            //CALL TO THE LOAD BALANCER ALGORITHM 
            lock (padIntLocationLock)
            {
                urlServerDest = DiscoverPadInt(uid);
                
                if (urlServerDest[0] == null)
                {
                    Console.WriteLine(urlServerDest);
                    urlServerDest = getBestSlave();
                    //CASO EM QUE SO EXISTE UM SLAVE
                    if (urlServerDest[1] == null)
                        return null;
                    padIntLocation.Add(uid, new string[] { "UNDEFINED", "UNDEFINED" });
                 }
                else {
                    urlServerDest = null;
                }
            }
            return urlServerDest;
        }

        private string[] getBestSlave()
        {
            String[] url = new String[2];
            var sortedSlaves =(from item in serversLoad
                                    orderby item.Value 
                                    ascending
                                    select item);
            int i = 0;
            foreach (KeyValuePair<string, int> item in sortedSlaves)
            {
                url[i]=item.Key;
                i++;
                if (i == 2)
                    break;
            }
            return url;
            
        }

        public string[] DiscoverPadInt(int uid)
        {
            string[] url = new string[2];
            foreach (KeyValuePair<int, string[]> entry in padIntLocation)
            {
                if (entry.Key == uid)
                {
                    url[0] = entry.Value[0];
                    url[1] = entry.Value[1];
                }
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
            if (padIntLocation[uid][0] == "UNDEFINED")
            {
                padIntLocation[uid][0] = serverURL;
                serversLoad[serverURL]++;
            }
            else
            {
                padIntLocation[uid][1] = serverURL;
                serversLoad[serverURL]++;
            }
               
            Console.WriteLine("REGISTER New PAD LOCATION: "+padIntLocation[uid][0]);
            Console.WriteLine("REGISTER New PAD LOCATION: " + padIntLocation[uid][1]);
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
                string[] url = padIntLocation[id];
                serversLoad[url[0]]--;
                serversLoad[url[1]]--;
                padIntLocation.Remove(id);
                ISlave server1 = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url[0]);
                ISlave server2 = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url[1]);
                server1.removePadInt(id);
                server2.removePadInt(id);

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
