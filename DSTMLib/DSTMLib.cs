using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PADIDSTM;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Collections;
using System.Runtime.Serialization.Formatters;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Runtime.Remoting.Messaging;

namespace PADIDSTM
{
    public class DSTMLib
    {

        public static IMaster masterServ;

        public static string transactionTS;
        public static long tsValue;
        public static Dictionary<RemotePadInt,string> visitedPadInts;
        public static Dictionary<RemotePadInt,string> createdPadInts;

        //Variable used for accessing PadInts
        public static RemotePadInt[] acessingPadInts;
        public static int votes;
        private static Object votesLock = new Object();
        private static AutoResetEvent[] handles;
        private static int handleIndex;
        

        //Delegate for calling acess PadInts
        public delegate RemotePadInt RemoteAsyncDelegate(int uid,long ts);
        public delegate int callPrepareCommitDelegate(long ts);

        
        //CallBack for AcessingPadInts operation
        public static void OurRemoteAsyncCallBack(IAsyncResult ar)
        {
            RemoteAsyncDelegate del = (RemoteAsyncDelegate)((AsyncResult)ar).AsyncDelegate;
            lock (votesLock)
            {
                if (acessingPadInts[0] == null)
                    acessingPadInts[0] = del.EndInvoke(ar);
                else
                    acessingPadInts[1] = del.EndInvoke(ar);

                handles[handleIndex].Set();
                handleIndex++;
            }
            return;
        }

        


        public static bool Init() {
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = 0;
            props["timeout"] = 10000; // in milliseconds
            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);
            masterServ = (IMaster)Activator.GetObject(
                                    typeof(IMaster),
                                "tcp://localhost:8086/RemoteMaster");


            return true;
            
        }

        public static bool TxBegin() {
            int tID = masterServ.getTransactionID();
            string timeStamp = masterServ.GetTS(tID);
            transactionTS = timeStamp;
            tsValue = Convert.ToInt64(transactionTS.Split('#')[0]);
            visitedPadInts = new Dictionary<RemotePadInt,string>();
            createdPadInts = new Dictionary<RemotePadInt, string>();
            acessingPadInts = new RemotePadInt[2];
            Console.WriteLine("IN DSTMlib: "+transactionTS);
            return true;
        }

        public static bool TxCommit()
        {
            int expectedVotes = visitedPadInts.Count + createdPadInts.Count;
            votes = 0;
            IAsyncResult[] r = new IAsyncResult[expectedVotes];
            callPrepareCommitDelegate[] callsForCommit = new callPrepareCommitDelegate[expectedVotes];
            KeyValuePair<RemotePadInt,string>[] PadIntsInTransaction = new KeyValuePair<RemotePadInt,string>[expectedVotes];
            int i=0;
            foreach (KeyValuePair<RemotePadInt, string> entry in visitedPadInts)
            {
               callsForCommit[i] = new callPrepareCommitDelegate(entry.Key.prepareCommitTx);
               PadIntsInTransaction[i] = entry;
               r[i] = callsForCommit[i].BeginInvoke(tsValue, null, null);
               i++;
            }
            foreach (KeyValuePair<RemotePadInt, string> entry in createdPadInts)
            {
                callsForCommit[i] = new callPrepareCommitDelegate(entry.Key.prepareCommitPadInt);
                PadIntsInTransaction[i] = entry;
                r[i] = callsForCommit[i].BeginInvoke(tsValue, null, null);
                i++;        
            }

            for (int j = 0; j < expectedVotes; j++)
            {
                try
                {
                    votes += callsForCommit[j].EndInvoke(r[j]);
                }
                catch (Exception)
                {
                    TxAbort();
                    //masterServ.declareSlaveFailed(PadIntsInTransaction[j].Value);
                    //if (i >= visitedPadInts.Count)
                    //    masterServ.addPadIntToRemoveFromFailed(PadIntsInTransaction[j].Key.uid);
                    return false;
                }
            }
                //COMMIT MESSAGES FOR COMMITING ON SECOND PHASE 2PC
            if (votes == expectedVotes){
                votes = 0;
                IAsyncResult[] r2 = new IAsyncResult[expectedVotes];
                callPrepareCommitDelegate[] callsForCommit2 = new callPrepareCommitDelegate[expectedVotes];
                KeyValuePair<RemotePadInt,string>[] PadIntsInTransaction2 = new KeyValuePair<RemotePadInt,string>[expectedVotes];
                int j=0;
                foreach (KeyValuePair<RemotePadInt, string> entry in visitedPadInts)
                {
                    callsForCommit2[j] = new callPrepareCommitDelegate(entry.Key.commitTx);
                    PadIntsInTransaction2[j] = entry;
                    r2[j] = callsForCommit2[j].BeginInvoke(tsValue, null, null);
                    j++;
                }
                foreach (KeyValuePair<RemotePadInt, string> entry in createdPadInts)
                {
                    callsForCommit2[j] = new callPrepareCommitDelegate(entry.Key.prepareCommitPadInt);
                    PadIntsInTransaction2[j] = entry;
                    r2[j] = callsForCommit2[j].BeginInvoke(tsValue, null, null);
                    j++;        
                }

            for (int k = 0; k < expectedVotes; k++)
            {
                try
                {
                    votes += callsForCommit2[k].EndInvoke(r2[k]);
                }
                catch (Exception)
                {
                    TxAbort();
                }
            }
                
           
                if (votes == expectedVotes)
                    return true;
                else
                {
                    throw new TxException("TRANSACTION FAILED DURING ACK PHASE");

                }
            }
            else
            {
                TxAbort();
                return false;
            }

        }


        public static bool TxAbort()
        {
            List<int> UIDsToRemove = new List<int>();
            int expectedVotes = visitedPadInts.Count + createdPadInts.Count;
            votes = 0;
            IAsyncResult[] r = new IAsyncResult[expectedVotes];
            callPrepareCommitDelegate[] callsForCommit = new callPrepareCommitDelegate[visitedPadInts.Count];
            KeyValuePair<RemotePadInt, string>[] PadIntsInTransaction = new KeyValuePair<RemotePadInt, string>[visitedPadInts.Count];
            int i = 0;
            foreach (KeyValuePair<RemotePadInt, string> entry in visitedPadInts)
            {
                callsForCommit[i] = new callPrepareCommitDelegate(entry.Key.abortTx);
                PadIntsInTransaction[i] = entry;
                r[i] = callsForCommit[i].BeginInvoke(tsValue, null, null);
                i++;
            }
            foreach (KeyValuePair<RemotePadInt, string> entry in createdPadInts)
            {
                try
                {
                    UIDsToRemove.Add(entry.Key.getUID());
                    votes++;
                }
                catch (Exception)
                {
                    //masterServ.addPadIntToRemoveFromFailed(entry.Key.uid);
                    masterServ.declareSlaveFailed(entry.Value);
                }
            }

            for (int j = 0; j < visitedPadInts.Count; j++)
            {
                try
                {
                    votes += callsForCommit[j].EndInvoke(r[j]);
                }
                catch (Exception)
                {
                    masterServ.addTransactionToAbort(PadIntsInTransaction[i].Key, tsValue);
                    masterServ.declareSlaveFailed(PadIntsInTransaction[i].Value);
                }
            }
            masterServ.removeUID(UIDsToRemove);
            if (votes == expectedVotes)
                return true;
            else
                return false;
        }

        public static bool Status() { 
            masterServ.callStatusOnSlaves();
            return true;
        }

        public static bool Fail(string url)
        {
            ISlave slave = (ISlave)Activator.GetObject(
                typeof(ISlave),
                url);
            slave.fail();
            return true;
        }

        public static bool Freeze(string url) {
            ISlave slave = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url);
            slave.freeze();
            return true;
        }

        public static bool Recover(string url)
        {
            ISlave slave = (ISlave)Activator.GetObject(
                  typeof(ISlave),
                  url);
            slave.recover();
            return true;  
        }

        public static PadInt CreatePadInt(int uid) {
            RemotePadInt[] RPadInts = CreateRemotePadInt(uid);
            if (RPadInts == null)
                return null;
            PadInt newPad = new PadInt(RPadInts[0].uid);
            createdPadInts.Add(RPadInts[0], RPadInts[0].url);
            createdPadInts.Add(RPadInts[1], RPadInts[1].url);
            return newPad;
        }

        public static RemotePadInt[] CreateRemotePadInt(int uid){
            masterServ.printSomeShit("Enter Create RemotePadInt with uid: " + uid);
            acessingPadInts[0] = null;
            acessingPadInts[1] = null;
            string[] url = new String[2];
            handles = new AutoResetEvent[2];
            handles[0] = new AutoResetEvent(false);
            handles[1] = new AutoResetEvent(false);
            handleIndex = 0;
            url = masterServ.GetLocationNewPadInt(uid);
            if (url == null)
            {
                masterServ.printSomeShit("GetLocationNewPadInt returned null indicating that PadInt with id already exists:" + uid);
                return null;
            }
            ISlave slave1 = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url[0]);
            ISlave slave2 = (ISlave)Activator.GetObject(
                                  typeof(ISlave),
                              url[1]);
            IAsyncResult r1 = null;
            IAsyncResult r2 = null;
            try
            {
                RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(slave1.create);
                // Create delegate to local callback
                AsyncCallback RemoteCallback = new AsyncCallback(DSTMLib.OurRemoteAsyncCallBack);
                // Call remote method
                r1 = RemoteDel.BeginInvoke(uid, tsValue, RemoteCallback, null);
            }
            catch (SocketException)
            {
                masterServ.printSomeShit("Declared that server with url:" + url[0] + "is unavailable on creating PadInt with ID:" + uid);
                bool res = masterServ.declareSlaveFailed(url[0]);
                return null;
            }
            catch (IOException)
            {
                masterServ.printSomeShit("Declared that server with url:" + url[0] + "is unavailable on creating PadInt with ID:" + uid);
                bool res = masterServ.declareSlaveFailed(url[0]);
                return null;
            }
            try
            {
                RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(slave2.create);
                // Create delegate to local callback
                AsyncCallback RemoteCallback = new AsyncCallback(DSTMLib.OurRemoteAsyncCallBack);
                // Call remote method
                r2 = RemoteDel.BeginInvoke(uid, tsValue, RemoteCallback, null);
            }
            catch (SocketException)
            {
                masterServ.printSomeShit("Declared that server with url:" + url[1] + "is unavailable on creating PadInt with ID:" + uid);
                bool res = masterServ.declareSlaveFailed(url[1]);
                return null;
            }
            catch (IOException)
            {
                masterServ.printSomeShit("Declared that server with url:" + url[1] + "is unavailable on creating PadInt with ID:" + uid);
                bool res = masterServ.declareSlaveFailed(url[1]);
                return null;
            }
            WaitHandle.WaitAll(handles);
            return acessingPadInts;
        }


        public static PadInt AccessPadInt(int uid) {
            RemotePadInt[] newRemotePadInt = AccessRemotePadInt(uid);
            if (newRemotePadInt[0] == null || newRemotePadInt[1]==null)
            {
                return null;
            }
            PadInt newPad = new PadInt(newRemotePadInt[0].uid);
            return newPad;
        }

        public static int getServerLoad(string url)
        {
            int load = masterServ.getLoad(url);
            return load;
        }

        

        public static RemotePadInt[] AccessRemotePadInt(int uid) {
            acessingPadInts[0] = null;
            acessingPadInts[1] = null;
            string[] url = masterServ.DiscoverPadInt(uid);
            RemotePadInt[] remotePadInts = new RemotePadInt[2];
            handles = new AutoResetEvent[2];
            handleIndex = 0;
            handles[0] = new AutoResetEvent(false);
            handles[1] = new AutoResetEvent(false);
            if (url[0] == null || url[1] == null || url[0] == "UNDEFINED" || url[1] == "UNDEFINED")
            {
                return remotePadInts;


            }
            ISlave slave1 = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url[0]);
            ISlave slave2 = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url[1]);

            IAsyncResult r1 = null;
            IAsyncResult r2 = null;
            try
            {
                RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(slave1.access);
                // Create delegate to local callback
                AsyncCallback RemoteCallback = new AsyncCallback(DSTMLib.OurRemoteAsyncCallBack);
                // Call remote method
                r1 = RemoteDel.BeginInvoke(uid,tsValue,RemoteCallback,null);
                //remotePadInts[0] = slave1.access(uid, tsValue);
            }
            catch (SocketException)
            {
                bool res = masterServ.declareSlaveFailed(url[0]);
                //Makes Second attemp to access padInt
                RemotePadInt[] rpi = AccessRemotePadInt(uid);

                return rpi;
            }
            catch (IOException)
            {
                bool res = masterServ.declareSlaveFailed(url[0]);
                //Makes Second attemp to access padInt
                RemotePadInt[] rpi = AccessRemotePadInt(uid);
                return rpi;
            }
            try
            {
                RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(slave1.access);
                // Create delegate to local callback
                AsyncCallback RemoteCallback = new AsyncCallback(DSTMLib.OurRemoteAsyncCallBack);
                // Call remote method
                r2 = RemoteDel.BeginInvoke(uid, tsValue, RemoteCallback, null);
                //remotePadInts[1] = slave2.access(uid, tsValue);
            }
            catch (SocketException)
            {
                masterServ.declareSlaveFailed(url[1]);
                //Makes Second attemp to access padInt
                RemotePadInt[] rpi = AccessRemotePadInt(uid);
                return rpi;
            }
            catch (IOException)
            {
                
                masterServ.declareSlaveFailed(url[1]);
                //Makes Second attemp to access padInt
                RemotePadInt[] rpi = AccessRemotePadInt(uid);
                return rpi;
            }
            WaitHandle.WaitAll(handles);
            return acessingPadInts;
        }

        
    }
}
