using System.Collections.Generic;

namespace DashBoard.MyClass.SMT.Models.Interface
{
    public interface IMachine
    {
        float CountVer { get;  set; }
        float CountRealease { get;  set; }
        float CountDefAOI { get;  set; }
        float CountDefVer { get;  set; }

        string ProgrammName { get; set; }

        string ShrtPGName { get; set; }

        short ModelID { get; set; }

        float CountNotVer { get; set; }

        List<TOPDefects> ListTop { get; set; }

        List<Chart> ListCharts { get; set; }

    }
}
