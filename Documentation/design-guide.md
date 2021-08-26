How to run the regression analysis tool: 
1.	Baselinining-tool.exe createAndbaseline -p “path-to-source” -q “path-to-queryset” -f “filter-option”
-	Filter option can be “single defect”, “multiple defect”, and “rule passes”. Passing wrong type of argument will set the result type to default filter status, which is showing all result types. 
-	Query files should have file extension of .ql or .qls. If one wants to run multiple queries, 
-	they should add paths of .ql files in a .qls files and use the .qls file for -q option.
2.	Baselining-tool.exe baseline --previous “previous analysis” –current “current analysis” –filter “output-option”
-	Both previous and current command line arguments should be results of running analysis on a CodeQL database. 
For Windows driver samples
-	The path to source file should have a build-dir.cmd build script. If one wants to only build certain drivers/sources, they they should copy the build-dir.cmd to the directory that those selected drivers exist.
For razzle drivers
For razzle drivers, the baselining tool should only be used with the second option. The build-dir.cmd mentioned above is for drivers that have .sln file. For inbox drivers, the razzle-script.cmd file in Files folder of the baselining tool (baselining-tool\Files\razzle-script.cmd) should be updated and used according to the instructions given in the email thread “Windows drivers database on LGTM”. After that, two analysis files should be generated (when source code changes, or query changed, or query gets added to a query set). Then those two analysis files should be passed as arguments to the baselining tool using the second way given in How to run the regression analysis tool.

Requirements before using the tool
-	Users should download CodeQL CLI (github/codeql-cli-binaries: Binaries for the CodeQL CLI ) and add the codeql.exe file to user/system PATH in environment variables setting.
-	users should also install Sarif multitool (NuGet Gallery | Sarif.Multitool 2.4.11)
-	The path to MSBuild should also be added to PATH for Windows driver samples.

Output of running baselining-tool.exe 
-	the output of running the regression tool will be an html file in user’s Application Data directory. Ex, C:\Users\t-atamene\AppData\Roaming\WDSBaselining and also in a public remote repo. In my case it was finchnest/baselineStore (github.com)
Overview and Structure of the program
BaseliningOptions.cs
This file contains Verbs and Options for command line parser. 
Verb 
-	createAndBaseline: for creating a codeql database for Windows driver samples and also generate a regression report which will be stored both locally and on a public repo.
-	Baseline: for generating a regression report if two analysis files exist.
Options  
-	--path for path to source
-	--query for path to query
-	--filter for output options
-	--previous previous analysis file
-	--current current analysis file
Blogger.cs
Contains one function that logs status and messages to a .txt file. 
Public static void baseline(string s){
}
The log file will be generated in AppData directory like the final result html file.
DriverInfo.cs
Contains Properties for a Driver entity namly _name and _path.
DRRun.cs
Holds Properties that describe a single query run on a single driver/source path. The properties are:
-	Public DriverInfo DriverInformation{}
-	Public RuleInfo RuleInformation{}
-	Public ResultStates PreviousState{}
-	Public ResultStates CurrentState{}
PreviousState refers to the summarized result of running the query/rule on the driver in the previous run while CurrentState refers to the current result of running the query. 
Result.cs
Contains enum for determining output types. There are four types of results a user will be able to see.
Public enum ResultStates{RulePasses, SingleDefect, MultipleDefect, Undetermined }
ResultInfo.cs
-	Contains Properties for describing a state of each error in a single DRRun. A dictionary data structure is used to pair each DRRun to a list of ResultInfos. A ResultInfo is described by four Properties.
o	Public string FileLocation{}: the path where a given file could be found.
o	Public string LineNum{}: the line number where a failure to pass a rule is discovered.
o	Public string RuleId{}: the id of a rule that found an error in the given LineNum.
o	Public BaselineState BState{}: baseline state of each error in each file, at each line number. The BaselineState for a given observed error could be
?	New – result observed in the current run but missed in the baseline state
?	Absent – result observed in the current run but not observed in the current run either due to a query upgrade or source code change. 
?	Updated – changed in a way that the sarif multitool considers significant.
?	Unchanged – no change between the baseline file and the current run
?	Details on how the text comparison happens can be found here: Static Analysis Results Interchange Format (SARIF) Version 2.1.0 (oasis-open.org)

RuleInfo.cs
contains Properties to describe what it means to be a Rule. An object of this class will have Name, Id, and Message fields.

UI/HtmlGenerator.cs
This file contains functions that are needed for generating .html file. 
-	Private static string stringify(ResultStates rs){}: this function get string representation of a result state for display
-	Private static string bsStringify(BaselineState b){}: similar to stringify but for BaselineStates of each error instead of a DRRun.
-	Internal static void generateHTML(Dictionary<DRRun, List<ResultInfo>> mDict){}: this function goes over each key-value pairs in the dictionary input and creates a table row for each DRRun. Each DRRun will have another interior table that appears on click for displaying ResultInfo data. Only DRRuns whose CurrentState matches the filter criteria chosen by the user will appear in the final html result. Users can pass this filter argument while running the tool. Otherwise, the filter status will default to displaying all results.

Program.cs
This file contains function that are integral parts of the tool. 
-	int Main(string[] args): calls the appropriate function that handles command line arguments
-	int BaselineCommand(BaselineOptions options): this function first checks for the extension of analysis files, then it compares them. After that it stores the regression report both locally and on GitHub.
-	Int CreateCommand(CreateOptions options): this function first checks for the extension of query files, then it calls prepEnv function which prepares the environment. After that, it will behave similarly to BaselineCommand function in that it generates a regression report and stores it locally and on GitHub.
-	bool checkFormat(SarifLog log){}: this function loads the analysis file generated by CodeQL CLI and does some checking to make sure it’s a correct file for analysis.
-	Void analyseSarif(SarifLog log): this function extracts information from the analysis JSON file and  creates ResultInfo and DRRun objects. And then after checking for presence of (DRRun, ResultInfo List) pairs in the dictionary, it either updates the value in the dictionary or creates a new entry. Following that, it call prepBundleState and displayResult functions.
-	void prepEnv(string sourcePath, string queryPath){}: this function creates necessary directories where the database, analysis files, regression report files, and log files reside. Following that it creates a process for running CodeQL commands. These commands create, analyze, and then do regression reporting one after the other. 
-	String lastNamefinder(string directory, string locName, string id){}: this function gets a directory path and looks for the last file that starts with prefix locName. The use of this function is generating a file name that is alphanumerically greater than all other names in the given directory. The Id parameter is used to identify whether the file is baseline file or analysis file.
o	Naming convention is locName_id_number where number is the highest number in the directory. Ex, if the last file’s name in analysis directory is audio_analysis_2, then the new analysis file will be named audio_analysis_3.
-	String lastBaselineFile(string directory, string locName){}: this function is a little similar to lastNameFider but what is does is returning the last baseline/regression report file instead of a new name.
-	String[] lastTwoAnalysis(string directory, string locName){}: this function is also a little similar to the previous two. It returns the last two analysis files. If there is a single analysis file, it will return that single file. The analysis directory cannot be empty by the time this function is called, meaning it will at least have a single file because the application runs on a single thread.
-	Dictionary<DRRun, List<ResultInfo>> dictData(){}: this function provides public access to the dictionary data. Useful for testing.
-	Void displayResult(Dictionary<DRRun, List<ResultInfo>> mDict){}: this function calls the html generator function in  the UI directory.
-	Void prepBundleStates(Dictionary<DRRun, List<ResultInfo>> mDict){}: this function calculates the number of new, absent, updated, and unchanged results in a given driver/source path and passes the result to bundleStates function which uses the information to generate the final output result. 
-	DRRun checkDict(Dictionary<DRRun, List<ResultInfo>> mDict, string dname, string rname){}: checks for the presence of a DRRun in the dictionary and it either returns the object if it’s in the dictionary or null if it’s not.
-	List<ResultStates> bundleStates(int, int, int, int, int, int){}: this function uses the input from prepBundleStates to do some logic computation and determine status for a report of driver-rule pair. 
Testing
A few function and feature tests are implemented in this repo: finchnest/BaseliningTestProject (github.com) 

Next steps
-	Porting more SDV rules to CodeQL: more SDV DDI rules need to be ported to CodeQL for driver developers to happily use CodeQL queries for their driver verification.
-	Currently the query I wrote, ported rules, and existing CodeQL queries are used against three drivers and KMDF framework. More databases should be built and queried to ensure driver quality and possibly find bugs that may not get caught easily by SDV.
-	Email follow up should continue with the LGTM team until the problem with Windows Instances is fixed. (queries against windows instances have been falling since the update to LGTM on June 28 according to the email from the team.)
