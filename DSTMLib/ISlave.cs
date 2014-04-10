using System;
namespace PADIDSTM
{
    public interface ISlave
    {
        RemotePadInt access(int uid,long tid);
        RemotePadInt create(int uid, long tid);
        void freeze();
        void fail();
        void recover();
        void status();
        void checkStatus();
        void removePadInt(int id);
    }
}
