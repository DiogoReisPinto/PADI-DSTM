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
        private string urlFailed;
        private Dictionary<RemotePadInt, List<long>> transactionsToAbort = new Dictionary<RemotePadInt, List<long>>();
        private List<int> padIntsToRemoveFromFailed = new List<int>();
        private int transactionID = 0;
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
                if (urlServerDest[0] == null)
                {
                    urlServerDest = getBestSlaves(2);
                    padIntLocation.Add(uid, new string[] { "UNDEFINED", "UNDEFINED" });
                   
                    
                }
                else
                {
                    urlServerDest = null;
                    
                }
            }
            
            return urlServerDest;
            
        }

       

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
                    bool res = slave.ping();
                    url[i] = item.Key;
                    i++;
                    if (i == num)
                        break;
                }
                catch (Exception)
                {
                    declareSlaveFailedDelegate del = new declareSlaveFailedDelegate(declareSlaveFailed);
                    IAsyncResult r = del.BeginInvoke(item.Key, null, null);
                   continue;
                }
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

        public List<int> recoverSlave()
        {
            List<int> refs = getReferences(urlFailed);
            urlFailed = null;
            return refs;
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

        public void addTransactionToAbort(RemotePadInt rpi, long ts)
        {
            if (transactionsToAbort.ContainsKey(rpi))
            {
                transactionsToAbort[rpi].Add(ts);

            }
            else
            {
                transactionsToAbort.Add(rpi, new List<long>());
                transactionsToAbort[rpi].Add(ts);

            }


        }

        public void addPadIntToRemoveFromFailed(int uid)
        {
            padIntsToRemoveFromFailed.Add(uid);
        }

        public void removeUID(List<int> UIDsToRemove)
        {
            foreach (int id in UIDsToRemove)
            {
                //SÃO ADICIONADOS OS 2 REMOTE PADINTS A REMOVER MAS APENAS SAO APAGADOS UMA VEZ
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

        public bool declareSlaveFailed(string serverUrlFailed)
        {
            if (urlFailed == null)
            {
                urlFailed = serverUrlFailed;
                serversLoad[serverUrlFailed] = int.MaxValue;
                copyDataFromFailedServer(serverUrlFailed);
            }
            return true;

        }

        private void copyDataFromFailedServer(string serverUrlFailed)
        {
            foreach (KeyValuePair<int, string[]> entry in padIntLocation)
            {
                //CASO EM QUE E O PRIMEIRO URL QUE ESTA DOWN

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
                    //Adicionar o PADInt para remover depois de o servidor ser declarado como morto
                    addPadIntToRemoveFromFailed(entry.Key);
                    RemotePadInt availablePadInt = slaveToCopy.access(entry.Key, 0);
                    RemotePadInt newPadInt = new RemotePadInt(availablePadInt, newURL);
                    slaveToCreate.addCopyOfPadInt(newPadInt);
                    entry.Value[0] = newURL;
                    serversLoad[newURL]++;
                }
                //CASO EM QUE E O SEGUNDO A ESTAR DOWN
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
                    //Adicionar o PADInt para remover depois de o servidor ser declarado como morto
                    addPadIntToRemoveFromFailed(entry.Key);
                    RemotePadInt availablePadInt = slaveToCopy.access(entry.Key, 0);
                    RemotePadInt newPadInt = new RemotePadInt(availablePadInt, newURL);
                    slaveToCreate.addCopyOfPadInt(newPadInt);
                    entry.Value[1] = newURL;
                    serversLoad[newURL]++;
                }


            }
            urlFailed = null;
            serversLoad.Remove(serverUrlFailed);
            updateForm();


        }

        public List<int> getReferences(string url){
            List<int> references= new List<int>();
            foreach(KeyValuePair<int,string[]> rpi in padIntLocation){
                if(rpi.Value[0]==url || rpi.Value[1]==url)
                    references.Add(rpi.Key);
            }
            return references;
        }


        public void printSomeShit(string toPrint)
        {
            Console.WriteLine(toPrint);
        }

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




        public static class TimeStamp
        {
            public static String GetTimestamp(this DateTime value)
            {
                return value.ToString("yyyyMMddHHmmssffff");
            }
        }

    } 

