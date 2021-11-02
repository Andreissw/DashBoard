using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DashBoard.MyClass
{
    public class DataPacking
    {

        public DataPacking()
        { 
        
        }        

        public int ID { get; set; }

        public string LineName { get; set; }

        public int Count { get; set; }

        public bool Breaks { get; set; }

        public int ShiftPlan { get; set; }

        public string StartBreak { get; set; }
        public string EndBreak { get; set; }

        public string leftTime { get; set; }

        public int Plancount { get; set; }

        public string FPY { get; set; }

        


    }
}