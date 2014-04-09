using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;

namespace PADIDSTM
{
    public class RemotePadInt : MarshalByRefObject, IPadInt
    {
        public int uid;
        public int value;
        public string url;
        private long wts;
        List<long> rts = new List<long>();
        List<TVersion> tentativeVersions = new List<TVersion>();

        public List<long> Rts
        {
            get { return rts; }
            set { rts = value; }
        }

        public long Wts
        {
            get { return wts; }
            set { wts = value; }
        }


        public RemotePadInt(int uid, string url)
        {
            this.uid = uid;
            this.url = url;
        }



       

       public int Read(){
           string ts = DSTMLib.transactionTS;
           long tc = Convert.ToInt64(ts.Split('#')[0]);
           //NOT USED FOR CHECKPOINT IMPLEMENTATION
           int tieBreaker = Convert.ToInt32(ts.Split('#')[1]);
           ISlave server = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url);
           //JUST FOR INITIALIZING. WILL ALWAYS BE OVERRIDED OR TRANSACTION WILL ABORT
           int value = -999;
           if (tc > this.wts)
           {
              TVersion dSelect = getMax(tc);
               if(dSelect.writeTS==this.wts)
                   value= server.ReadPadInt(uid);
               else
               {
                   Thread.Sleep(1000);
                   if (dSelect.writeTS == this.wts)
                       value=server.ReadPadInt(uid);
                   else
                       DSTMLib.TxAbort();

               }
           }
           else
               DSTMLib.TxAbort();
           return value;
    }

       public void Write(int value){
           string ts = DSTMLib.transactionTS;
           long tc = Convert.ToInt64(ts.Split('#')[0]);
           //NOT USED FOR CHECKPOINT IMPLEMENTATION
           int tieBreaker = Convert.ToInt32(ts.Split('#')[1]);
           ISlave server = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url);
           long maxD = rts.Max();
           if (tc >= maxD && tc > wts)
           {
               tentativeVersions.Add(new TVersion(tc, value));
               DSTMLib.visitedPadInts.Add(this);
           }
           else
               DSTMLib.TxAbort();

           //server.WritePadInt(uid, value);

       }

       private TVersion getMax(long ts)
       {
           TVersion max = null;
           long maxLong=0; 
           foreach (TVersion v in tentativeVersions)
           {
               if (v.writeTS <= ts && v.writeTS > maxLong)
               {
                   maxLong = v.writeTS;
                   max = v;
               }

           }
           return max;
       }

       public void abortTx(long txID)
       {
           foreach (TVersion tv in tentativeVersions)
           {
               if (tv.writeTS == txID)
                   tentativeVersions.Remove(tv);
           }
       }

       public void commitTx(long txID)
       {
           foreach (TVersion tv in tentativeVersions)
           {
               if (tv.writeTS == txID)
               {
                   tentativeVersions.Remove(tv);
                   wts = txID;
                   value = tv.versionVal;
               }
           }
       }
    }
}
