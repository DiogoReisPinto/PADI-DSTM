using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPerformance
{
    using System;
    using PADIDSTM;

    class ClientPerformance
    {
        static void Main(string[] args)
        {
            bool res;

            DSTMLib.Init();
            res = DSTMLib.TxBegin();
            PadInt pi_a = DSTMLib.CreatePadInt(0);
            res = DSTMLib.TxCommit();
            Console.ReadLine();
            string startTime = "Started at: " + DateTime.Now.ToString("h:mm:ss tt");
            for (int i = 0; i < 300; i++)
            {
                res = DSTMLib.TxBegin();
                try
                {
                    PadInt a = DSTMLib.AccessPadInt(0);
                    int val = a.Read();
                    val++;
                    a.Write(val);
                }
                catch (Exception)
                {
                    DSTMLib.TxAbort();
                }
                res = DSTMLib.TxCommit();
            }
            string endTime = "Started at: " + DateTime.Now.ToString("h:mm:ss tt");
            Console.WriteLine("Started at: " + startTime + " and ended at: " + endTime);
            Console.ReadLine();
            res = DSTMLib.TxBegin();
            PadInt result = DSTMLib.AccessPadInt(0);
            int valor = result.Read();
            res = DSTMLib.TxCommit();
            Console.WriteLine("The value of PadInt is: " + valor);
            Console.ReadLine();

        }
    }
}
