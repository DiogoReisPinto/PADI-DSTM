using System;
namespace PADIDSTM
{
    public interface ISlave
    {
        RemotePadInt access(int uid,long tid);
        RemotePadInt create(int uid, long tid);
        void addCopyOfPadInt(RemotePadInt pi);
        void freeze();
        void fail();
        void recover();
        void status();
        void checkStatus();
        bool ping();
        void removePadInt(int id);

        
    }
}
