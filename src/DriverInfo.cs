using System;
using Microsoft.CodeAnalysis.Sarif;
namespace baselining_tool
{

    class DriverInfo
    {
        private string _name;
        private string _path;

        public DriverInfo(string name, string path)
        {

            this._name = name;
            this._path = path;

        }

        public string Name
        {
            get => _name;
            set
            {
                this._name = value;
            }
        }

        public string Path
        {
            get => _path;
            set
            {
                this._path = value;
            }
        }
    }
}