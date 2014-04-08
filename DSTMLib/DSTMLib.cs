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

namespace PADIDSTM
{
    public class DSTMLib
    {

        public static IMaster masterServ;
        public static string transactionTS;

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
            return true;
        }

        public static bool TxCommit() { return true; }

        public static bool TxAbort() { return true; }

        public static bool Status() { 
            masterServ.callStatusOnSlaves();
            return true;
        }

        public static bool Fail(string URL) { return true; }

        public static bool Freeze(string URL) { return true; }

        public static bool Recover(string URL) { return true; }

        public static PadInt CreatePadInt(int uid) {
            string url = masterServ.GetLocationNewPadInt(uid);
            ISlave slave = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url);

            PadInt newPadInt = slave.create(uid);
            return newPadInt;
        }

        public static PadInt AccessPadInt(int uid) {
            string url = masterServ.DiscoverPadInt(uid);
            ISlave slave = (ISlave)Activator.GetObject(
                                  typeof(ISlave),
                              url);

            PadInt pint = slave.access(uid);
            return pint;

        }
    
    }

}
