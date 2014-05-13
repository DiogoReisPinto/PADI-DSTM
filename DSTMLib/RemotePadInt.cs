
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;

namespace PADIDSTM
{
    public class RemotePadInt : MarshalByRefObject
    {
        public int uid;
        public int value;
        public string url;
        private long wts;
        public List<long> rts = new List<long>();
        public List<TVersion> tentativeVersions = new List<TVersion>();
        public bool isCommited;
        public long creatorTID;
        public TVersion preparedCommit;
        public bool freezed = false;
        public bool failed = false;
        


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
            this.wts = 0;
            this.isCommited = false;
        }

        public RemotePadInt(RemotePadInt availablePadInt, string newURL)
        {

            this.uid = availablePadInt.uid;
            this.value = availablePadInt.value;
            this.url = newURL;
            this.wts = availablePadInt.Wts;
            this.rts = availablePadInt.Rts;
            this.tentativeVersions = availablePadInt.tentativeVersions;
            this.isCommited = availablePadInt.isCommited;
            this.creatorTID = availablePadInt.creatorTID;
            this.preparedCommit = availablePadInt.preparedCommit;
            this.freezed = availablePadInt.freezed;
        }

        public int getUID(){
            while (freezed) { }
            while (failed)
            {
                while (true) { }
            }
            return this.uid;
        }

        public int Read(string ts)
        {
            while (freezed) { }
            while (failed) { 
                while (true){ } 
            }
            long tc = Convert.ToInt64(ts.Split('#')[0]);
            int tieBreaker = Convert.ToInt32(ts.Split('#')[1]);
            ISlave server = (ISlave)Activator.GetObject(
                                    typeof(ISlave),
                                url);
            //JUST FOR INITIALIZING. WILL ALWAYS BE OVERRIDED OR TRANSACTION WILL ABORT
            int val;
            if (tc > this.wts)
            {
                TVersion dSelect = getMax(tc);
                if (dSelect == null)
                {
                    rts.Add(tc);
                    server.checkStatus();
                    return 0;
                }
                if (dSelect.commited || dSelect.writeTS == tc)
                {
                    server.checkStatus();//to block when server is freezed or failed
                    val = dSelect.versionVal;
                    rts.Add(tc);
                }
                else
                {
                    //WAITING FOR THE DSELECT TO COMMIT
                    Thread.Sleep(4000);
                    if (dSelect.writeTS == this.wts)
                    {
                        server.checkStatus();//to block when server is freezed or failed
                        val = dSelect.versionVal;
                        rts.Add(tc);
                    }
                    else
                        throw new TxException("Timeout when waiting for a write commit.");

                }
            }
            else
                throw new TxException("Reading canceled because value is outdated");

            return val;
        }

        public bool Write(int val, string ts)
        {
            while (freezed) { }
            while (failed)
            {
                while (true) { }
            }
            Console.WriteLine("uid:" + uid + " ts: " + ts);
            string[] txID = ts.Split('#');

            long tc = Convert.ToInt64(txID[0]);
            int tieBreaker = Convert.ToInt32(ts.Split('#')[1]);
            ISlave server = (ISlave)Activator.GetObject(
                                    typeof(ISlave),
                                url);
            long maxD = Int64.MinValue;
            if (rts.Count > 0)
                maxD = rts.Max();
            if (tc >= maxD && tc > wts)
            {
                server.checkStatus();
                tentativeVersions.Add(new TVersion(tc, val));
                this.value = val;
                return true;
            }
            else
                return false;
        }

        private TVersion getMax(long ts)
        {
            TVersion max = null;
            long maxLong = 0;
            foreach (TVersion v in tentativeVersions)
            {
                if (v.writeTS <= ts && v.writeTS >= maxLong)
                {
                    maxLong = v.writeTS;
                    max = v;
                }

            }
            return max;
        }

        public int abortTx(long txID)
        {
            while (freezed) { }
            while (failed)
            {
                while (true) { }
            }
            List<TVersion> toRemove = new List<TVersion>();
            foreach (TVersion tv in tentativeVersions)
            {
                if (tv.writeTS == txID)
                    toRemove.Add(tv);
            }
            foreach (TVersion tv in toRemove)
            {
                tentativeVersions.Remove(tv);
            }
            return 1;
        }

        public int prepareCommitTx(long txID)
        {
            bool res = FreezeFailedCycle();
            while (freezed) { }
            while (failed)
            {
                while (true) { }
            }
            foreach (TVersion tv in tentativeVersions)
            {
                if (tv.writeTS == txID)
                {
                    preparedCommit = tv;
                    preparedCommit.commited = false;
                }
            }

            return 1;
        }

        public int commitTx(long txID)
        {
            this.value = preparedCommit.versionVal;
            this.wts = preparedCommit.writeTS;
            preparedCommit.commited = true;
            preparedCommit = null;
            return 1;
        }

        public int prepareCommitPadInt(long txID) {
            try
            {
                bool res = FreezeFailedCycle();
            }
            catch (Exception)
            {
                throw new IOException();
            }
            while (freezed) { }
            while (failed)
            {
                while (true) { }
            }
            return 1;
        }

        public int commitPadInt(long txID)
        {
            this.isCommited = true;
            return 1;
        }

        

        public void Freeze()
        {
            this.freezed = true;
        }

        public void Fail()
        {
            this.failed = true;
        }

        public void Recover()
        {
            this.freezed = false;
            this.failed = false;
        }

        public bool FreezeFailedCycle()
        {
            ISlave server = (ISlave)Activator.GetObject(
                                     typeof(ISlave),
                                 url);
            return server.ping();
        }
    }
}
