using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MasterServer
{
    public class Program
    {
        private Dictionary<int, string> serversLocation = new Dictionary<int, string>();
        private Dictionary<int, int> padIntLocation = new Dictionary<int, int>();
        
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
