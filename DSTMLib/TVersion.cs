using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADIDSTM
{
    public class TVersion
    {
        
        public long writeTS;
        public int versionVal;

        public TVersion(long wts, int val)
        {
            this.writeTS = wts;
            this.versionVal = val;
        }
    }
}
