using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADIDSTM
{
    public class TxException : Exception
    {
        public TxException()
        {
        }

        public TxException(string message)
            : base(message)
        {
        }

        public TxException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
