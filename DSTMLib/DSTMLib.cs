﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PADIDSTM;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Collections;
using System.Runtime.Serialization.Formatters;

namespace PADIDSTM
{
    public class DSTMLib
    {

        public static IMaster masterServ;
        public static string transactionTS;
        public static List<RemotePadInt> visitedPadInts;
        public static List<RemotePadInt> createdPadInts;

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
            visitedPadInts = new List<RemotePadInt>();
            createdPadInts = new List<RemotePadInt>();
            Console.WriteLine("IN DSTMlib :"+transactionTS);
            return true;
        }

        public static bool TxCommit()
        {
            long tc = Convert.ToInt64(transactionTS.Split('#')[0]);
            foreach (RemotePadInt rpi in visitedPadInts)
            {
                rpi.commitTx(tc);
            }
            foreach (RemotePadInt rpi in createdPadInts)
            {
                rpi.commitTx(tc);
            }
            return true;

        }

        public static bool TxAbort() {
            long tc = Convert.ToInt64(transactionTS.Split('#')[0]);
            foreach (RemotePadInt rpi in visitedPadInts)
            {
                rpi.abortTx(tc);
            }
            return true;
        
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
            string url = masterServ.GetLocationNewPadInt(uid);
            if (url == null)
                return null;

            ISlave slave = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url);
            long tid = Convert.ToInt64(transactionTS.Split('#')[0]);
            RemotePadInt newRemotePadInt = slave.create(uid,tid);
            PadInt newPad = new PadInt(newRemotePadInt.uid);
            createdPadInts.Add(newRemotePadInt);
            return newPad;
        }


        public static PadInt AccessPadInt(int uid) {
            RemotePadInt newRemotePadInt = AccessRemotePadInt(uid);
            if (newRemotePadInt == null)
            {
                return null;
            }
            PadInt newPad = new PadInt(newRemotePadInt.uid);
            return newPad;
        }


        public static RemotePadInt AccessRemotePadInt(int uid) {
            string url = masterServ.DiscoverPadInt(uid);
            if (url == null|| url=="UNDEFINED")
                return null;
            Console.WriteLine(url);

            ISlave slave = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url);
            long tid = Convert.ToInt64(transactionTS.Split('#')[0]);
            RemotePadInt retPadInt = slave.access(uid,tid);
            if (retPadInt == null)
            {
                return null;
            }
            else
                return retPadInt;
           
        }

    }

}
