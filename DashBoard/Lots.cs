using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DashBoard
{
    public class Lots
    {
        public Lots()
        {
            ListLots = new List<ListLots>();
            LotsLine = new List<ListLots>();
        }
        public List<ListLots> ListLots { get; set; }
        public List<ListLots> LotsLine { get; set; }
    }
   
    public class ListLots
    { 
        public string Name { get; set; }
        public int ID { get;set; }
    }
}