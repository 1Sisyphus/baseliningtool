using CommandLine;

namespace baselining_tool
{
    [Verb("createAndBaseline", HelpText = "A baseline command for baselining-tool.exe. It creates database and then outputs a regression report.")]
    internal class CreateOptions
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

    [Verb("baseline", HelpText = "This command compares two baseline files.")]
    internal class BaselineOptions
    {
        [Option("previous", HelpText = "A path to previous analysis file.")]
        public string FilePrev
        {
            get; set;
        }

        [Option("current", HelpText = "A path to current analysis file.")]
        public string FileCurr
        {
            get; set;
        }

        [Option('f', "filter", HelpText = "Output format for Current Regression Report. Choose from 'single defect, multiple defect, and rule passes.'")]
        public string Filter { get; set; }
    }

}
