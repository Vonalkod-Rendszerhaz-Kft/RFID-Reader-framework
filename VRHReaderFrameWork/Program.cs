using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameWork
{
    class Program
    {
        static void Main(string[] args)
        {
            string sCurrentDir = System.IO.Directory.GetCurrentDirectory();


            Console.WriteLine("VRHReaderFramework starter. Consol mode application. 2016.03.xx");
            Console.WriteLine("Please use the service for production environments.");

            Console.WriteLine("Press any key to stop the framework.");

            Console.WriteLine("");

            Console.WriteLine("Base directory: " + sCurrentDir);

            VRHReaderFrameworkMain.clsMainWorker.Start(sCurrentDir, "Debug executable");

            Console.Read();

            VRHReaderFrameworkMain.clsMainWorker.Stop();
        }
    }
}
