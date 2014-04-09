using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADIDSTM
{
    public class PadInt : IPadInt
    {

        public int uid;

        public PadInt(int uid)
        {
            this.uid = uid;
        }
        
        public void Write(int value)
        {
            RemotePadInt RpadInt = DSTMLib.AccessRemotePadInt(uid);
            bool success = RpadInt.Write(value, DSTMLib.transactionTS);
            if (success)
            {
                DSTMLib.visitedPadInts.Add(RpadInt);
            }
            else {
                DSTMLib.TxAbort();
            }
        }

        public int Read()
        {
            RemotePadInt RpadInt = DSTMLib.AccessRemotePadInt(uid);
            int val = RpadInt.Read(DSTMLib.transactionTS);
            if (val == -999) {
                DSTMLib.TxAbort();
            } 

            return val;
        }
    }
}
