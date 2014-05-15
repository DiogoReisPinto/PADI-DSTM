using PADIDSTM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
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


        }
    }

    public class RemoteMaster : MarshalByRefObject, IMaster
    {
        private Dictionary<string, int> serversLoad = new Dictionary<string, int>();
        private Dictionary<int, string[]> padIntLocation = new Dictionary<int, string[]>();
        private string urlFailed; //URL OF A SERVER THAT IS FAILED
        private int transactionID = 0; //ID THAT WILL BE USED FOR TIE-BREAKER
        private Object tIDLock = new Object();
        private Object urlLock = new Object();
        private Object padIntLocationLock = new Object();
        private Form1 form;
        private delegate bool declareSlaveFailedDelegate(string url);

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
                if (urlServerDest[0] == null) //IF ONE OF THE URL IS NULL IS BECAUSE PADINT DOESNT EXIST
                {
                    urlServerDest = getBestSlaves(2); //FINDS THE TWO LESS LOADED SERVERS
                    padIntLocation.Add(uid, new string[] { "UNDEFINED", "UNDEFINED" }); //ADDS AN ENTRY TO RESERVE THE PADINT. LATER WILL CONFIRM
                   
                    
                }
                else
                {
                    urlServerDest = null;//WILL RETURN NULL FOR CALLER TO KNOW THAT PADINT ALREADY EXISTS
                    
                }
            }
            
            return urlServerDest;
            
        }

       
        //RETURNS THE NUM LESS LOADED SLAVES
        private string[] getBestSlaves(int num)
        {
            String[] url = new String[num];
            var sortedSlaves = (from item in serversLoad
                                orderby item.Value
                                ascending
                                select item);
            int i = 0;
            foreach (KeyValuePair<string, int> item in sortedSlaves)
            {
                ISlave slave = (ISlave)Activator.GetObject(
                                       typeof(ISlave),
                                   item.Key);
                try { 
                    bool res = slave.ping(); //CALL TO KNOW IF SERVER IS AVAILABLE OR NOT
                    url[i] = item.Key;
                    i++;
                    if (i == num)
                        break;
                }
                catch (Exception)
                {
                    declareSlaveFailedDelegate del = new declareSlaveFailedDelegate(declareSlaveFailed); //WILL CALL IN ANOTHER THREAD PROTOCOL TO RECOVER FAILED SERVER
                    IAsyncResult r = del.BeginInvoke(item.Key, null, null);
                   continue;
                }
             }
            return url;

        }

        
        //IF PADINT EXISTS WILL RETURN AN ARRAY OF THE TWO URL WHERE IT IS. IF NOT EXISTS WILL RETURN NULL
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


        //WILL GIVE ALL THE PADINTS THAT BELONG TO THE SLAVE THAT FAILED
        public List<int> recoverSlave()
        {
            List<int> refs = getReferences(urlFailed);
            urlFailed = null;
            return refs;
        }


        public string GetTS()
        {
            int tID;
            lock (tIDLock)
            {
                tID = transactionID++;
            }
            string timestamp = TimeStamp.GetTimestamp(DateTime.Now) + "#" + tID; //TIMESTAMP WITH THE TIE-BREAKER
            return timestamp;
        }

        public bool registerSlave(String url)
        {
            form.Invoke(new delLog(form.log), new Object[] { "Slave Server connecting at: " + url });
            serversLoad.Add(url, 0);
            form.Invoke(new delServerLoad(form.updateServerLoad), new Object[] { this.serversLoad });
            return true;
        }

        //THIS FUNCTION IS CALLED AFTER EACH SLAVE CREATES THE PADINT. USED FOR VERIFY THE CREATION
        public void RegisterNewPadInt(int uid, string serverURL)
        {
            lock (urlLock)
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
            }
            updateForm();
        }

       
        public void callStatusOnSlaves()
        {
            foreach (string slave in serversLoad.Keys)
            {
                ISlave server = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               slave);
                try
                {
                    server.status();
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        //CALLED IN THE CONTEXT OF AN TRANSACTION THAT ABORTED AND WANT TO ABORT THE CREATED PADINTS
        public void removeUID(List<int> UIDsToRemove)
        {
            foreach (int id in UIDsToRemove)
            {
                //WE HAVE ALWAYS TWO PADINTS (BECAUSE OF REPLICATION) BUT WE ONLY DELETE ONE TIME PADINTLOCATION ENTRY
                if (padIntLocation.ContainsKey(id))
                {
                    string[] url = padIntLocation[id];
                    if (url[0] != "COPYING" && url[0] != "UNDEFINED")
                    {
                        if (serversLoad[url[0]] > 0)
                            serversLoad[url[0]]--;
                        ISlave server1 = (ISlave)Activator.GetObject(
                                       typeof(ISlave),
                                   url[0]);
                        server1.removePadInt(id);
                    }
                    if (url[1] != "COPYING" && url[0] != "UNDEFINED")
                    {
                        if (serversLoad[url[1]] > 0)
                            serversLoad[url[1]]--;
                        ISlave server2 = (ISlave)Activator.GetObject(
                                       typeof(ISlave),
                                       url[1]);
                        server2.removePadInt(id);
                    }
                    padIntLocation.Remove(id);
                }
                updateForm();
            }

           

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

        //FINDS A SLAVE TO COPY A PADINT FROM A SERVER FAILED AND THAT EXISTS IN THE SERVER AT urlPadIntAvailable
        private string getSlaveToCopy(string urlPadIntAvailable)
        {
            string url=null;
            var sortedSlaves = (from item in serversLoad
                                orderby item.Value
                                ascending
                                select item);
            foreach (KeyValuePair<string, int> item in sortedSlaves)
            {
                if (item.Key != urlPadIntAvailable && item.Key != urlFailed)
                {
                    url=item.Key;
                    break;
                }
                
            }
            return url;
        }

        //CALLED WHEN IS DETECTED THAT A SERVER IS FAILED
        public bool declareSlaveFailed(string serverUrlFailed)
        {
            if (urlFailed == null) //WE ONLY DECLARE THE SLAVE FAILED ONE TIME FOR THE SAME SLAVE
            {
                urlFailed = serverUrlFailed;
                serversLoad[serverUrlFailed] = int.MaxValue;
                copyDataFromFailedServer(serverUrlFailed);
            }
            return true;

        }

        //WILL COPY ALL THE PADINTS FROM THE FAILED SERVER TO THE NEW ONE AND REFRESH ALL THE NECESSARY STRUCTURES
        private void copyDataFromFailedServer(string serverUrlFailed)
        {
            foreach (KeyValuePair<int, string[]> entry in padIntLocation) 
            {
                
                //IF ITS THE FIRST SLAVE ON URL OF PADINT THAT IS FAILED
                if (entry.Value[0] == serverUrlFailed)
                {
                    entry.Value[0] = "COPYING";
                    string URLWithPadIntAvaliable = entry.Value[1];
                    string newURL = getSlaveToCopy(URLWithPadIntAvaliable);
                    ISlave slaveToCopy = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               URLWithPadIntAvaliable);
                    ISlave slaveToCreate = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               newURL);
                    RemotePadInt availablePadInt = slaveToCopy.access(entry.Key, 0); //SPECIAL CALL WITH TS==0
                    RemotePadInt newPadInt = new RemotePadInt(availablePadInt, newURL);//SPECIAL CONSTRUCT
                    slaveToCreate.addCopyOfPadInt(newPadInt);
                    entry.Value[0] = newURL;
                    serversLoad[newURL]++;
                }
                //IF ITS THE SECOND SLAVE ON URL OF PADINT THAT IS FAILED
                else if (entry.Value[1] == serverUrlFailed)
                {
                    
                    entry.Value[1] = "COPYING";
                    string URLWithPadIntAvaliable = entry.Value[0];
                    string newURL = getSlaveToCopy(URLWithPadIntAvaliable);
                    ISlave slaveToCopy = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               URLWithPadIntAvaliable);
                    ISlave slaveToCreate = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               newURL);
                    RemotePadInt availablePadInt = slaveToCopy.access(entry.Key, 0); //SPECIAL CALL WITH TS==
                    RemotePadInt newPadInt = new RemotePadInt(availablePadInt, newURL);//SPECIAL CONSTRUCT
                    slaveToCreate.addCopyOfPadInt(newPadInt);
                    entry.Value[1] = newURL;
                    serversLoad[newURL]++;
                }


            }
            //AFTER RECOVERING WILL UPDATE THE URL OF FAILED SERVER AND REMOVE THE FAILED SERVER FROM THE LIST OF SLAVES
            urlFailed = null;
            serversLoad.Remove(serverUrlFailed);
            updateForm();


        }

        //WILL RETURN ALL THE PADINTS UID THAT EXIST ON SERVER WITH URL:url
        public List<int> getReferences(string url){
            List<int> references= new List<int>();
            foreach(KeyValuePair<int,string[]> rpi in padIntLocation){
                if(rpi.Value[0]==url || rpi.Value[1]==url)
                    references.Add(rpi.Key);
            }
            return references;
        }

        //WILL UPDATE LOAD OF THE SERVER WITH THE NEW VALUE
        public bool updateLoad(string slaveUrl, int load)
        {
            serversLoad[slaveUrl] = load;
            return true;
        }


        public int getLoad(string slaveUrl)
        {
            return serversLoad[slaveUrl];
        }

        
    }

        //CLASS USED TO GENERATE TIMESTAMPS
        public static class TimeStamp
        {
            public static String GetTimestamp(this DateTime value)
            {
                return value.ToString("yyyyMMddHHmmssffff");
            }
        }

    } 

