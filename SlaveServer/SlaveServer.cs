using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PADIDSTM;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace SlaveServer
{
    public class SlaveServer
    {

        private static string url;
        
        static void Main()
        {
             
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public bool login(string URL)
        {
           
            TcpChannel channel = new TcpChannel(0);

            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(RemoteSlave),
                "RemoteSlave",
                WellKnownObjectMode.Singleton);

            var channelData = (ChannelDataStore)channel.ChannelData;
            var port = new System.Uri(channelData.ChannelUris[0]).Port;
            url = "tcp://localhost:" + port + "/Server";

            IMaster obj = (IMaster)Activator.GetObject(
                                    typeof(IMaster),
                                "tcp://localhost:8086/RemoteMaster");
            bool ret = obj.registerSlave(url);
            return ret;
        }
    }

    public class RemoteSlave : MarshalByRefObject, ISlave
    {
        private Dictionary<int, PadInt> padIntObjects = new Dictionary<int, PadInt>();

        public PadInt access(int uid)
        {
            return null;
        }
        public PadInt create(int uid)
        {
            PadInt newPadInt = new PadInt(uid);
            padIntObjects.Add(uid, newPadInt);
            return newPadInt;
        }
        public void freeze() 
        { 
            return; 
        }
        public void recover() {
            return;
        }
        public void status()
        {
            return;
        }

    }

}
