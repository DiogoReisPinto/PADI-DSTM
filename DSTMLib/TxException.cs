using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADIDSTM
{
    [Serializable]
    public class TxException : ApplicationException
    {
        public string message;
        
        public TxException(string message)
        {
            this.message = message;
        }

        public TxException(System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
		: base(info, context) {
            message = info.GetString("message");
	}

	public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) {
		base.GetObjectData(info, context);
        info.AddValue("message", message);
	}
    }
}
