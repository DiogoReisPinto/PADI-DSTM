using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PADIDSTM;
using System.Threading;

namespace ClientPerformance
{
    

    class ClientPerformance
    {
        static void Main(string[] args)
        {
            Thread.Sleep(2000);
            string arg = "C";
            crossedLocks(arg);
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
            int valor=-1;
            try
            {
                res = DSTMLib.TxBegin();
                PadInt result = DSTMLib.AccessPadInt(0);
                valor = result.Read();
                res = DSTMLib.TxCommit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                DSTMLib.TxAbort();
            }
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

        public static void crossedLocks(string arg)
        {
            
        bool res = false;
        PadInt pi_a, pi_b;
        DSTMLib.Init();
        if (arg.Equals("C")) {
            try{
            res = DSTMLib.TxBegin();
            pi_a = DSTMLib.CreatePadInt(1);
            pi_b = DSTMLib.CreatePadInt(2000000000);
            Console.WriteLine("####################################################################");
            Console.WriteLine("BEFORE create commit. Press enter for commit.");
            Console.WriteLine("####################################################################");
            DSTMLib.Status();
            Console.ReadLine();
            res = DSTMLib.TxCommit();
            Console.WriteLine("####################################################################");
            Console.WriteLine("AFTER create commit. commit = " + res + " . Press enter for next transaction.");
            Console.WriteLine("####################################################################");
            Console.ReadLine();
                        } catch (Exception e) {
                Console.WriteLine("Exception: " + e.Message);
                Console.WriteLine("####################################################################");
                Console.WriteLine("AFTER create ABORT. Commit returned " + res + " . Press enter for abort and next transaction.");
                Console.WriteLine("####################################################################");
                Console.ReadLine();
                DSTMLib.TxAbort();
            }

        }

        try { 
        res = DSTMLib.TxBegin();
        if ((arg.Equals("A") || (arg.Equals("C")))) {
            pi_b = DSTMLib.AccessPadInt(2000000000);
            pi_b.Write(211);
            Console.WriteLine("####################################################################");
            Console.WriteLine("Status post first op: write. Press enter for second op.");
            Console.WriteLine("####################################################################");
            DSTMLib.Status();
            Console.ReadLine();
            pi_a = DSTMLib.AccessPadInt(1);
            //pi_a.Write(212);
            Console.WriteLine("####################################################################");
            Console.WriteLine("Status post second op: read. uid(1)= " + pi_a.Read() + ". Press enter for commit.");
            Console.WriteLine("####################################################################");
            DSTMLib.Status();
            Console.ReadLine();
        } else {
            pi_a = DSTMLib.AccessPadInt(1);
            pi_a.Write(221);
            Console.WriteLine("####################################################################");
            Console.WriteLine("Status post first op: write. Press enter for second op.");
            Console.WriteLine("####################################################################");
            DSTMLib.Status();
            Console.ReadLine();
            pi_b = DSTMLib.AccessPadInt(2000000000);
            //pi_b.Write(222);
            Console.WriteLine("####################################################################");
            Console.WriteLine("Status post second op: read. uid(1)= " + pi_b.Read() + ". Press enter for commit.");
            Console.WriteLine("####################################################################");
            DSTMLib.Status();
            Console.ReadLine();
        }
        res = DSTMLib.TxCommit();
        Console.WriteLine("####################################################################");
        Console.WriteLine("commit = " + res + " . Press enter for verification transaction.");
        Console.WriteLine("####################################################################");
        Console.ReadLine();
        } catch (Exception e) {
            Console.WriteLine("Exception: " + e.Message);
            Console.WriteLine("####################################################################");
            Console.WriteLine("AFTER r/w ABORT. Commit returned " + res + " . Press enter for abort and next transaction.");
            Console.WriteLine("####################################################################");
            Console.ReadLine();
            DSTMLib.TxAbort();
        }

        try { 
        res = DSTMLib.TxBegin();
        PadInt pi_c = DSTMLib.AccessPadInt(1);
        PadInt pi_d = DSTMLib.AccessPadInt(2000000000);
        Console.WriteLine("0 = " + pi_c.Read());
        Console.WriteLine("2000000000 = " + pi_d.Read());
        Console.WriteLine("####################################################################");
        Console.WriteLine("Status after verification read. Press enter for verification commit.");
        Console.WriteLine("####################################################################");
        DSTMLib.Status();
        res = DSTMLib.TxCommit();
        Console.WriteLine("####################################################################");
        Console.WriteLine("commit = " + res + " . Press enter for exit.");
        Console.WriteLine("####################################################################");
        Console.ReadLine();
        } catch (Exception e) {
            Console.WriteLine("Exception: " + e.Message);
            Console.WriteLine("####################################################################");
            Console.WriteLine("AFTER verification ABORT. Commit returned " + res + " . Press enter for abort and exit.");
            Console.WriteLine("####################################################################");
            Console.ReadLine();
            DSTMLib.TxAbort();
        }

    }


        public static void testCycles(string arg) {
        bool res=false; int aborted = 0, committed = 0;

        DSTMLib.Init();
        try{
        if (arg.Equals("C")) {
            res = DSTMLib.TxBegin();
            PadInt pi_a = DSTMLib.CreatePadInt(2);
            PadInt pi_b = DSTMLib.CreatePadInt(2000000001);
            PadInt pi_c = DSTMLib.CreatePadInt(1000000000);
            pi_a.Write(0);
            pi_b.Write(0);
            res = DSTMLib.TxCommit();
        }
        Console.WriteLine("####################################################################");
        Console.WriteLine("Finished creating PadInts. Press enter for 300 R/W transaction cycle.");
        Console.WriteLine("####################################################################");
        Console.ReadLine();
            } catch (Exception e) {
                Console.WriteLine("Exception: " + e.Message);
                Console.WriteLine("####################################################################");
                Console.WriteLine("AFTER create ABORT. Commit returned " + res + " . Press enter for abort and next transaction.");
                Console.WriteLine("####################################################################");
                Console.ReadLine();
                DSTMLib.TxAbort();
            }
        for (int i = 0; i < 300; i++) {
            try {
            res = DSTMLib.TxBegin();
            PadInt pi_d = DSTMLib.AccessPadInt(2);
            PadInt pi_e = DSTMLib.AccessPadInt(2000000001);
            PadInt pi_f = DSTMLib.AccessPadInt(1000000000);
            int d = pi_d.Read();
            d++;
            pi_d.Write(d);
            int e = pi_e.Read();
            e++;
            pi_e.Write(e);
            int f = pi_f.Read();
            f++;
            pi_f.Write(f);
            Console.Write(".");
            res = DSTMLib.TxCommit();
            if (res) { committed++; Console.Write("."); } else {
                aborted++;
                Console.WriteLine("$$$$$$$$$$$$$$ ABORT $$$$$$$$$$$$$$$$$");
            }
            } catch (Exception e) {
                Console.WriteLine("Exception: FODASSE " + e.Message);
                Console.WriteLine("####################################################################");
                Console.WriteLine("AFTER create ABORT. Commit returned " + res + " . Press enter for abort and next transaction.");
                Console.WriteLine("####################################################################");
                //Console.ReadLine();
                DSTMLib.TxAbort();
                aborted++;
            }

        }
        Console.WriteLine("####################################################################");
        Console.WriteLine("committed = " + committed + " ; aborted = " + aborted);
        Console.WriteLine("Status after cycle. Press enter for verification transaction.");
        Console.WriteLine("####################################################################");
        DSTMLib.Status();
        Console.ReadLine();

        try{
        res = DSTMLib.TxBegin();
        PadInt pi_g = DSTMLib.AccessPadInt(2);
        PadInt pi_h = DSTMLib.AccessPadInt(2000000001);
        PadInt pi_j = DSTMLib.AccessPadInt(1000000000);
        int g = pi_g.Read();
        int h = pi_h.Read();
        int j = pi_j.Read();
        res = DSTMLib.TxCommit();
        Console.WriteLine("####################################################################");
        Console.WriteLine("2 = " + g);
        Console.WriteLine("2000000001 = " + h);
        Console.WriteLine("1000000000 = " + j);
        Console.WriteLine("Status post verification transaction. Press enter for exit.");
        Console.WriteLine("####################################################################");
        DSTMLib.Status();
        Console.ReadLine();
            } catch (Exception e) {
                Console.WriteLine("Exception: " + e.Message);
                Console.WriteLine("####################################################################");
                Console.WriteLine("AFTER create ABORT. Commit returned " + res + " . Press enter for abort and next transaction.");
                Console.WriteLine("####################################################################");
                Console.ReadLine();
                DSTMLib.TxAbort();
            }
    }
}
}


