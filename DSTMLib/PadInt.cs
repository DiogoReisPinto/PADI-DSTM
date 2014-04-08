using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIDSTM
{
    public class PadInt : MarshalByRefObject, IPadInt
    {
        public int uid;
        public int value;
        public string ts;

        public PadInt(int uid)
        {
            this.uid = uid;
        }

       public int Read(){
           return this.value;
    }

       public void Write(int value){
           this.value = value;

       }
    }
}
