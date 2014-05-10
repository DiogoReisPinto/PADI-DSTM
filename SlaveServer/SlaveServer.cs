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

namespace SlaveServer
{
    public class SlaveServer
    {

        public static string url;
        private static IMaster masterServ;
        
        static void Main()
        {
            bool res = login();
            Console.Read();
        }

        public static bool login()
        {

            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = 0;
            props["timeout"] = 4000;
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

        

        //tid==0 is from a call for copy padint - tolerant version
        public RemotePadInt access(int uid,long tid)
        {

            while (freezed) { }
            while (failed)
            {
                while (true) { }
            }
            RemotePadInt req = padIntObjects[uid];
            if (tid == req.creatorTID || tid==0)
                return req;
            if(!req.isCommited){
                Thread.Sleep(1000);
                if (req.isCommited)
                    return req;
                else
                    return null;
            }
            return req;
        }


        public void checkStatus()
        {

            while (freezed || failed) { }

        }

        public bool ping()
        {
            while (freezed || failed) {
                //throw new System.Net.Sockets.SocketException();
            };
            return true;
            
        }


        public RemotePadInt create(int uid, long tid)
        {
            while (freezed) { }
            while (failed)
            {
                while (true) { }
            }
            RemotePadInt newPadInt = new RemotePadInt(uid, url);
            newPadInt.creatorTID = tid;
            padIntObjects.Add(uid, newPadInt);
            masterServ.RegisterNewPadInt(uid, url);
            return newPadInt;
        }

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
            masterServ.recoverSlave();
            Console.WriteLine("Will send update load with url:{0} and load:{1}", url, padIntObjects.Count);
            masterServ.updateLoad(url, padIntObjects.Count);
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
            foreach (KeyValuePair<int, RemotePadInt> entry in padIntObjects)
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
                    Console.WriteLine("PadInt with uid:{0} and value:{1}", entry.Key, 0);
                else
                    Console.WriteLine("PadInt with uid:{0} and value:{1}", entry.Key, actual.versionVal);
            }
        }

        public override object InitializeLifetimeService()
        {

            return null;

        }

        public void removePadInt(int id)
        {
            padIntObjects.Remove(id);
        }
    }

}
