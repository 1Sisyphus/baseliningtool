using CommandLine;
using Microsoft.CodeAnalysis.Sarif;
using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Html;

namespace baselining_tool
{
    internal class Program
    {
        private static int log_status;

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
            log_status = 0;
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
            if (log_status == 0 && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(v);
            File.AppendAllText(filePath + "btlog.txt", sb.ToString());
            sb.Clear();
        }

        private static void analyseSarif(SarifLog sarifLog)
        {
            ResultStates prevStatus;
            ResultStates currStatus;


            int newCount = 0, absentCount = 0, unchangedCount = 0, updatedCount = 0;

            for (int i = 0; i < sarifLog.Runs[0].Results.Count; i++)
            {
                if (sarifLog.Runs[0].Results[i].BaselineState == BaselineState.New)
                {
                    newCount += 1;

                }
                else if (sarifLog.Runs[0].Results[i].BaselineState == BaselineState.Unchanged)
                {
                    unchangedCount += 1;

                }
                else if (sarifLog.Runs[0].Results[i].BaselineState == BaselineState.Absent)
                {
                    absentCount += 1;

                }
                else if (sarifLog.Runs[0].Results[i].BaselineState == BaselineState.Updated)
                {
                    updatedCount += 1;

                }
            }

            //has to be inside for-loop

            if ((newCount == sarifLog.Runs[0].Results.Count) && (updatedCount == 0) && (unchangedCount == 0))
            {
                //case of Previous Rule Pass
                 prevStatus = ResultStates.RulePasses;
             
            }
            else if ((absentCount == sarifLog.Runs[0].Results.Count) && (updatedCount == 0) && (unchangedCount == 0))
            {
                //case of Current Rule Pass
                currStatus = ResultStates.RulePasses;

            }
            else if (updatedCount + unchangedCount + absentCount == 1)
            {
                //case of Previous Single Defect
                prevStatus = ResultStates.SingleDefect;


            }
            else if (updatedCount + unchangedCount + newCount == 1)
            {
                //case of Currnet Single Defect
                currStatus = ResultStates.SingleDefect;

            }
            else if (updatedCount + unchangedCount + absentCount >= 2)
            {
                //case of Multiple Previous Defect
                prevStatus = ResultStates.MultipleDefect;

            }
            else if (updatedCount + unchangedCount + newCount >= 2)
            {
                //case of Multiple Current Defect
                currStatus = ResultStates.MultipleDefect;

            }

             
            

        }

        //should work for now but check html generator APIs
        private static void htmlGenerator(ResultStates rs, )
        {
            //Table start.
            string html = "<table cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;font-family:arial'>";

            //Adding HeaderRow.
            html += "<tr>";
            html += "<th style='background-color: #B8DBFD;border: 1px solid #ccc'>col_1</th>";//test header
            html += "<th style='background-color: #B8DBFD;border: 1px solid #ccc'>col_2</th>";//test header
            html += "</tr>";

            SarifLog log = SarifLog.Load(@"/Users/abeni/Projects/TestProj/testfile.sarif");//test spit
            for (int i = 0; i < log.Runs[0].Results.Count; i++)
            {
                //Adding DataRow.
                html += "<tr>";
                html += "<td style='width:120px;border: 1px solid #ccc'>" + log.Runs[0].Results[i].RuleId + "</td>";

                //Adding DataRow.

                html += "<td style='width:120px;border: Ï1px solid #ccc'>" + log.Runs[0].Results[i].BaselineState.ToString() + "</td>";
                html += "</tr>";
            }

            //Table end.
            html += "</table>";

            //Create the HTML file.
            File.WriteAllText(@"E:\Files\Console.htm", html);

            baselineLogger("Html File Created");
        }
    }
}

