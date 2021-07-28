using CommandLine;

namespace baselining_tool
{
    [Verb("baseline", isDefault: true, HelpText = "command to baseline consecutive runs.")]
    internal class BaselineOptions
    {
        [Value(0, HelpText = "value help")]
        public string Input { get; set; }

        [Option('p', "path", HelpText = "enter a path to baseline file")]
        public string InputFile
        {
            get; set;
        }

        [Option('f', "filter", HelpText = "choose output option from 'single defect', 'multiple defect', 'rule passes'. ")]
        public string Filter { get; set; }
    }
}
