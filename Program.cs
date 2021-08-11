using CommandLine;
using Microsoft.CodeAnalysis.Sarif;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

namespace baselining_tool
{
    public class Program
    {
        private static int log_status;
        private static ResultStates rs;
        private static Dictionary<DRRun, List<ResultInfo>> mDict = new Dictionary<DRRun, List<ResultInfo>>();
        private static bool prevPresent = true;
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<CreateOptions, BaselineOptions>(args).
                MapResult(
                (CreateOptions options) => CreateCommand(options),
                (BaselineOptions options) => BaselineCommand(options),
                _ => 1);
        }

        public static bool Prev
        {
            get => prevPresent;
        }

        public static int LogStatus
        {
            get => log_status;
        }
        public static ResultStates RS
        {
            get => rs;
        }

        private static int BaselineCommand(BaselineOptions options)
        {
            string fileprev = options.FilePrev;
            string filecurr = options.FileCurr;

            if (Path.GetExtension(fileprev) != ".sarif" || Path.GetExtension(filecurr) != ".sarif")
            {
                BLogger.baselineLogger("Analysis files should have a .sarif extension.\n");
                Console.WriteLine("Wrong file kind passed\n");
                Environment.Exit(1);
            }

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WDSBaselining");
            Directory.CreateDirectory(path);
            string baseSubDirectory = Path.Combine(path, "baselineStore");
            Directory.CreateDirectory(baseSubDirectory);


            Process p1 = new Process();
            p1.StartInfo.FileName = "cmd.exe";
            p1.StartInfo.CreateNoWindow = true;
            p1.StartInfo.RedirectStandardInput = true;
            p1.StartInfo.RedirectStandardOutput = true;
            p1.StartInfo.UseShellExecute = false;
            p1.Start();


            string time = DateTime.Now.ToString();
            string fullname = fileprev.Split(Path.DirectorySeparatorChar)[fileprev.Split(Path.DirectorySeparatorChar).Length - 1];
            string newlastfile = lastNameFinder(baseSubDirectory, fullname.Split('.')[0], "baseline");

            Console.WriteLine(newlastfile);

            p1.StandardInput.WriteLine("sarif match-results-forward " + filecurr + " " + "--previous " + fileprev + " " +
                "--output-file-path " + Path.Combine(baseSubDirectory, newlastfile));
            p1.StandardInput.Flush();
            Console.WriteLine("Setting up git...");
            p1.StandardInput.WriteLine("cd " + baseSubDirectory);
            p1.StandardInput.WriteLine("git add *");
            p1.StandardInput.WriteLine("git commit -m \" " + time + "\"");
            p1.StandardInput.WriteLine("git push origin main");
            Console.WriteLine("Pushed to GitHub\n");

            p1.StandardInput.Flush();
            p1.StandardInput.Close();
            p1.WaitForExit();
            Console.WriteLine(p1.StandardOutput.ReadToEnd());




            BLogger.baselineLogger("Create a log file\n");
            //initialize some var
            log_status = 1;
            rs = ResultStates.Undetermined;



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

            baselineAndCreateCommonTask(fullname.Split('.')[0]);

            return 0;

        }

        private static int CreateCommand(CreateOptions options)
        {
            string sourcePath = options.InputFile;
            string queryPath = options.QueryFile;

            string ext = Path.GetExtension(queryPath);
            if ((ext != ".ql") && (ext != ".qls"))
            {
                BLogger.baselineLogger("Query files should have .ql or .qls extension. A wrong file type passed.\n");
                Console.WriteLine("Query should have type .ql or .qls");
                Environment.Exit(1);

            }

            prepEnv(sourcePath, queryPath);

            BLogger.baselineLogger("Create a log file\n");
            //initialize some var
            log_status = 1;
            rs = ResultStates.Undetermined;



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


            baselineAndCreateCommonTask(sourcePath.Split(Path.DirectorySeparatorChar)[sourcePath.Split(Path.DirectorySeparatorChar).Length - 1]);


            return 0;
        }

        private static void baselineAndCreateCommonTask(string lname)
        {



            SarifLog sarifLog;
            try
            {
                string mpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WDSBaselining");
                string baseSubDirectory = Path.Combine(mpath, "baselineStore");
                string locName = lname;
                string fpath = lastBaselineFile(baseSubDirectory, locName);

                sarifLog = SarifLog.Load(fpath);
                Console.WriteLine("Input recieved. Checking format...\n");
                BLogger.baselineLogger($"Input file {fpath} recieved\n");

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
                Console.WriteLine("Error occured while loading file \n");
                Console.WriteLine(e.Message + "\n");
                Environment.Exit(1);
            }
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

        public static void prepEnv(string sourcePath, string queryPath)
        {
            //initializing directories

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WDSBaselining");
            Directory.CreateDirectory(path);
            string baseSubDirectory = Path.Combine(path, "baselineStore");
            Directory.CreateDirectory(baseSubDirectory);
            string analysisSubDirectory = Path.Combine(path, "analysis-files");
            Directory.CreateDirectory(analysisSubDirectory);
            string databseSubDirectory = Path.Combine(path, "databases");
            Directory.CreateDirectory(databseSubDirectory);

            //creating a process to handle command line executions
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();

            //name of driver or main source directory
            string locName = sourcePath.Split(Path.DirectorySeparatorChar)[sourcePath.Split(Path.DirectorySeparatorChar).Length - 1];

            DirectoryInfo di = new DirectoryInfo(Path.Combine(databseSubDirectory, locName));
            if (Directory.Exists(di.FullName))
            {
                Directory.Delete(di.FullName, true);
            }

            p.StandardInput.WriteLine("codeql database create " +
            "-l=cpp " +
            "-s=" + sourcePath + " " +
            "-c " + sourcePath + "/build-dir.cmd " +
            databseSubDirectory + @"/" + locName + "");

            p.StandardInput.WriteLine("codeql database analyze " +
                Path.Combine(databseSubDirectory, locName) + " " +
                "--format=sarifv2.1.0 " +
                "--output=" + Path.Combine(analysisSubDirectory, lastNameFinder(analysisSubDirectory, locName, "analysis")) + " " +
                queryPath);

            p.StandardInput.Flush();
            p.StandardInput.Close();
            p.WaitForExit();
            Console.WriteLine(p.StandardOutput.ReadToEnd());

            //A separate process for comparing analysis files.
            Process p1 = new Process();
            p1.StartInfo.FileName = "cmd.exe";
            p1.StartInfo.CreateNoWindow = true;
            p1.StartInfo.RedirectStandardInput = true;
            p1.StartInfo.RedirectStandardOutput = true;
            p1.StartInfo.UseShellExecute = false;
            p1.Start();

            string[] lastTwo = lastTwoAnalysis(analysisSubDirectory, locName);
            string lastbaseline = lastNameFinder(baseSubDirectory, locName, "baseline");

            p1.StandardInput.WriteLine("sarif match-results-forward " + analysisSubDirectory + "/" + lastTwo[1] + " " + "--previous " + analysisSubDirectory + "/" + lastTwo[0] + " " +
                "--output-file-path " + Path.Combine(baseSubDirectory, lastbaseline));

            Console.WriteLine("setting up git");
            p1.StandardInput.WriteLine("cd " + baseSubDirectory);
            p1.StandardInput.WriteLine("git add *");
            p1.StandardInput.WriteLine("git commit -m baseline_" +
                lastbaseline);
            p1.StandardInput.WriteLine("git push origin main");
            Console.WriteLine("pushed git");

            p1.StandardInput.Flush();
            p1.StandardInput.Close();
            p1.WaitForExit();
            Console.WriteLine(p1.StandardOutput.ReadToEnd());
        }

        private static string lastNameFinder(string directory, string locName, string id)
        {
            string matcher = "_analysis_";
            if (id == "baseline")
            {
                matcher = "_baseline_";
            }
            string[] mMatches = Directory.GetFiles(directory, locName + matcher + "*");
            if (mMatches.Length == 0)
            {
                return locName + matcher + 1 + ".sarif";
            }
            Array.Sort(mMatches);
            string lastFilePath = mMatches[mMatches.Length - 1];
            string fullname = lastFilePath.Split(Path.DirectorySeparatorChar)[lastFilePath.Split(Path.DirectorySeparatorChar).Length - 1];
            int lastNum = int.Parse(fullname.Split('.')[0].Split('_')[2]);
            string newName = locName + matcher + (lastNum + 1);
            return newName + ".sarif";
        }

        private static string lastBaselineFile(string directory, string locName)
        {
            string[] mMatches = Directory.GetFiles(directory, locName + "_baseline_*");
            Array.Sort(mMatches);
            return mMatches[mMatches.Length - 1];
        }

        private static string[] lastTwoAnalysis(string directory, string locName)
        {
            string[] mMatches = Directory.GetFiles(directory, locName + "_analysis*");
            Array.Sort(mMatches);
            string lastFilePath = mMatches[mMatches.Length - 1];
            string lastFileName = lastFilePath.Split(Path.DirectorySeparatorChar)[lastFilePath.Split(Path.DirectorySeparatorChar).Length - 1];

            if (mMatches.Length == 1)
            {
                prevPresent = false;
                return new string[] { lastFileName, lastFileName };
            }
            string secondToLastFilePath = mMatches[mMatches.Length - 2];
            string secondToLastFile = secondToLastFilePath.Split(Path.DirectorySeparatorChar)[secondToLastFilePath.Split(Path.DirectorySeparatorChar).Length - 1];
            return new string[] { secondToLastFile, lastFileName };
        }

        public static Dictionary<DRRun, List<ResultInfo>> dictData()
        {
            return mDict;
        }

        private static void displayResult(Dictionary<DRRun, List<ResultInfo>> mDict)
        {
            //do other stuff related to result output
            HtmlGenerator.generateHTML(mDict);
        }

        public static void prepBundleStates(Dictionary<DRRun, List<ResultInfo>> mDict)
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

            else if (updatedCount + unchangedCount + absentCount == 1)
            {
                //case of Previous Single Defect
                prevStatus = ResultStates.SingleDefect;

            }

            else if (updatedCount + unchangedCount + absentCount >= 2)
            {
                //case of Multiple Previous Defect
                prevStatus = ResultStates.MultipleDefect;

            }
            if ((absentCount == count) && (updatedCount == 0) && (unchangedCount == 0))
            {
                //case of Current Rule Pass
                currStatus = ResultStates.RulePasses;

            }
            else if (updatedCount + unchangedCount + newCount == 1)
            {
                //case of Currnet Single Defect
                currStatus = ResultStates.SingleDefect;

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

    }
}