using System;
namespace PADIDSTM
{
    public interface ISlave
    {
        RemotePadInt access(int uid);
        RemotePadInt create(int uid);
        void freeze();
        void fail();
        void recover();
        void status();

        int ReadPadInt(int uid);

        void WritePadInt(int uid, int value);
    }
}
