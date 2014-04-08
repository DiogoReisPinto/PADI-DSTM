using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADIDSTM
{
    class PadInt : IPadInt
    {

        int uid;

        public PadInt(int uid)
        {
            this.uid = uid;
        }
        
        public void Write(int value)
        {
            RemotePadInt RpadInt = DSTMLib.AccessPadInt(uid);
            RpadInt.Write(value);
        }

        public int Read()
        {
            RemotePadInt RpadInt = DSTMLib.AccessPadInt(uid);
            int val = RpadInt.Read();
            return val;
        }
    }
}
