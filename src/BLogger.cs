using System;
using System.IO;
using System.Text;

namespace baselining_tool
{

    internal class BLogger
    {
        public static void baselineLogger(string v)
        {

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WDSBaselining");
            Directory.CreateDirectory(path);

            if (Program.LogStatus == 0 && File.Exists(Path.Combine(path, "btlog.txt")))
            {
                File.Delete(Path.Combine(path, "btlog.txt"));
                Console.WriteLine("Exisiting log deleted");
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(v);
            File.AppendAllText(path + "/btlog.txt", sb.ToString());
            sb.Clear();
        }
    }
}