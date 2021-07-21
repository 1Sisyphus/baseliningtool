using System;
using Microsoft.CodeAnalysis.Sarif;
namespace baselining_tool
{

    class DriverInfo
    {
        private string name;
        private string path;

        public DriverInfo(string name, string path)
        {

            this.name = name;
            this.path = path;

        }

        public string Name
        {
            get => name;
            set
            {
                this.name = value;
            }
        }

        public string Path
        {
            get => path;
            set
            {
                this.path = value;
            }
        }
    }
}