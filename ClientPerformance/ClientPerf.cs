using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPerformance
{
    using System;
    using PADIDSTM;
    using System.Threading;

    class ClientPerformance
    {
        static void Main(string[] args)
        {
            Thread.Sleep(2000);
            testAborts(200);
        }


        public static void testCycleConcurrency(int cycles)
        {
            bool res;

            DSTMLib.Init();
            res = DSTMLib.TxBegin();
            PadInt pi_a = DSTMLib.CreatePadInt(0);
            res = DSTMLib.TxCommit();
            Console.ReadLine();
            int aborted = 0;
            int commited = 0;
            string startTime = "Started at: " + DateTime.Now.ToString("h:mm:ss tt");
            for (int i = 1; i <= cycles; i++)
            {
                res = DSTMLib.TxBegin();
                try
                {
                    PadInt a = DSTMLib.AccessPadInt(0);
                    int val = a.Read();
                    val++;
                    Console.WriteLine(val);
                    a.Write(val);
                }
                catch (Exception)
                {
                    aborted++;
                    DSTMLib.TxAbort();
                    continue;
                }
                commited++;
                res = DSTMLib.TxCommit();
                
            }
            string endTime = "Started at: " + DateTime.Now.ToString("h:mm:ss tt");
            Console.WriteLine("Started at: " + startTime + " and ended at: " + endTime);
            Console.WriteLine("Number of transactions commited: " + commited);
            Console.WriteLine("Number of transactions aborted: " + aborted);
            Console.ReadLine();
            res = DSTMLib.TxBegin();
            PadInt result = DSTMLib.AccessPadInt(0);
            int valor = result.Read();
            res = DSTMLib.TxCommit();
            Console.WriteLine("The value of PadInt is: " + valor);
            Console.ReadLine();

        }

        public static void testWrites(int numWrites)
        {
            bool res;
            DSTMLib.Init();
            res = DSTMLib.TxBegin();
            PadInt pi_a = DSTMLib.CreatePadInt(0);
            res = DSTMLib.TxCommit();
            Console.ReadLine();
            string startTime = "Started at: " + DateTime.Now.ToString("h:mm:ss tt");
            for (int i = 0; i < numWrites; i++)
            {
                res = DSTMLib.TxBegin();
                try
                {
                    PadInt a = DSTMLib.AccessPadInt(0);
                    a.Write(i);
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


        public static void testReads(int numReads)
        {
            bool res;
            DSTMLib.Init();
            res = DSTMLib.TxBegin();
            PadInt pi_a = DSTMLib.CreatePadInt(0);
            res = DSTMLib.TxCommit();
            Console.ReadLine();
            string startTime = "Started at: " + DateTime.Now.ToString("h:mm:ss tt");
            for (int i = 0; i < numReads; i++)
            {
                res = DSTMLib.TxBegin();
                try
                {
                    PadInt a = DSTMLib.AccessPadInt(0);
                    a.Read();
                }
                catch (Exception)
                {
                    DSTMLib.TxAbort();
                }
                res = DSTMLib.TxCommit();
            }
            string endTime = "ended at: " + DateTime.Now.ToString("h:mm:ss tt");
            Console.WriteLine(startTime + " and " + endTime);
            Console.ReadLine();
        }

        public static void testTimeToRecover(int numPadIntsToRecover)
        {
            bool res;
            Console.WriteLine("Press enter to create PadInts!");
            Console.ReadLine();
            DSTMLib.Init();
            res = DSTMLib.TxBegin();
            for (int i = 1; i <= numPadIntsToRecover; i++)
            {
                    DSTMLib.CreatePadInt(i);
                    
             }
            DSTMLib.TxCommit();
            DSTMLib.TxBegin();
            Console.WriteLine("Kill one of the two servers available, start another one and press Enter!");
            Console.ReadLine();
            string startTime = "Started at: " + DateTime.Now.ToString("h:mm:ss tt");
            PadInt a = DSTMLib.AccessPadInt(1);
            string endTime = "Ended at: " + DateTime.Now.ToString("h:mm:ss tt");
            DSTMLib.TxCommit();
            Console.WriteLine(startTime + " and " + endTime);
            Console.ReadLine();
        }

        public static void testCreates(int numPadIntsToCreate){
            bool res;
            Console.WriteLine("Press enter to create PadInts!");
            Console.ReadLine();
            DSTMLib.Init();
            
            string startTime = "Started at: " + DateTime.Now.ToString("h:mm:ss tt");
            for (int i = 1; i <= numPadIntsToCreate; i++)
            {
                res = DSTMLib.TxBegin();
                DSTMLib.CreatePadInt(i);
                DSTMLib.TxCommit();

            }
            string endTime = "Ended at: " + DateTime.Now.ToString("h:mm:ss tt");

            Console.WriteLine(startTime + " and " + endTime);
            Console.ReadLine();
        }

        public static void testCreatesOnTransaction(int numPadIntsToCreate)
        {
            bool res;
            Console.WriteLine("Press enter to create PadInts!");
            Console.ReadLine();
            DSTMLib.Init();
            string startTime = "Started at: " + DateTime.Now.ToString("h:mm:ss tt");
            res = DSTMLib.TxBegin();
            for (int i = 1; i <= numPadIntsToCreate; i++)
            {
                
                DSTMLib.CreatePadInt(i);
                

            }
            DSTMLib.TxCommit();
            string endTime = "Ended at: " + DateTime.Now.ToString("h:mm:ss tt");

            Console.WriteLine(startTime + " and " + endTime);
            Console.ReadLine();
        }

        public static void testAborts(int numTransactionsToAbort)
        {
            bool res;
            Console.WriteLine("Press enter to start Test!");
            Console.ReadLine();
            DSTMLib.Init();
            res = DSTMLib.TxBegin();
                for (int i = 1; i <= numTransactionsToAbort; i++)
                {
                    DSTMLib.CreatePadInt(i);
                    PadInt pad = DSTMLib.AccessPadInt(i);
                    pad.Write(10);
                }
                string startTime = "Started at: " + DateTime.Now.ToString("h:mm:ss tt");
                DSTMLib.TxAbort();
            string endTime = "Ended at: " + DateTime.Now.ToString("h:mm:ss tt");
            Console.WriteLine(startTime + " and " + endTime);
            Console.ReadLine();
        }
    }

    
}
