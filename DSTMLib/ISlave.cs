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

        int ReadPadInt(int uid);
        void removePadInt(int id);
        void WritePadInt(int uid, int value);
    }
}
