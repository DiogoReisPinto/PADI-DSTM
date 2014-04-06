using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADIDSTM
{
    public interface IMaster
    {
        string DiscoverPadInt(int uid);
        int GetTS();
        void ConfirmWrite(int uid, string serverID);
    }
}
