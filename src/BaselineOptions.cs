using CommandLine;

namespace baselining_tool
{
    [Verb("baseline", isDefault: true, HelpText = "change later.")]
    internal class BaselineOptions
    {
        [Value(0, HelpText = "value help")]
        public string Input { get; set; }

        [Option('p', "path", HelpText = "path help")]
        public string InputFile
        {
            get; set;
        }

        [Option('f', "filter", HelpText = "filter help")]
        public string Filter { get; set; }
    }
}
