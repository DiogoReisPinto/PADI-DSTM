using System;
namespace PADIDSTM
{
    public interface ISlave
    {
        PadInt access(int uid);
        PadInt create(int uid);
        void freeze();
        void recover();
        void status();
    }
}
