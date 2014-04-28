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
            RemotePadInt[] RpadInt = DSTMLib.AccessRemotePadInt(uid);
            bool success1 = RpadInt[0].Write(value, DSTMLib.transactionTS);
            bool success2 = RpadInt[1].Write(value, DSTMLib.transactionTS);
            if (success1 && success2)
            {
                DSTMLib.visitedPadInts.Add(RpadInt[0]);
                DSTMLib.visitedPadInts.Add(RpadInt[1]);
            }
            else
            {
                DSTMLib.TxAbort();
                throw new TxException("Write canceled because value to write is outdated");
            }
        }

        public int Read()
        {
            RemotePadInt[] RpadInt = DSTMLib.AccessRemotePadInt(uid);
            int val;
            try
            {
                //HERE SHOULD WE READ THE TWO VALUES? TIME FOR CHECKING THE SLAVES HEALTH?
                //WE ONLY READ FROM ONE PADINT BECAUSE THE OTHER ONE IS JUST A COPY, FOR SURE.
                val = RpadInt[0].Read(DSTMLib.transactionTS);
            }
            catch (TxException e)
            {
                DSTMLib.TxAbort();
                throw new TxException(e.message);
            }

            return val;
        }
    }
}
