using DashBoard.MyClass.SMT.Models.Interface;
using System.Collections.Generic;

namespace DashBoard.MyClass.SMT.Models
{
    public class Orbotec : IMachine
    {
        public float CountVer { get;  set; }
        public float CountRealease { get;  set; }
        public float CountDefAOI { get;  set; }
        public float CountDefVer { get;  set; }

        public float CountNotVer { get; set; }
        public string ShrtPGName { get; set; }

        public string ProgrammName { get; set; }
        public short ModelID { get; set; }
        public List<TOPDefects> ListTop { get; set; }

        public List<Chart> ListCharts { get; set; }
        private void GetDataFPY()
        {

        }
    }
}