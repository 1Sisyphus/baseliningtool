using CommandLine;
using Microsoft.CodeAnalysis.Sarif;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections;

namespace baselining_tool
{
    public class Program
    {
        private static int log_status;
        private static ResultStates rs;

        private static Dictionary<DRRun, List<ResultInfo>> mDict = new Dictionary<DRRun, List<ResultInfo>>();

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
        private  static int BaselineCommand(BaselineOptions options)
        {
            string mFilePath = options.InputFile;
            string ext = Path.GetExtension(mFilePath);

            BLogger.baselineLogger("Create a log file\n");
            //initialize some var
            log_status = 1;
            rs = ResultStates.Undetermined;

            if (ext != ".sarif")
            {
                BLogger.baselineLogger("Expected a sarif file but got " + ext + "\n");
                Console.WriteLine("Didn't get a expected file format\n");
       
                Environment.Exit(1);
            }

            if (options.Filter != null)
            {
                switch (options.Filter.ToString().ToLower())
                {
                    case "single defect":
                        rs = ResultStates.SingleDefect;
                        break;
                    case "multiple defect":
                        rs = ResultStates.MultipleDefect;
                        break;
                    case "rule passes":
                        rs = ResultStates.RulePasses;
                        break;
                    default:
                        Console.WriteLine("Filter is not chosen from 'single defect, multiple defect, rule passes.' Choosing the default option: All resutls \n");
                        break;
                }
            }

            SarifLog sarifLog;
            try
            {
                sarifLog = SarifLog.Load(mFilePath);
                Console.WriteLine("Input recieved. Checking format...\n");
                BLogger.baselineLogger($"Input file {mFilePath} recieved\n");

                bool validFormat = checkFormat(sarifLog);
                if (validFormat == false)
                {
                    BLogger.baselineLogger("The saif file missing important properties.  \n");
                    Console.WriteLine("File is invalid. Check for presence of a Run, Results, or baselineState property\n");
                    Environment.Exit(1);
                }

                Console.WriteLine("analysing file ...\n");
                analyseSarif(sarifLog);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error occured while loading file " + mFilePath + "\n");
                Console.WriteLine(e.Message + "\n");
                Environment.Exit(1);
            }

            return 0;
        }



        public static bool checkFormat(SarifLog sarifLog)
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
            //if at least one Result is available for a Run, the result and all
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


        public static void analyseSarif(SarifLog sarifLog)
        {


            for (int i = 0; i < sarifLog.Runs.Count; i++)
            {
               


                for (int j = 0; j < sarifLog.Runs[i].Results.Count; j++)
                {

                    //initializations

                    ArtifactLocation artifactLocation = sarifLog.Runs[i].Results[j].Locations[0].PhysicalLocation.ArtifactLocation;
                    int ln = sarifLog.Runs[i].Results[j].Locations[0].PhysicalLocation.Region.StartLine;
                    string rId = sarifLog.Runs[i].Results[j].RuleId;
                    string rmessage = sarifLog.Runs[i].Results[j].Message.Text;
                    BaselineState bs = sarifLog.Runs[i].Results[j].BaselineState;
                    string dname = artifactLocation.Uri.ToString().Split('/')[0];
                    string rname = rId.Split('/')[rId.Split('/').Length - 1];


                    ResultInfo rInfo = new ResultInfo(
                        artifactLocation.Uri.ToString(),
                        ln,
                        rId,
                        bs);

                    DRRun available = CheckDict(mDict, dname, rname);

                    if (available == null)
                    {
                        //DRRun obj not in dic

                        DRRun dRRun = new DRRun(
                            new DriverInfo(dname, artifactLocation.UriBaseId + dname),
                            new RuleInfo(rname, rId, rmessage)
                        );
                        BLogger.baselineLogger("new driver-rule pair added " + dRRun.GetHashCode() + "\n");
                        List<ResultInfo> rInfoList = new List<ResultInfo>();
                        rInfoList.Add(rInfo);
                        mDict.Add(dRRun, rInfoList);

                    }
                    else
                    {
                        BLogger.baselineLogger("driver-rule pair is in dictionary. Updating value ... " + available.GetHashCode() + "\n");
                        List<ResultInfo> newValue = mDict[available];
                        newValue.Add(rInfo);
                        mDict[available] = newValue;

                    }

                }
            }

            prepBundleStates(mDict);
            displayResult(mDict);

        }

        private static void displayResult(Dictionary<DRRun, List<ResultInfo>> mDict)
        {
            //do other stuff related to result output
            HtmlGenerator.generateHTML(mDict);
        }

        private static void prepBundleStates(Dictionary<DRRun, List<ResultInfo>> mDict)
        {

            foreach (DRRun run in mDict.Keys)
            {
                int newCount = 0, unchangedCount = 0, updatedCount = 0, absentCount = 0;
                List<ResultInfo> rInfos = mDict[run];
                foreach (ResultInfo ri in rInfos)
                {

                    if (ri.BState == BaselineState.New)
                    {
                        newCount += 1;

                    }
                    else if (ri.BState == BaselineState.Unchanged)
                    {
                        unchangedCount += 1;

                    }
                    else if (ri.BState == BaselineState.Absent)
                    {
                        absentCount += 1;

                    }
                    else if (ri.BState == BaselineState.Updated)
                    {
                        updatedCount += 1;

                    }
                }

                List<ResultStates> rs = bundleStates(newCount, absentCount, unchangedCount, updatedCount, rInfos.Count);

                run.PreviousState = rs[0];
                run.CurrentState = rs[1];

            }

        }



        private static DRRun CheckDict(Dictionary<DRRun, List<ResultInfo>> mDict, string dname, string rname)
        {
            DRRun rval = null;
            foreach (DRRun run in mDict.Keys)
            {
                if (run.DriverInformation.Name == dname && run.RuleInformation.Name == rname)
                {
                    rval = run;
                    break;
                }
            }
            return rval;
        }


        private static List<ResultStates> bundleStates(
            int newCount,
            int absentCount,
            int unchangedCount,
            int updatedCount,
            int count
        )
        {

            List<ResultStates> rs = new List<ResultStates>();

            ResultStates prevStatus = ResultStates.Undetermined;
            ResultStates currStatus = ResultStates.Undetermined;

            if ((newCount == count) && (updatedCount == 0) && (unchangedCount == 0))
            {
                //case of Previous Rule Pass For a given driver
                prevStatus = ResultStates.RulePasses;


            }
            if ((absentCount == count) && (updatedCount == 0) && (unchangedCount == 0))
            {
                //case of Current Rule Pass
                currStatus = ResultStates.RulePasses;

            }
             if (updatedCount + unchangedCount + absentCount == 1)
            {
                //case of Previous Single Defect
                prevStatus = ResultStates.SingleDefect;


            }
             if (updatedCount + unchangedCount + newCount == 1)
            {
                //case of Currnet Single Defect
                currStatus = ResultStates.SingleDefect;

            }
             if (updatedCount + unchangedCount + absentCount >= 2)
            {
                //case of Multiple Previous Defect
                prevStatus = ResultStates.MultipleDefect;

            }
            if (updatedCount + unchangedCount + newCount >= 2) 
            {
                //case of Multiple Current Defect
                currStatus = ResultStates.MultipleDefect;

            }

            rs.Add(prevStatus);
            rs.Add(currStatus);

            return  rs;
        }

    }
}