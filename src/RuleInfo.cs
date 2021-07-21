using System;

namespace baselining_tool
{

    class RuleInfo
    {

        private string name;
        private string id;
        private string severity;
        private string description;

        public RuleInfo(string name, string id, string severity, string description)
        {
            this.name = name;
            this.id = id;
            this.severity = severity;
            this.description = description;

        }

        public string Id
        {
            get => id;
        }

        public string Name
        {
            get => name;
        }

        public string Severity
        {
            get => severity;
        }

        public string Description
        {
            get => description;
        }


    }
}