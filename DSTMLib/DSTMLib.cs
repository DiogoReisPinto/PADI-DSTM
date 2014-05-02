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
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            IDictionary props = new Hashtable();
            props["port"] = 0;
            props["timeout"] = 4000; // in milliseconds
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
            masterServ.printSomeShit("Expected Acks:" + Convert.ToString(expectedVotes));
            int votes = 0;
            //PREPARE MESSAGES FOR COMMITING ON FIRST PHASE 2PC
            foreach (KeyValuePair<RemotePadInt, string> entry in visitedPadInts)
            {
               try{
                   votes = votes +  entry.Key.prepareCommitTx(tsValue);
                   
               }
               catch (SocketException){
                   masterServ.declareSlaveFailed(entry.Value);
                   //Make another try to commit transaction
                   TxCommit();
               }
            }
            foreach (KeyValuePair<RemotePadInt, string> entry in createdPadInts)
            {
                    try{
                        votes = votes + entry.Key.prepareCommitPadInt(tsValue);
                        masterServ.printSomeShit("Votes Aquired:" + Convert.ToString(votes));
                    }
                    catch (SocketException){
                        masterServ.declareSlaveFailed(entry.Value);
                        //Make another attemp to commit transaction
                        TxAbort();
                        return false;
                    }
                    catch (IOException) {
                        masterServ.declareSlaveFailed(entry.Value);
                        //Make another attemp to commit transaction
                        TxAbort();
                        return false;
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
            masterServ.printSomeShit("Entrei no Abort");
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
                    //TxAbort();
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
            RemotePadInt[] RPadInts = CreateRemotePadInt(uid);
            PadInt newPad = new PadInt(RPadInts[0].uid);
            createdPadInts.Add(RPadInts[0], url[0]);
            createdPadInts.Add(RPadInts[1], url[1]);
            return newPad;
        }

        public static RemotePadInt[] CreateRemotePadInt(int uid){
            string[] url = masterServ.GetLocationNewPadInt(uid);
            ISlave slave1 = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url[0]);
            ISlave slave2 = (ISlave)Activator.GetObject(
                                  typeof(ISlave),
                              url[1]);
            RemotePadInt[] newRemotePadInts = new RemotePadInt[2];
            try
            {
                newRemotePadInts[0] = slave1.create(uid, tsValue);
            }
            catch (SocketException)
            {
                bool res = masterServ.declareSlaveFailed(url[0]);
                //Makes Second attemp to access padInt
                RemotePadInt[] retriedRemotePadInt = CreateRemotePadInt(uid);
                return retriedRemotePadInt;
            }
            catch (IOException)
            {
                bool res = masterServ.declareSlaveFailed(url[0]);
                //Makes Second attemp to access padInt
                RemotePadInt[] retriedRemotePadInt = CreateRemotePadInt(uid);
                return retriedRemotePadInt;
            }
            try
            {
                newRemotePadInts[1] = slave2.create(uid, tsValue);
            }
            catch (SocketException)
            {
                bool res = masterServ.declareSlaveFailed(url[1]);
                //Makes Second attemp to access padInt
                RemotePadInt[] retriedRemotePadInt = CreateRemotePadInt(uid);
                return retriedRemotePadInt;
            }
            catch (IOException)
            {
                bool res = masterServ.declareSlaveFailed(url[1]);
                //Makes Second attemp to access padInt
                RemotePadInt[] retriedRemotePadInt = CreateRemotePadInt(uid);
                return retriedRemotePadInt;
            }
            return newRemotePadInts;
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

            
            try
            {
                remotePadInts[0] = slave1.access(uid, tsValue);
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
                masterServ.printSomeShit("IM HERE MOTHAFUCKERS!");
                bool res = masterServ.declareSlaveFailed(url[0]);
                //Makes Second attemp to access padInt
                RemotePadInt[] rpi = AccessRemotePadInt(uid);
                return rpi;
            }
            try
            {
                remotePadInts[1] = slave2.access(uid, tsValue);
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
                masterServ.printSomeShit("IM HERE MOTHAFUCKERS!");
                masterServ.declareSlaveFailed(url[1]);
                //Makes Second attemp to access padInt
                RemotePadInt[] rpi = AccessRemotePadInt(uid);
                return rpi;
            }
            return remotePadInts;
        }

        
    }
}
