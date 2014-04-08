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

namespace SlaveServer
{
    public class SlaveServer
    {

        private static string url;
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
            props["port"] = 2001;
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
        private Dictionary<int, PadInt> padIntObjects = new Dictionary<int, PadInt>();
        private List<Action> freezedQueue = new List<Action>();
        private bool freezed=false;
        private bool failed = false;
        public static string url;
        public static IMaster masterServ;

        
        public PadInt access(int uid)
        {
            if (!freezed && !failed)
            {
                PadInt req = padIntObjects[uid];
                return req;
            }
            else if (freezed)
            {
                freezedQueue.Add(() => access(uid));
                return null;
            }
            else
                return null;
        }

        public void WritePadInt(int uid, int value)
        {
            if (freezed)
                freezedQueue.Add(() => WritePadInt(uid, value));
            else
                padIntObjects[uid].Write(value);
            
        }

        public int ReadPadInt(int uid)
        {
            if (freezed)
                freezedQueue.Add(() => ReadPadInt(uid));
            else
                return padIntObjects[uid].Read();
            return 0;
        }

        public PadInt create(int uid)
        {
            if (!freezed && !failed)
            {
                PadInt newPadInt = new PadInt(uid,url);
                padIntObjects.Add(uid, newPadInt);
                masterServ.RegisterNewPadInt(uid, url);
                
                return newPadInt;

            }
            else
            {
                freezedQueue.Add(() => create(uid));
                return null;
            }
        }


        public void freeze() 
        {
            freezed = true;
        }

        public void fail()
        {
            failed = true;
        }

        public void recover() {
            foreach (var action in freezedQueue)
            {
                action.Invoke();
            }
        }
        public void status()
        {
            Console.WriteLine("STATUS ON SERVER {0}", url);
            if (freezed)
                Console.WriteLine("SERVER IS FREEZED");
            else if (failed)
                Console.WriteLine("SERVER IS FAILED");
            else
                Console.WriteLine("SERVER IS OK");
            Console.WriteLine("------------STORED OBJECTS------------");
            foreach (KeyValuePair<int, PadInt> entry in padIntObjects)
            {
                Console.WriteLine("Object with id:{0} has value {1}", entry.Key, entry.Value.Read());
            }
        }

    }

}
