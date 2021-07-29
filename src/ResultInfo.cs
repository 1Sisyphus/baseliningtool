using System;
using Microsoft.CodeAnalysis.Sarif;

namespace baselining_tool{
    
    public class ResultInfo{
        private string _fileLocation;
        private int _lineNum;
        private string _ruleId;
        private BaselineState _bState;

        public ResultInfo(string fl, int ln, string rId, BaselineState bs){
            this._bState = bs;
            this._fileLocation = fl;
            this._ruleId = rId;
            this._lineNum = ln;
        }

        public string FileLocation{
            get => _fileLocation;
        }

        public int LineNum{
            get => _lineNum;
        }

        public string RuleId{
            get => _ruleId;
        }

        public BaselineState BState{
            get => _bState;
        }
    }
}