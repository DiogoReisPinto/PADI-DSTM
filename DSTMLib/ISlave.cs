using System;
namespace PADIDSTM
{
    public interface ISlave
    {
        PadInt access(int uid);
        PadInt create(int uid);
        void freeze();
        void fail();
        void recover();
        void status();

        int ReadPadInt(int uid);

        void WritePadInt(int uid, int value);
    }
}
