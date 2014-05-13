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
            bool success1=false;
            bool success2=false;
            RemotePadInt[] RpadInt = DSTMLib.AccessRemotePadInt(uid);
            try
            {
                success1 = RpadInt[0].Write(value, DSTMLib.transactionTS);
                success2 = RpadInt[1].Write(value, DSTMLib.transactionTS);
            }
            catch (Exception)
            {
                DSTMLib.TxAbort();
                //throw new TxException("Write canceled because server is unavailable. Transaction will abort");
                
            }
            if (success1 && success2)
            {
                if(!DSTMLib.visitedPadInts.ContainsKey(RpadInt[0]))
                    DSTMLib.visitedPadInts.Add(RpadInt[0],RpadInt[0].url);
                if (!DSTMLib.visitedPadInts.ContainsKey(RpadInt[1]))
                    DSTMLib.visitedPadInts.Add(RpadInt[1],RpadInt[1].url);
            }
            else
            {
                DSTMLib.TxAbort();
                //throw new TxException("Write canceled because value to write is outdated");
            }
        }

        public int Read()
        {
            RemotePadInt[] RpadInt = DSTMLib.AccessRemotePadInt(uid);
            int val;
            try
            {
                val = RpadInt[0].Read(DSTMLib.transactionTS);
            }
            catch (TxException e)
            {
                DSTMLib.TxAbort();
                string msg = e.message;
                throw new TxException(msg);
            }

            return val;
        }
    }
}
