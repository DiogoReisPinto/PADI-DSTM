using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADIDSTM
{
    public class TVersion:MarshalByRefObject
    {
        
        public long writeTS; //WRITE TIMESTAMP OF THE TENTATIVE VERSION
        public int versionVal; //VALUE OF THE WRITE OF TENTATIVE VERSION
        public bool commited; //IF TENTATIVE VERSION IS COMMITED OR NOT

        public TVersion(long wts, int val)
        {
            this.writeTS = wts;
            this.versionVal = val;
            this.commited = false;
        }
    }
}
