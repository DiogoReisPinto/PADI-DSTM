using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Text;

namespace PADIDSTM
{
    public class RemotePadInt : MarshalByRefObject, IPadInt
    {
        public int uid;
        public int value;
        public string ts;
        public string url;

        public RemotePadInt(int uid, string url)
        {
            this.uid = uid;
            this.url = url;
        }

       

       public int Read(){
           ISlave server = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url);
           server.ReadPadInt(uid);
           return this.value;
    }

       public void Write(int value){
           ISlave server = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url);
           server.WritePadInt(uid, value);

       }
    }
}
