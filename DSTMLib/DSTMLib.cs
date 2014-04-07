using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PADIDSTM;
using System.Runtime.Remoting.Channels.Tcp;

namespace PADIDSTM
{
    public class DSTMLib
    {

        public static IMaster masterServ;

        bool Init() {
            masterServ = (IMaster)Activator.GetObject(
                                    typeof(IMaster),
                                "tcp://localhost:8086/RemoteMaster");
            return true;
        }

        bool TxBegin() { return true; }

        bool TxCommit() { return true; }

        bool TxAbort() { return true; }

        bool Status() { return true; }

        bool Fail(string URL) { return true; }

        bool Freeze(string URL) { return true; }

        bool Recover(string URL) { return true; }

        PadInt CreatePadInt(int uid) {
            string url = masterServ.GetLocationNewPadInt();
            ISlave slave = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url);
            PadInt newPadInt = slave.create(uid);
            return newPadInt;
            

        }

        PadInt AccessPadInt(int uid) { return null; }
    
    }

}
