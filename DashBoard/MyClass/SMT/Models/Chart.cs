using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DashBoard.MyClass.SMT.Models
{
    public class Chart
    {        

        public string Color { get; set; }
        public string FPYChart { get; set; }
        public decimal GPY { get; set; }
        public string Time { get; set; }

        public float Countdef { get; set; }

        public float CountRel { get; set; }
        
    }
}