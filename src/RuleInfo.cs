using System;

namespace baselining_tool
{

    class RuleInfo
    {

        private string _name;
        private string _id;
        private string _message;

        public RuleInfo(string name, string id, string message)
        {
            this._name = name;
            this._id = id;
            this._message = message;

        }

        public string Id
        {
            get => _id;
        }

        public string Name
        {
            get => _name;
        }


        public string Message
        {
            get => _message;
        }


    }
}