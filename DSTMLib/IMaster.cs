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

        string GetLocationNewPadInt(int uid);
        string GetTS(int uid);
        void RegisterNewPadInt(int uid, string serverURL);
        bool registerSlave(String url);

        int getTransactionID();
    }
}
