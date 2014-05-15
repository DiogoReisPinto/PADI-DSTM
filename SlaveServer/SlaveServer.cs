using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PADIDSTM;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters;
using System.Collections;
using System.Threading;
using System.Net.Sockets;

namespace SlaveServer
{
    public class SlaveServer
    {

        public static string url;
        private static IMaster masterServ;
        
        static void Main()
        {
            bool res = login(); 
            Console.Read(); //SLAVE KEPT IN A CYCLE RECEIVING CALLS
        }

        //STARTS THE CONNECTION TO THE MASTER AND REGISTER
        public static bool login()
        {

            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = 0;
            props["timeout"] = 8000;
            TcpChannel channel = new TcpChannel(props, null, provider);

            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(RemoteSlave),
                "RemoteSlave",
                WellKnownObjectMode.Singleton);

            var channelData = (ChannelDataStore)channel.ChannelData;
            var port = new System.Uri(channelData.ChannelUris[0]).Port;
            url = "tcp://localhost:" + port + "/RemoteSlave";

            masterServ = (IMaster)Activator.GetObject(
                                    typeof(IMaster),
                                "tcp://localhost:8086/RemoteMaster");
            RemoteSlave.masterServ = masterServ;
            bool ret = masterServ.registerSlave(url);
            RemoteSlave.url = url;
            return ret;
        }
    }

    public class RemoteSlave : MarshalByRefObject, ISlave
    {
        private Dictionary<int, RemotePadInt> padIntObjects = new Dictionary<int, RemotePadInt>();
        private bool freezed=false;
        private bool failed = false;
        public static string url;
        public static IMaster masterServ;

        

        //TID==0 IS FOR A RECOVER CALL
        public RemotePadInt access(int uid,long tid)
        {
            bool res = ping();
            RemotePadInt req = padIntObjects[uid];
            if (tid == req.creatorTID || tid==0) //IF THE TRANSACTION TRYING TO ACCESS IS THE ONE THAT CREATED IT OR A ACCESS RECOVER CALL
                return req;
            if(!req.isCommited){
                Thread.Sleep(2000); //WAITS FOR THE TRANSACTION TO COMMIT
                if (req.isCommited)
                {
                    return req;
                }
                else
                    return null;
            }
            return req;
        }

        //METHOD USED FOR READS AND WRITES TO KNOW IF SLAVE IS UNAVAILABLE OR NOT
        public void checkStatus()
        {
            while (freezed || failed) { }
        }

        //METHOD FOR KNOWING IF SLAVE IS RESPONDING OR NOT - WAITS 5 SECONDS - SIMULATE A 5 SECOND DELAY
        public bool ping()
        {
            while (freezed || failed) {
                Thread.Sleep(5000);
                if(freezed|| failed)
                    throw new SocketException();
            };
            return true;
            
        }


        public RemotePadInt create(int uid, long tid)
        {
            bool res = ping();
            RemotePadInt newPadInt = new RemotePadInt(uid, url);
            newPadInt.creatorTID = tid;
            padIntObjects.Add(uid, newPadInt); //ADS THE PADINT TO THE LIST OF PADINTS IN THE SERVER
            masterServ.RegisterNewPadInt(uid, url); //CONFIRMATION OF THE CREATION OF THE PADINT IN THE SERVER TO THE MASTER
            return newPadInt;
        }

        //METHOD CALLED IN A RECOVER CONTEXT - JUST ADDS THE PADINT TO THE SLAVE
        public void addCopyOfPadInt(RemotePadInt pi)
        {
            padIntObjects.Add(pi.uid,pi);
        }


        public void freeze() 
        {
            freezed = true;
            foreach (KeyValuePair<int, RemotePadInt> entry in padIntObjects)
            {
                entry.Value.Freeze();
            }

        }

        public void fail()
        {
            failed = true;
            foreach (KeyValuePair<int, RemotePadInt> entry in padIntObjects)
            {
                entry.Value.Fail();
            }
        }

        public void recover() {
            freezed = false;
            failed = false;
            foreach (KeyValuePair<int, RemotePadInt> entry in padIntObjects)
            {
               
                    entry.Value.Recover();
               
            }

        }
        public void status()
        {
            Console.WriteLine("\r\n");
            Console.WriteLine("STATUS ON SERVER {0}", url);
            if (freezed)
                Console.WriteLine("SERVER IS FREEZED");
            else if (failed)
                Console.WriteLine("SERVER IS FAILED");
            else
                Console.WriteLine("SERVER IS OK");
            Console.WriteLine("------------STORED OBJECTS------------");
            foreach (KeyValuePair<int, RemotePadInt> entry in padIntObjects) //FINDS THE CORRECT TENTATIVE VERSION OF PADINT - THE ONE WITH THE GREATEST TS AND COMMITED 
            {
                long maxTS = long.MinValue;
                TVersion actual = null;
                foreach (TVersion t in entry.Value.tentativeVersions)
                {
                    if (t.commited == true && t.writeTS > maxTS)
                    {
                        maxTS = t.writeTS;
                        actual = t;
                    }
                }
                if(actual==null)
                    Console.WriteLine("PadInt with uid:{0} and value:{1}", entry.Key, 0); //WHEN PADINT HAS NOT TVERSIONS
                else
                    Console.WriteLine("PadInt with uid:{0} and value:{1}", entry.Key, actual.versionVal);
            }
        }

        public override object InitializeLifetimeService()
        {

            return null;

        }

        //REMOVES THE PADINT FROM THE LIST OF PADINTS OF THE SLAVE
        public void removePadInt(int id)
        {
            padIntObjects.Remove(id);
        }
    }

}
