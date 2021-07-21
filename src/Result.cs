using System;
namespace baselining_tool
{
    public enum ResultStates
    {
        RulePasses = 1,
        SingleDefect = 2,
        MultipleDefect = 3,
        Other = 4
    };


    public enum FilterStates
    {
        New,
        Absent,
        Updated,
        Unchanged,
        All

    }

}
