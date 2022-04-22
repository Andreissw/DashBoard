using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace DashBoard.MyClass.SMT.Models
{
    public class Pages
    {

        public List<string> NameLines { get; set; }

        public Pages (DataRowCollection row)
        {
            NameLines = new List<string>() { "Orbotec_Line_4" };

            for (int i = 0; i < row.Count; i++) {
                var p = row[i].ItemArray;
                NameLines.Add(p[0].ToString());
            }
            
        }
    }
}