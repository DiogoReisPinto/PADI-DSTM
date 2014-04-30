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

namespace PADIDSTM
{
    public class DSTMLib
    {

        public static IMaster masterServ;
        public static string transactionTS;
        public static long tsValue;
        public static Dictionary<RemotePadInt,string> visitedPadInts;
        public static Dictionary<RemotePadInt,string> createdPadInts;

        public static bool Init() {

            TcpChannel channel = new TcpChannel(0);
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
            //PREPARE MESSAGES FOR COMMITING ON FIRST PHASE 2PC
            
            foreach (KeyValuePair<RemotePadInt, string> entry in visitedPadInts)
            {
               try{
                   votes = votes +  entry.Key.prepareCommitTx(tsValue);
               }
               catch (SocketException){
                   masterServ.declareSlaveFailed(entry.Value);
               }
            }
            foreach (KeyValuePair<RemotePadInt, string> entry in createdPadInts)
            {
                    try{
                        votes = votes + entry.Key.prepareCommitPadInt(tsValue);
                    }
                    catch (SocketException){
                        masterServ.declareSlaveFailed(entry.Value);
                    }
                    catch (IOException) {
                        masterServ.declareSlaveFailed(entry.Value);
                    }
                }
           
           
            
            //COMMIT MESSAGES FOR COMMITING ON SECOND PHASE 2PC
            int acks = 0;
            if (votes == expectedVotes)
            {
                foreach (KeyValuePair<RemotePadInt, string> entry in visitedPadInts)
                {
                    acks += entry.Key.commitTx(tsValue);
                }
                foreach (KeyValuePair<RemotePadInt, string> entry in createdPadInts)
                {
                    acks += entry.Key.commitPadInt(tsValue);
                }
                if (acks == expectedVotes)
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

        public static bool TxAbort() {
            List<int> UIDsToRemove = new List<int>();
            int acks = 0;
            int expectedAcks= visitedPadInts.Count + createdPadInts.Count;
            foreach (KeyValuePair<RemotePadInt, string> entry in visitedPadInts)
            {
                try
                {
                    acks += entry.Key.abortTx(tsValue);
                }
                catch (SocketException)
                {
                    masterServ.declareSlaveFailed(entry.Value);
                }
            }
            foreach (KeyValuePair<RemotePadInt, string> entry in createdPadInts)
            {
                try
                {
                    UIDsToRemove.Add(entry.Key.uid);
                    acks++;
                }
                catch (SocketException)
                {
                    masterServ.declareSlaveFailed(entry.Value);
                }
            }
            masterServ.removeUID(UIDsToRemove);
            if (acks == expectedAcks)
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
            string[] url = masterServ.GetLocationNewPadInt(uid);
            if (url == null)
                return null;
            ISlave slave1 = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url[0]);
            ISlave slave2 = (ISlave)Activator.GetObject(
                                  typeof(ISlave),
                              url[1]); 
            RemotePadInt newRemotePadInt1 = slave1.create(uid,tsValue);
            RemotePadInt newRemotePadInt2 = slave2.create(uid, tsValue);
            PadInt newPad = new PadInt(newRemotePadInt1.uid);
            createdPadInts.Add(newRemotePadInt1,url[0]);
            createdPadInts.Add(newRemotePadInt2,url[1]);
            return newPad;
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


        public static RemotePadInt[] AccessRemotePadInt(int uid) {
            string[] url = masterServ.DiscoverPadInt(uid);
            RemotePadInt[] remotePadInts = new RemotePadInt[2];
            if (url == null|| url[0]=="UNDEFINED" || url[1]=="UNDEFINED")
                return null;
            ISlave slave1 = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url[0]);
            ISlave slave2 = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url[1]);

            
            try
            {
                remotePadInts[0] = slave1.access(uid, tsValue);
            }
            catch (SocketException)
            {
                masterServ.declareSlaveFailed(url[0]);
            }
            try
            {
                remotePadInts[1] = slave2.access(uid, tsValue);
            }
            catch (SocketException)
            {
                masterServ.declareSlaveFailed(url[1]);
            }
            return remotePadInts;
        }

        
    }
}
