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
                throw new TxException("Write canceled because value to write is outdated");
            }
        }

        public int Read()
        {
            RemotePadInt RpadInt = DSTMLib.AccessRemotePadInt(uid);
            int val;
            try{
                val = RpadInt.Read(DSTMLib.transactionTS);
            }catch(TxException e){
                DSTMLib.TxAbort();
                throw new Exception();
            } 

            return val;
        }
    }
}
