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
        public static string transactionTS; //TRANSACTION TIMESTAMP WITH TIEBREAKER
        public static long tsValue; //TIMESTAMP VALUE WITHOUT TIEBREAKER
        public static Dictionary<RemotePadInt,string> visitedPadInts;
        public static Dictionary<RemotePadInt,string> createdPadInts;

        //DELEGATES USED FOR ASYNC CALLS:ACCESSES, PREPARES AND TO DECLARE A SLAVE FAILED RESPECTIVELY
        public delegate RemotePadInt RemoteAccessCreateDelegate(int uid,long ts);
        public delegate int callPrepareDelegate(long ts);
        public delegate bool declareSlaveFailedDelegate(string url);

        

        public static bool Init() {
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = 0; //FINDS AUTOMATICALLY A PORT AVAILABLE
            props["timeout"] = 5000; // TIMEOUT FOR THE CLIENT
            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);
            masterServ = (IMaster)Activator.GetObject(
                                    typeof(IMaster),
                                "tcp://localhost:8086/RemoteMaster");


            return true;
            
        }

        public static bool TxBegin() {
            string timeStamp = masterServ.GetTS();
            transactionTS = timeStamp;
            tsValue = Convert.ToInt64(transactionTS.Split('#')[0]);
            visitedPadInts = new Dictionary<RemotePadInt,string>();
            createdPadInts = new Dictionary<RemotePadInt, string>();
            return true;
        }

        public static bool TxCommit()
        {
            int expectedVotes = visitedPadInts.Count + createdPadInts.Count;
            int votes = 0;
            IAsyncResult[] r = new IAsyncResult[expectedVotes];
            callPrepareDelegate[] callsForCommit = new callPrepareDelegate[expectedVotes];
            KeyValuePair<RemotePadInt,string>[] PadIntsInTransaction = new KeyValuePair<RemotePadInt,string>[expectedVotes];
            int i=0;
            foreach (KeyValuePair<RemotePadInt, string> entry in visitedPadInts)
            {
               callsForCommit[i] = new callPrepareDelegate(entry.Key.prepareCommitTx);
               PadIntsInTransaction[i] = entry;
               r[i] = callsForCommit[i].BeginInvoke(tsValue, null, null);
               i++;
            }
            foreach (KeyValuePair<RemotePadInt, string> entry in createdPadInts)
            {
                callsForCommit[i] = new callPrepareDelegate(entry.Key.prepareCommitPadInt);
                PadIntsInTransaction[i] = entry;
                r[i] = callsForCommit[i].BeginInvoke(tsValue, null, null);
                i++;        
            }

            //FIRST PHASE OF 2PC PROTOCOL - WAITING FOR THE VOTES
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
            //SECOND PHASE OF 2PC - IF ALL VOTED YES WILL COMMIT OTHERWHISE ABORT
            if (votes == expectedVotes){
                votes = 0; //ACKS RECEIVED
                IAsyncResult[] r2 = new IAsyncResult[expectedVotes];
                callPrepareDelegate[] callsForCommit2 = new callPrepareDelegate[expectedVotes];
                KeyValuePair<RemotePadInt,string>[] PadIntsInTransaction2 = new KeyValuePair<RemotePadInt,string>[expectedVotes];
                int j=0;
                foreach (KeyValuePair<RemotePadInt, string> entry in visitedPadInts)
                {
                    callsForCommit2[j] = new callPrepareDelegate(entry.Key.commitTx);
                    PadIntsInTransaction2[j] = entry;
                    r2[j] = callsForCommit2[j].BeginInvoke(tsValue, null, null);
                    j++;
                }
                foreach (KeyValuePair<RemotePadInt, string> entry in createdPadInts)
                {
                    callsForCommit2[j] = new callPrepareDelegate(entry.Key.commitPadInt);
                    PadIntsInTransaction2[j] = entry;
                    r2[j] = callsForCommit2[j].BeginInvoke(tsValue, null, null);
                    j++;        
                }
            //COUNTING ACKS FOR THE SECOND PHASE OF 2PC
            for (int k = 0; k < expectedVotes; k++)
            {
                try
                {
                    votes += callsForCommit2[k].EndInvoke(r2[k]);
                }
                catch (Exception)
                {
                    TxAbort();
                    return false;
                }
            }
                
                //IF ALL ACKS RECEIVED
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
            List<int> UIDsToRemove = new List<int>(); //LIST OF UID THAT WERE CREATED AND NEED TO BE REMOVED
            int expectedVotes = visitedPadInts.Count + createdPadInts.Count;
            int votes = 0; 
            IAsyncResult[] r = new IAsyncResult[expectedVotes];
            callPrepareDelegate[] callsForAbort = new callPrepareDelegate[visitedPadInts.Count];
            KeyValuePair<RemotePadInt, string>[] PadIntsInTransaction = new KeyValuePair<RemotePadInt, string>[visitedPadInts.Count]; //LIST OF VISITED PADINT OF TRANSACTION
            int i = 0;
            foreach (KeyValuePair<RemotePadInt, string> entry in visitedPadInts)
            {
                callsForAbort[i] = new callPrepareDelegate(entry.Key.abortTx);
                PadIntsInTransaction[i] = entry;
                r[i] = callsForAbort[i].BeginInvoke(tsValue, null, null);
                i++;
            }
            //ADDS PADINT UID TO REMOVE ON THE CONTEXT OF THE TRANSACTION ABORT
            foreach (KeyValuePair<RemotePadInt, string> entry in createdPadInts)
            {
                try
                {
                    UIDsToRemove.Add(entry.Key.getUID());
                    votes++;
                }
                catch (Exception)
                {
                    masterServ.declareSlaveFailed(entry.Value);
                }
            }
            //WILL WAIT FOR ALL THE VISITEDPADINTS TO ABORT
            for (int j = 0; j < visitedPadInts.Count; j++)
            {
                try
                {
                    votes += callsForAbort[j].EndInvoke(r[j]);
                }
                catch (Exception)
                {
                    masterServ.declareSlaveFailed(PadIntsInTransaction[j].Value);
                }
            }
            masterServ.removeUID(UIDsToRemove); //MAKES THE CALL TO REMOVE CREATED PADINTS
            if (votes == expectedVotes) //CHECKS IF ALL WENT WELL ON THE ABORT OPERATION
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
            PadInt newPad = new PadInt(RPadInts[0].uid); //WILL CREATE WITH UID OF ONE OF THE CREATED PADINTS
            //WILL ADD BOTH CREATED PADINTS TO CREATEDPADINTS LIST FOR COMMIT/ABORT OPERATIONS
            createdPadInts.Add(RPadInts[0], RPadInts[0].url);
            createdPadInts.Add(RPadInts[1], RPadInts[1].url);
            return newPad;
        }

        public static RemotePadInt[] CreateRemotePadInt(int uid){
            string[] url = new String[2];
            RemotePadInt[] createdRemotePadInt = new RemotePadInt[2];
            url = masterServ.GetLocationNewPadInt(uid); //CHECKS IF PADINT ALREADY EXISTS
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
            //WILL MAKE ASYNCHRONOUS CALLS TO THE TWO SLAVES FOR CREATING PADINT
            RemoteAccessCreateDelegate RemoteDel1 = new RemoteAccessCreateDelegate(slave1.create);
            // Call remote method
            r1 = RemoteDel1.BeginInvoke(uid, tsValue, null, null);


            RemoteAccessCreateDelegate RemoteDel2 = new RemoteAccessCreateDelegate(slave2.create);
            // Call remote method
            r2 = RemoteDel2.BeginInvoke(uid, tsValue, null, null);

            //WILL WAIT FOR THE CALLS TO END AND IF EXCEPTION THROWN WILL DECLARE THE SLAVE FAILED

            try
            {
                createdRemotePadInt[0] = RemoteDel1.EndInvoke(r1);
            }
            catch (Exception)
            {
                declareSlaveFailedDelegate del = new declareSlaveFailedDelegate(masterServ.declareSlaveFailed);
                IAsyncResult res = del.BeginInvoke(url[0], null, null);
                return null;
            }
            try
            {
                createdRemotePadInt[1] = RemoteDel2.EndInvoke(r2);
            }
            catch (Exception)
            {
                declareSlaveFailedDelegate del = new declareSlaveFailedDelegate(masterServ.declareSlaveFailed);
                IAsyncResult res = del.BeginInvoke(url[1], null, null);
                return null;
            }
            return createdRemotePadInt;
        }

        public static PadInt AccessPadInt(int uid) {
            RemotePadInt[] newRemotePadInt = AccessRemotePadInt(uid);
            if (newRemotePadInt[0] == null || newRemotePadInt[1]==null) //IF THERE WERE A PROBLEM ACCESSING PADINTS
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
            if (url[0] == "COPYING" || url[1] == "COPYING") //CASE THAT A PADINT IS GOING TO BE RECOVERED
            {
                Thread.Sleep(1000); //WAITS A SECOND FOR THE PADINT TO BE RECOVERED AND WILL TRY AGAIN TO ACCESS
                remotePadInts = AccessRemotePadInt(uid);
                return remotePadInts;
            }
            if (url[0] == null || url[1] == null || url[0] == "UNDEFINED" || url[1] == "UNDEFINED") //CASE THAT PADINT NOT EXISTS
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

            //WILL MAKE ASYNC CALLS TO ACCESS ON EACH SLAVE SERVER
            RemoteAccessCreateDelegate RemoteDel1 = new RemoteAccessCreateDelegate(slave1.access);
            r1 = RemoteDel1.BeginInvoke(uid,tsValue,null,null);
            RemoteAccessCreateDelegate RemoteDel2 = new RemoteAccessCreateDelegate(slave2.access);
            r2 = RemoteDel2.BeginInvoke(uid, tsValue, null, null);
            
            //WILL WAIT FOR THE CALLS TO END
            try
                {
                    remotePadInts[0] = RemoteDel1.EndInvoke(r1);
                }
                catch (Exception)
                {
                    masterServ.declareSlaveFailed(url[0]); //DECLARES THE SLAVE FAILED AND RETRY TO ACCESS
                    remotePadInts = AccessRemotePadInt(uid);
                    return remotePadInts;
                }
            try
                {
                    remotePadInts[1] = RemoteDel2.EndInvoke(r2);
                }
                catch (Exception)
                {
                    masterServ.declareSlaveFailed(url[1]);//DECLARES THE SLAVE FAILED AND RETRY TO ACCESS
                    remotePadInts = AccessRemotePadInt(uid);
                    return remotePadInts;
                }
            return remotePadInts;
        }

        
    }
}
