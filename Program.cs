using CommandLine;
using Microsoft.CodeAnalysis.Sarif;
using System;
using System.IO;
using System.Text;

namespace baselining_tool
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<BaselineOptions>(args).
                MapResult(
                (BaselineOptions options) => BaselineCommand(options),
                _ => 1);
        }
        private static int BaselineCommand(BaselineOptions options)
        {
            string mFilePath = options.InputFile;
            string ext = Path.GetExtension(mFilePath);
            if (ext != ".sarif")
            {
                baselineLogger("Expected a sarif file but got " + ext + "\n");
                //check exit codes for more details
                Environment.Exit(1);
            }

            SarifLog sarifLog = SarifLog.Load(mFilePath);
  
            baselineLogger("Input recieved. checking format...\n");
            bool validFormat = checkFormat(sarifLog);
            if (validFormat == false)
            {
                //I might be able to run sarif multitool inside this executible instead of running it initially??
                baselineLogger("The saif file is not a proper baseline file \n"); 
                Console.WriteLine("SarifLog is invalid.");

                Environment.Exit(1);
            }
            analyseSarif(sarifLog);

            return 0;
        }

        private static bool checkFormat(SarifLog sarifLog)
        {
            //enough ckeck for now
            if (sarifLog == null ||
                sarifLog.Runs == null ||
                sarifLog.Runs[0].Results == null ||
                sarifLog.Runs[0].Results.Count == 0)
            {
                return false;
            }


            return sarifLog.Runs[0].Results[0].BaselineState != BaselineState.None;
        }

        private static void baselineLogger(string v)
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory;
            StringBuilder sb = new StringBuilder();
            sb.Append(v);
            File.AppendAllText(filePath + "log.txt", sb.ToString());
            sb.Clear();
        }

        private static void analyseSarif(SarifLog sarifLog)
        {
            //do sth 
        }
    }
}

