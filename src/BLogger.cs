using System;
using System.IO;
using System.Text;

namespace baselining_tool
{

    internal class BLogger
    {
        public static void baselineLogger(string v)
        {

            string filePath = AppDomain.CurrentDomain.BaseDirectory;
            if (Program.LogStatus == 0 && File.Exists(filePath + "btlog.txt"))
            {
                File.Delete(filePath + "btlog.txt");
                Console.WriteLine("Exisiting log deleted");
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(v);
            File.AppendAllText(filePath + "btlog.txt", sb.ToString());
            sb.Clear();
        }
    }
}