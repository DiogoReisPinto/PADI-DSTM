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

        //Delegate for calling acess PadInts
        public delegate RemotePadInt RemoteAsyncDelegate(int uid,long ts);
        public delegate int callPrepareCommitDelegate(long ts);

        

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
            Console.WriteLine("IN DSTMlib: "+transactionTS);
            return true;
        }

        public static bool TxCommit()
        {
            int expectedVotes = visitedPadInts.Count + createdPadInts.Count;
            int votes = 0;
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
                    callsForCommit2[j] = new callPrepareCommitDelegate(entry.Key.commitPadInt);
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
            int votes = 0;
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
            string[] url = new String[2];
            RemotePadInt[] createdRemotePadInt = new RemotePadInt[2];
            url = masterServ.GetLocationNewPadInt(uid);
            if (url == null)
            {
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

            RemoteAsyncDelegate RemoteDel1 = new RemoteAsyncDelegate(slave1.create);
            // Call remote method
            r1 = RemoteDel1.BeginInvoke(uid, tsValue, null, null);


            RemoteAsyncDelegate RemoteDel2 = new RemoteAsyncDelegate(slave2.create);
            // Call remote method
            r2 = RemoteDel2.BeginInvoke(uid, tsValue, null, null);
            //remotePadInts[1] = slave2.access(uid, tsValue);
            try
            {
                createdRemotePadInt[0] = RemoteDel1.EndInvoke(r1);
            }
            catch (Exception)
            {
                masterServ.declareSlaveFailed(url[0]);
                return null;
            }
            try
            {
                createdRemotePadInt[1] = RemoteDel2.EndInvoke(r2);
            }
            catch (Exception)
            {
                masterServ.declareSlaveFailed(url[1]);
                return null;
            }
            return createdRemotePadInt;
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
            string[] url = masterServ.DiscoverPadInt(uid);
            RemotePadInt[] remotePadInts = new RemotePadInt[2];
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
           
                RemoteAsyncDelegate RemoteDel1 = new RemoteAsyncDelegate(slave1.access);
                // Call remote method
                r1 = RemoteDel1.BeginInvoke(uid,tsValue,null,null);
                
           
                RemoteAsyncDelegate RemoteDel2 = new RemoteAsyncDelegate(slave2.access);
                // Call remote method
                r2 = RemoteDel2.BeginInvoke(uid, tsValue, null, null);
                //remotePadInts[1] = slave2.access(uid, tsValue);
                try
                {
                    remotePadInts[0] = RemoteDel1.EndInvoke(r1);
                }
                catch (Exception)
                {
                    masterServ.declareSlaveFailed(url[0]);
                    Thread.Sleep(3000);
                    remotePadInts = AccessRemotePadInt(uid);
                    return remotePadInts;
                }
                try
                {
                    remotePadInts[1] = RemoteDel2.EndInvoke(r2);
                }
                catch (Exception)
                {
                    masterServ.declareSlaveFailed(url[1]);
                    Thread.Sleep(3000);
                    remotePadInts = AccessRemotePadInt(uid);
                    return remotePadInts;
                }
                masterServ.printSomeShit(remotePadInts[1].url + remotePadInts[0].url);
            return remotePadInts;
        }

        
    }
}
