using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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
            catch (TxException e) //CASE THAT A WRITE FAILS IN THE SLAVE
            {
                DSTMLib.TxAbort();//ABORTS TRANSACTION
                string msg = e.message;
                throw new TxException(msg);

            }
            catch (Exception)
            {
                DSTMLib.TxAbort();
                throw new TxException("Cant write commit because server to write is not available. Transaction Aborted");
            }
            if (success1 && success2) //IF WE HAVE SUCCESS IN BOTH WRITES WE CAN ADD TO VISITED PADINTS
            {
                if(!DSTMLib.visitedPadInts.ContainsKey(RpadInt[0]))
                    DSTMLib.visitedPadInts.Add(RpadInt[0],RpadInt[0].url);
                if (!DSTMLib.visitedPadInts.ContainsKey(RpadInt[1]))
                    DSTMLib.visitedPadInts.Add(RpadInt[1],RpadInt[1].url);
            }
            else //IF ONE OF THE WRITES FAILED WE SHOULD ABORT THE TRANSACTION
            {
                DSTMLib.TxAbort();
                throw new TxException("Write canceled. Transaction aborted");
            }
        }

        public int Read()
        {
            RemotePadInt[] RpadInt = DSTMLib.AccessRemotePadInt(uid);
            int val;
            try
            {
                val = RpadInt[0].Read(DSTMLib.transactionTS); //ONLY READS A VALUE FROM ONE OF THE ACCESSED PADINTS
            }
            catch (TxException e) //IF READ IS NOT POSSIBLE WE SHOULD ABORT THE TRANSACTION
            {
                DSTMLib.TxAbort();
                string msg = e.message;
                throw new TxException(msg);
            }

            return val;
        }
    }
}
