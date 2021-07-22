using CommandLine;
using Microsoft.CodeAnalysis.Sarif;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace baselining_tool
{
    internal class Program
    {
        private static int log_status;
        private static FilterStates f;

        private static List<Run> runList = new List<Run>();

        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<BaselineOptions>(args).
                MapResult(
                (BaselineOptions options) => BaselineCommand(options),
                _ => 1);
        }

        public static int LogStatus
        {
            get => log_status;
        }
        private static int BaselineCommand(BaselineOptions options)
        {
            string mFilePath = options.InputFile;
            string ext = Path.GetExtension(mFilePath);

            //initialize some var
            log_status = 0;
            f = FilterStates.All;

            if (ext != ".sarif")
            {
                BLogger.baselineLogger("Expected a sarif file but got " + ext + "\n");
                Console.WriteLine("Didn't get a expected \n");
                Environment.Exit(1);
            }

            if (options.Filter != null)
            {
                switch (options.Filter.ToString().ToLower())
                {
                    case "new":
                        f = FilterStates.New;
                        break;
                    case "absent":
                        f = FilterStates.Absent;
                        break;
                    case "updated":
                        f = FilterStates.Updated;
                        break;
                    case "unchanged":
                        f = FilterStates.Unchanged;
                        break;
                    case "all":
                        f = FilterStates.All;
                        break;
                    default:
                        BLogger.baselineLogger("Filter is not chosen from 'new, unchanged, updated, absent, or all'" + "\n");
                        Console.WriteLine("Choose from available filter options \n");
                        Environment.Exit(1);
                        break;
                }
            }

            SarifLog sarifLog;
            try
            {
                sarifLog = SarifLog.Load(mFilePath);
                BLogger.baselineLogger("Input recieved. Checking format...\n");

                bool validFormat = checkFormat(sarifLog);
                if (validFormat == false)
                {
                    BLogger.baselineLogger("The saif file missing important properties. Check for presence of a Run, Results, or baselineState property \n");
                    Console.WriteLine("file is invalid.");

                    Environment.Exit(1);
                }
                BLogger.baselineLogger("File getting analysed is: " + mFilePath + "\n");

                //analyseSarif(sarifLog);
                HtmlGenerator.generateHTML(sarifLog);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error occured while loading file " + mFilePath + "\n");
                Console.WriteLine(e.Message + "\n");
                Environment.Exit(1);
            }



            return 0;
        }

        

        private static bool checkFormat(SarifLog sarifLog)
        {
            bool bstate = true;
            //first level check
            if (sarifLog == null ||
                sarifLog.Runs == null ||
                sarifLog.Runs.Count == 0
                )
            {
                return false;

            }
            //however, if at least one Result is available for a Run, the result and all
            //other remaining results in the Run should have a baselineState property.
            for (int k = 0; k < sarifLog.Runs.Count; k++)
            {
                if (sarifLog.Runs[k].Results.Count != 0)
                {
                    for (int i = 0; i < sarifLog.Runs[k].Results.Count; i++)
                    {
                        bstate = bstate && (sarifLog.Runs[k].Results[i].BaselineState != BaselineState.None);
                    }
                }
            }
            return bstate;
        }



        private static void analyseSarif(SarifLog sarifLog)
        {


            for (int i = 0; i < sarifLog.Runs.Count; i++)
            {
                int newCount = 0, absentCount = 0, unchangedCount = 0, updatedCount = 0;


                for (int j = 0; j < sarifLog.Runs[i].Results.Count; j++)
                {
                    if (sarifLog.Runs[i].Results[j].BaselineState == BaselineState.New)
                    {
                        newCount += 1;

                    }
                    else if (sarifLog.Runs[i].Results[j].BaselineState == BaselineState.Unchanged)
                    {
                        unchangedCount += 1;

                    }
                    else if (sarifLog.Runs[i].Results[j].BaselineState == BaselineState.Absent)
                    {
                        absentCount += 1;

                    }
                    else if (sarifLog.Runs[i].Results[j].BaselineState == BaselineState.Updated)
                    {
                        updatedCount += 1;

                    }
                }

                List<ResultStates> rs = getBStateForARun(
                        newCount,
                        absentCount,
                        unchangedCount,
                        updatedCount,
                        sarifLog,
                        i);
                //check needed for null values when displaying results.

                createARun(sarifLog, i, rs);


            }

            displayResult(runList);

        }

        private static void createARun(SarifLog sarifLog, int i, List<ResultStates> rs)
        {
            //only Rule[0] needed as others in the array are duplicates.
            RuleInfo ruleInfo = new RuleInfo(
                sarifLog.Runs[i].Tool.Driver.Rules[0].Name,
                sarifLog.Runs[i].Tool.Driver.Rules[0].Id,
                sarifLog.Runs[i].Tool.Driver.Rules[0].GetProperty("problem.severity"),
                sarifLog.Runs[i].Tool.Driver.Rules[0].ShortDescription.Text
            );

            //need a way to fix this.
            DriverInfo driverInfo = new DriverInfo
                (sarifLog.Runs[i].Artifacts[0].Location.Uri.GetFileName(),
                sarifLog.Runs[i].Artifacts[0].Location.Uri.GetFilePath()
            );

            var oneQueryonOneDriver = new Run(
                driverInfo,
                ruleInfo,
                rs[0],
                rs[1]);
            runList.Add(oneQueryonOneDriver);

        }

        private static List<ResultStates> getBStateForARun(
            int newCount,
            int absentCount,
            int unchangedCount,
            int updatedCount,
            SarifLog sarifLog,
            int runIndex

        )
        {

            List<ResultStates> rs = new List<ResultStates>();

            //other is a placeholder for future baselineState types
            ResultStates prevStatus = ResultStates.Other;
            ResultStates currStatus = ResultStates.Other;

            if ((newCount == sarifLog.Runs[runIndex].Results.Count) && (updatedCount == 0) && (unchangedCount == 0))
            {
                //case of Previous Rule Pass For a given driver
                prevStatus = ResultStates.RulePasses;

            }
            else if ((absentCount == sarifLog.Runs[runIndex].Results.Count) && (updatedCount == 0) && (unchangedCount == 0))
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

            rs.Add(prevStatus);
            rs.Add(currStatus);

            return rs;
        }


        private static void displayResult(List<Run> runList)
        {
            //stuff related to output display
            HtmlGenerator.generateHTML(runList);
        }
    }
}