using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADIDSTM
{
    public class TVersion:MarshalByRefObject
    {
        
        public long writeTS;
        public int versionVal;
        public bool commited;

        public TVersion(long wts, int val)
        {
            this.writeTS = wts;
            this.versionVal = val;
            this.commited = false;
        }
    }
}
