using CommandLine;

namespace baselining_tool
{
    [Verb("baseline", isDefault: true, HelpText = "A baseline command for baselining-tool.exe. It outputs a regression report.")]
    internal class BaselineOptions
    {
        //[Value(0, HelpText = "value help")]
        //public string Input { get; set; }

        [Option('p', "path", HelpText = "A path to source directory that has build-dir.cmd in it. build-dir.cmd needed for database creation.")]
        public string InputFile
        {
            get; set;
        }

        [Option('q', "query", HelpText = "A path to a query or query suite. It should have .ql or .qls extension.")]
        public string QueryFile
        {
            get; set;
        }

        [Option('f', "filter", HelpText = "Output format for Current Regression Report. Choose from 'single defect, multiple defect, and rule passes.'")]
        public string Filter { get; set; }
    }
}
