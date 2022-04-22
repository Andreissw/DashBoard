using DashBoard.Models;
using DashBoard.MyClass.SMT.Models.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DashBoard.MyClass.SMT.Models
{
    public class DashBoardSMT
    {
      
        public DashBoardSMT(string _nameline)
        {
            this._nameline = _nameline;
            GetFujiPG();
            ShLine = _nameline.Contains("line1") ? "Линия 1" : _nameline.Contains("line2") ? "Линия 2" : _nameline.Contains("line3") ? "Линия 3" : "Линия 4";
            Machine = GetMachine();
            FPY = new FPY(Machine);
            Plans = new Plans(_nameline, (int)Machine.CountRealease,Machine);
            GetListTimes();
            //Plans.FactCount;
            Table = new List<TableInfo>()
            {
                new TableInfo() {Name = "План:", Value = Plans.PlanCount.ToString()  },
                new TableInfo() {Name ="Факт:", Value = Plans.FactCount.ToString()},
                new TableInfo() {Name = Plans.StatusName+":", Value = Plans.DeviationPlan.ToString() },
                new TableInfo() {Name = "NF:", Value = Machine.CountNotVer.ToString()},
                new TableInfo() {Name = "FFPY:", Value  = FPY.FPYBefore.ToString()},
                new TableInfo() {Name ="FPY:", Value = FPY.FPYAfter.ToString()},
            };

            GetCharts();

        }

        public IMachine Machine { get; set; }

        Plans Plans { get; set; }

        FPY FPY { get; set; }    
        
        public List<TableInfo> Table { get; set; }

        public string IsStatusPrt { get; set; }

        public string FujiPG { get; set; }

        public string NameProblem { get; set; }

        public bool IsStatus { get; set; }

        public string ShLine { get; set; }

        private string _nameline { get; }

        public List<string> ListTime { get; set; }

        private void GetFujiPG()
        {
            using (var fas = new FASEntities())
            {
                FujiPG = fas.EP_Monitoring.Where(c => c.Description == _nameline).Select(c => c.ProgrammName).FirstOrDefault();
            }
        }

        private IMachine GetMachine()
        {
            return _nameline.Contains("Orbotec") ? (IMachine)new Orbotec() : new Omron(_nameline);
        }

        private IMachine GetMachine(Time time)
        {
            return _nameline.Contains("Orbotec") ? (IMachine)new Orbotec() : new Omron(_nameline,time);
        }

        private void GetCharts()
        {
            foreach (var item in ListTime)
            {
                var time = new Time() { TimeStart = DateTime.Parse(item).AddHours(-2).ToString("HH:mm:ss") };
                time.TimeEnd = DateTime.Parse(item).AddHours(-1).ToString("HH:mm:ss");

                var Mach = GetMachine(time);
                FPY _fpy = new FPY(Mach);

                Chart chart = new Chart()
                {
                    Time = item,
                    FPYChart = _fpy.FPYAfter,
                    CountRel = Mach.CountRealease ,
                    Countdef = Mach.CountDefVer,
                    //Color = short.Parse(_fpy.FPYAfter) < 90 ? "#FF4500" : "#9ACD32",

                };

                chart.Color = double.Parse(_fpy.FPYAfter) < 90 ? "#FF4500" : "#9ACD32";

                Machine.ListCharts.Add(chart);
            }
        }

        private void GetListTimes()
        {
            Time time = new Time();

            ListTime = time.Shift == "Дневная смена" ? 
                new List<string>() { "08:00","09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00" } :
                new List<string>() { "20:00", "21:00", "22:00", "23:00", "00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00", "07:00" };
        }

    }

    public class TOPDefects
    {
        public string ModelName { get; set; }
        public string Name { get; set; }
        public string Count { get; set; }
    }

    public class FPY
    {
        public FPY(IMachine M)
        {
            //if (M.ShrtPGName == "SberBox")
            //{
            //    Random rnd = new Random();
            //    var d = rnd.NextDouble();
            //    var f = 98.00 + d;
            //    FPYBefore = f.ToString("##.##");

            //     d = rnd.NextDouble();
            //     f = 98.00 + d;
            //    FPYAfter = f.ToString("##.##");
            //    return;
            ////}

            FPYBefore = (100 - M.CountDefAOI / (M.CountRealease) * 100).ToString("##.#");
            FPYAfter = (100 - M.CountDefVer / (M.CountRealease) * 100).ToString("##.#");

            FPYBefore = FPYBefore == "не число" ? "0" : FPYBefore;
            FPYAfter = FPYAfter == "не число" ? "0" : FPYAfter;

            FPYBefore = FPYBefore == "" ? "0" : FPYBefore;
            FPYAfter = FPYAfter == "" ? "0" : FPYAfter;

           
        }

        public string FPYBefore { get; }
        public string FPYAfter { get; }

    }

    public class Plans
    {
        FASEntities fas = new FASEntities();
        public Plans(string _nameline, int _factPlan, IMachine machine)
        {
            var plan = fas.FAS_Lines.Where(c => c.Description.Contains(_nameline)).Select(b => fas.FAS_ShiftPlan.Where(c => c.LineID == b.LineID &  machine.ModelID == c.ModelID).Select(c => c.ShiftPlan).FirstOrDefault()).FirstOrDefault();
            //if (plan == 0) { 
            
            //}

            PlanCount = plan;
            FactCount = _factPlan;

            GetDeviation(_nameline, machine.ShrtPGName);

            //DeviationPlan = ?
        }

        public int PlanCount { get; set; }
        public int FactCount { get; set; }
        public string DeviationPlan { get; set; }

        public string StatusName { get; set; }

        public bool Status { get; set; }

        private void GetDeviation(string nameline,string model)
        {
            if (PlanCount == 0)
            {
                DeviationPlan = "0";
                StatusName = "";
                Status = false;
                return;
            }

            var VypInSec = TimeSpan.Parse("12:00:00").TotalSeconds / PlanCount;
            var _worktime = getWorkTime();
            var countplan = (_worktime / VypInSec);
            DeviationPlan = (FactCount - countplan).ToString("##");


            //if (model == "SberBox") //Временно
            //{
               
                //if (int.Parse(DeviationPlan) <= 15)
                //{
                //    Random rnd = new Random();
                //    var co = rnd.Next(20, 25);
                //    DeviationPlan = FactCount == 0 ? "0" : co.ToString();
                //}
                
            //}
        

            if (int.Parse(DeviationPlan) < 0)
            {
                StatusName = "Отст";
                Status = false;
            }
            else
            {
                StatusName = "Опрж";
                Status = true;
            }

        }

        double getWorkTime()
        {
            var now = DateTime.UtcNow.AddHours(2);
            double _worktime = 0;

            if (now.Hour >= 8 & now.Hour <= 20)  _worktime = TimeSpan.Parse(now.AddHours(-8).ToString("HH:mm:ss")).TotalSeconds;            
            else
            {
                if (now.Hour >= 20 & now.Hour <= 23)  _worktime = _worktime = TimeSpan.Parse(now.AddHours(-20).ToString("HH:mm:ss")).TotalSeconds;
                else  _worktime = TimeSpan.Parse((now.AddHours(4)).ToString("HH:mm:ss")).TotalSeconds;               
            }

            return _worktime;
        }

        
    }
}