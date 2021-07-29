using System;

namespace baselining_tool
{

    public class DRRun
    {

        private DriverInfo _driverInfo;
        private RuleInfo _ruleInfo;

        private ResultStates _prevState;
        private ResultStates _currState;
        public DRRun(DriverInfo driverInfo, RuleInfo ruleInfo, ResultStates prevState = ResultStates.Undetermined, ResultStates currState = ResultStates.Undetermined)
        {
            this._driverInfo = driverInfo;
            this._ruleInfo = ruleInfo;
            this._prevState = prevState;
            this._currState = currState;
        }

        public DriverInfo DriverInformation
        {
            get => _driverInfo;
        }
        public RuleInfo RuleInformation
        {
            get => _ruleInfo;
        }

        public ResultStates PreviousState
        {
            get => _prevState;
            set{
                _prevState = value;
            }
        }

        public ResultStates CurrentState
        {
            get => _currState;
            set{
                _currState = value;
            }
        }


    }
}