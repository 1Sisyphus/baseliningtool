using System;

namespace baselining_tool
{

    class Run
    {

        private DriverInfo _driverInfo;
        private RuleInfo _ruleInfo;

        private ResultStates _prevState;
        private ResultStates _currState;
        public Run(DriverInfo driverInfo, RuleInfo ruleInfo, ResultStates prevState, ResultStates currState)
        {
            this._driverInfo = driverInfo;
            this._ruleInfo = ruleInfo;
            this._prevState = prevState;
            this._currState = currState;
        }

        public DriverInfo DriverInformation
        {
            get => _driverInfo;
            set
            {
                _driverInfo = value;
            }
        }
        public RuleInfo RuleInformation
        {
            get => _ruleInfo;
        }

        public ResultStates PreviousState
        {
            get => _prevState;
        }

        public ResultStates CurrentState
        {
            get => _currState;
        }


    }
}