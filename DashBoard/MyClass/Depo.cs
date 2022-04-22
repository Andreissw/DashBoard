using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DashBoard.MyClass
{
    public class Depo
    {
        public string name { get; set; }
        public string CountPacking { get; set; }
        public string CountTest { get; set; }

        public string Pass { get; set; }

        public string Fail { get; set; }

        public string Cycle { get; set; }

        public List<UserTop> userTops { get; set; }

        public Depo()
        {
            userTops = new List<UserTop>();
        }

    }

    public class UserTop
    {
        public int NUM { get; set; }
        public string UserName { get; set; }
        public int Count { get; set; }

        public int DisCount { get; set; }

        public string Cycle { get; set; }
    }
}