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
        public int writeTS;
        public int readTS;

        public PadInt(int uid)
        {
            this.uid = uid;
        }

       public int read(){
           return this.value;
    }

       public void write(int value){
           this.value = value;

       }
    }
}
