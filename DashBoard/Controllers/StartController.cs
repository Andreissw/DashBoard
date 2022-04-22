﻿using DashBoard.Models;
using DashBoard.MyClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DashBoard.Controllers
{
    public class StartController : Controller
    {
        // GET: Start
        public ActionResult FAS()
        {
            var fas = new FASEntities();
            Lots lots = new Lots();
            lots.LotsLine = fas.FAS_Lines.Where(c => c.LineName.Contains("FAS Line")).Select(c => new ListLots() { Name = c.LineName }).ToList();

            lots.ListLots = fas.Ct_OperLog.Where(c => c.StepID == 6 & c.LOTID != null).GroupBy(c => c.LOTID).Where(c => c.Count() > 10).Select(c => new ListLots()
            {
                Name = fas.Contract_LOT.Where(b => b.ID == c.Key & b.IsActive == true).Select(b => b.FullLOTCode).FirstOrDefault(),
                ID = fas.Contract_LOT.Where(b => b.ID == c.Key & b.IsActive == true).Select(b => b.ID).FirstOrDefault(),

            }).OrderByDescending(c=>c.ID).Where(c=>c.ID != 0).ToList();

            //lots.ListLots = fas.Contract_LOT.OrderByDescending(c => c.ID).Select(c => new ListLots() { Name = c.FullLOTCode }).ToList();
            return View(lots);
        }

        public ActionResult GetLine(string LineName)
        {
            DataPacking shiftCounter;
            using (var fas = new FASEntities())
            {
                var lineid = fas.FAS_Lines.FirstOrDefault(c => c.LineName == LineName).LineID; //Получаем ключ линии
                var ShiftPlan = fas.FAS_ShiftPlan.FirstOrDefault(c => c.LineID == lineid).ShiftPlan; //Получаем план по ключу линии
                var Breaks = fas.FAS_Breaks.Where(c => c.LineID == lineid).Select(c => new { c.StartBreak, c.EndBreak }); //Получаем список перерывов по линии
                var literId = fas.FAS_Liter.Where(c => c.LineID == lineid).Select(c => c.ID).FirstOrDefault(); //Получаем ключ Литера

                var TimeNow = TimeSpan.Parse(DateTime.UtcNow.AddHours(2).ToString("HH:mm:ss")); //Текущее время в формате часы минуты секунды

                //Инициализируем класс и придаем туда свойства, которые имеем в коде (Имя линии и план на текущий день текущую линии)
                shiftCounter = new DataPacking() { LineName = LineName, ShiftPlan = ShiftPlan }; 
                ////var TimeNow = TimeSpan.Parse("11:35:00");
                ///
                
                foreach (var item in Breaks) //Определение перерывов
                {
                    if (TimeNow >= item.StartBreak & TimeNow <= item.EndBreak)
                    {
                        shiftCounter.Breaks = true; //Перерыв начался или продолжается
                        shiftCounter.StartBreak = item.StartBreak.ToString();  //Начало текущего перерыва
                        shiftCounter.EndBreak = item.EndBreak.ToString(); //Начало конца перерыва
                        shiftCounter.leftTime = (item.EndBreak - TimeNow).Value.ToString(); //Осталось до конца перерыва
                        break; //Прекращение цикла
                    }
                }

               var Ls = Breaks.Select(c=>c.StartBreak).ToList(); //Список начало перерывов
               var Le = Breaks.Select(c => c.EndBreak).ToList(); //Список конца перерывов


                var TimeSecond = ((TimeSpan)Ls[0] - TimeSpan.Parse("08:00:00") +  //Всего секунд работают люди
                    (TimeSpan)Ls[1] - (TimeSpan)Le[0] + 
                    (TimeSpan)Ls[2] - (TimeSpan)Le[1] +
                    (TimeSpan)Ls[3] - (TimeSpan)Le[2] +
                    (TimeSpan)Ls[4] - (TimeSpan)Le[3] +
                    (TimeSpan)Ls[5] - (TimeSpan)Le[4]).TotalSeconds;

                var Result = ( TimeSecond / ShiftPlan); // Кол-во выпуска в секунду


                var r = (TimeNow - TimeSpan.Parse("08:00:00")).TotalSeconds;

                var cTimes = Breaks.Where(c => c.StartBreak.Value < TimeNow ).ToList();

                TimeSpan timeSpan = TimeSpan.Parse("00:00:00");
                foreach (var item in cTimes) {

                    if (TimeNow > item.StartBreak & TimeNow < item.EndBreak)
                    {
                        timeSpan += TimeNow - (TimeSpan)item.StartBreak; continue;
                    }

                    timeSpan += (TimeSpan)item.EndBreak - (TimeSpan)item.StartBreak;
                }

                r -= timeSpan.TotalSeconds;
               

                var Re = (int)(r / Result);

                var one = DateTime.UtcNow.AddDays(0).ToString("dd.MM.yyyy");
                var two = DateTime.Parse("07:55:00").ToString("HH:mm:ss");
                var three = DateTime.Parse("20:00:00").ToString("HH:mm:ss");

                //--------------------------------------------------------------------
                var now = DateTime.Parse( one + " " + two );
                var nowLast = DateTime.Parse(one + " " + three);

                Data data;
                //decimal discount;

                data = GetDataContract(now, nowLast, lineid);

                //if (data == null)
                //{
                //    //Packing = fas.FAS_PackingGS.OrderByDescending(c => c.PackingDate).
                //    //         Where(c => c.LiterID == literId
                //    //         & c.PackingDate >= now).Count();
                    
                //}

                if (data != null)
                {
                    decimal fpy = 0;
                    try
                    {
                         fpy = 100 - (data.DissCount / (decimal)(data.PackingCount + data.DissCount) * 100);
                    }
                    catch (DivideByZeroException ex)
                    {
                        
                    }
                   
                    shiftCounter.FPY = fpy.ToString("#.##");
                    shiftCounter.Count = data.PackingCount;
                    shiftCounter.Plancount = shiftCounter.Count - Re;
                    shiftCounter.ModelName = data.Model;

                    //if (shiftCounter.ModelName == "SberBox")//Временно
                    //{
                    //    shiftCounter.Count += 1000;
                    //    if (shiftCounter.Plancount <= 0)
                    //    {
                    //        Random rnd = new Random();
                    //        var co = rnd.Next(20, 29);
                    //        shiftCounter.Plancount = co;
                    //    }

                    //    if (shiftCounter.Count >= 0)
                    //    {
                    //        if ( decimal.Parse(shiftCounter.FPY) < 98)
                    //        {
                    //            Random rnd = new Random();
                    //            var d = rnd.NextDouble();
                    //            var f = 98.00 + d;
                    //            shiftCounter.FPY = f.ToString("##.##");
                    //        }
                    //    }


                    //}
                }        

            }

            return View (shiftCounter);
        }

        Data GetDataContract(DateTime? now,DateTime? nowLast, int line)
        {
            Data data = new Data();
            List<DateTime?> Ch = new List<DateTime?>();
            //int IDSber = 20109;
            using (var fas = new FASEntities())
            {
                //return fas.Ct_OperLog.Where(c => c.LOTID == IDSber & c.StepID == 6 & c.TestResultID == 2 & c.StepDate > now & c.LineID == line).Count();

                Ch.Add(fas.Ct_OperLog.Where(c => c.StepID == 6 & c.TestResultID == 2 & c.StepDate >= now & c.StepDate <= nowLast & c.LineID == line).OrderByDescending(c=>c.StepDate).Select(c => c.StepDate).FirstOrDefault());
                Ch.Add(fas.FAS_PackingGS.OrderByDescending(c => c.PackingDate).
                             Where(c => fas.FAS_Liter.Where(b => b.LineID == line).FirstOrDefault().ID == c.LiterID
                             & c.PackingDate >= now & c.PackingDate <= nowLast).Select(c=>c.PackingDate).FirstOrDefault());

                if (Ch.First() == DateTime.MinValue & Ch.Last() == DateTime.MinValue)
                {
                    return null;
                }

                if (Ch.First() >= Ch.Last())
                {
                    var LOTID = (int)fas.Ct_OperLog.Where(c => c.StepID == 6 & c.TestResultID == 2 & c.StepDate > now & c.StepDate <= nowLast  & c.LineID == line).FirstOrDefault().LOTID;
                    data.PackingCount = fas.Ct_OperLog.Where(c => c.LOTID == LOTID & c.StepID == 6 & c.TestResultID == 2 & c.StepDate > now & c.StepDate <= nowLast  & c.LineID == line).Count();
                    data.Model = fas.FAS_Models.Where(c => fas.Contract_LOT.Where(b => b.ID == LOTID).FirstOrDefault().ModelID == c.ModelID).FirstOrDefault().ModelName;

                    List<short?> steps = new List<short?>() { 1, 8,44 };
                    data.DissCount = fas.Ct_OperLog.Where(c => c.LOTID == LOTID & steps.Contains(c.StepID) & c.TestResultID == 3 & c.StepDate > now & c.StepDate <= nowLast & c.ErrorCodeID != 580).Count();
                }
                else 
                {
                    if (Ch.Last() != null)
                    {
                        if (Ch.Last() != DateTime.MinValue)
                        {

                      
                        var lotid = fas.FAS_PackingGS.OrderByDescending(c => c.PackingDate).
                                  Where(c => fas.FAS_Liter.Where(b => b.LineID == line).FirstOrDefault().ID == c.LiterID
                                  & c.PackingDate >= now & c.PackingDate <= nowLast).FirstOrDefault().LOTID;

                        data.PackingCount = fas.FAS_PackingGS.OrderByDescending(c => c.PackingDate).
                                  Where(c => fas.FAS_Liter.Where(b => b.LineID == line).FirstOrDefault().ID == c.LiterID
                                  & c.PackingDate >= now & c.PackingDate <= nowLast  & c.LOTID == lotid).Count();
                        data.DissCount = fas.FAS_Disassembly.Where(c => c.DisAssemblyLineID == line & c.DisassemblyDate > now & c.DisassemblyDate <= nowLast & c.LOTID == c.LOTID).Count();
                        data.Model = fas.FAS_Models.Where(c=>c.ModelID == fas.FAS_GS_LOTs.Where(b => b.LOTID == lotid).FirstOrDefault().ModelID).FirstOrDefault().ModelName;

                        }
                    }
                }   
                
            }

            return data;
        }

        //decimal GetCountDisSber(DateTime now)
        //{
        //    int IDSber = 20109;
        //    using (var fas = new FASEntities())
        //    {
        //        List<short?> steps = new List<short?>() {1,8 };
        //        return decimal.Parse( fas.Ct_OperLog.Where(c => c.LOTID == IDSber & steps.Contains(c.StepID) & c.TestResultID == 3 & c.StepDate > now).Count().ToString());
        //    }
        //}

        List<DateTime> GetUserDate(string datest, string dateend)
        {
            List<DateTime> dates = new List<DateTime>();

            if (datest == null)
            {
                dates.Add(DateTime.Parse(DateTime.UtcNow.AddHours(2).ToString("dd.MM.yyyyy") + " 08:00:00"));
            }
            else
            {
                dates.Add(DateTime.Parse(DateTime.Parse(datest).ToString("dd.MM.yyyy HH:mm:ss")));
                dates.Add(DateTime.Parse(DateTime.Parse(dateend).ToString("dd.MM.yyyy HH:mm:ss")));
            }
            

            return dates;
        }

        List<UserTop> GetUserTOP(int lotid, List<DateTime> Dates)
        {
            List<UserTop> userTops = new List<UserTop>();

            using (var fas = new FASEntities())
            {
                if (Dates.Count == 1)
                {                
                    var date = Dates.FirstOrDefault();
                    userTops = fas.Ct_OperLog.Where(c => c.LOTID == lotid & c.StepDate >= date & c.StepID == 1 & c.TestResultID != 3).GroupBy(c => c.StepByID).Select(c => new UserTop()
                    {
                        UserName = fas.FAS_Users.Where(b => b.UserID == c.Key.Value).Select(b => b.UserName).FirstOrDefault(),
                        Count = c.Count(),
                        DisCount = fas.Ct_OperLog.Where(b => b.LOTID == lotid & b.StepDate >= date & b.StepID == 1 & b.StepByID == c.Key.Value & b.TestResultID == 3).Count(),
                        
                    }).OrderByDescending(c => c.Count).ToList();
                }
                else
                {
                    var date = Dates[0];
                    var date2 = Dates[1];
                    userTops = fas.Ct_OperLog.Where(c => c.LOTID == lotid & c.StepDate >= date & c.StepDate <= date2  & c.StepID == 1).GroupBy(c => c.StepByID).Select(c => new UserTop()
                    {
                        UserName = fas.FAS_Users.Where(b => b.UserID == c.Key.Value).Select(b => b.UserName).FirstOrDefault(),
                        Count = c.Count(),
                        DisCount = fas.Ct_OperLog.Where(b => b.LOTID == lotid & b.StepDate >= date & b.StepDate <= date2  & b.StepID == 1 & b.StepByID == c.Key.Value & b.TestResultID == 3).Count(),
                        
                    }).OrderByDescending(c => c.Count).ToList();

                    foreach (var item in userTops)
                        if (item.Count + item.DisCount > 0)
                            item.Cycle = (36600 / (item.Count + item.DisCount) ).ToString("##.##") + " Сек.";
                }

                

              
            }

            return userTops;
        }

        int[] GetData(int lotid,List<DateTime> Dates)
        {
            int[] pactest = new int[4];
            using (var fas = new FASEntities())
            {          
                if (Dates.Count == 1)
                {
                    var date = Dates[0];
                    pactest[0] = fas.Ct_OperLog.Where(c => c.LOTID == lotid & c.StepDate >= date & c.StepID == 6).Count();
                    pactest[1] = fas.Ct_OperLog.Where(c => c.LOTID == lotid & c.StepDate >= date & c.StepID == 1).Count();
                    pactest[2] = fas.Ct_OperLog.Where(c => c.LOTID == lotid & c.StepDate >= date & c.StepID == 1 & c.TestResultID == 2).Count();
                    pactest[3] = fas.Ct_OperLog.Where(c => c.LOTID == lotid & c.StepDate >= date & c.StepID == 1 & c.TestResultID == 3).Count();
                }
                else
                {
                    var date = Dates[0];
                    var date2 = Dates[1];
                    pactest[0] = fas.Ct_OperLog.Where(c => c.LOTID == lotid & c.StepDate >= date & c.StepDate <= date2 & c.StepID == 6).Count();
                    pactest[1] = fas.Ct_OperLog.Where(c => c.LOTID == lotid & c.StepDate >= date & c.StepDate <= date2 & c.StepID == 1).Count();
                    pactest[2] = fas.Ct_OperLog.Where(c => c.LOTID == lotid & c.StepDate >= date & c.StepDate <= date2 & c.StepID == 1 & c.TestResultID == 2).Count();
                    pactest[3] = fas.Ct_OperLog.Where(c => c.LOTID == lotid & c.StepDate >= date & c.StepDate <= date2 & c.StepID == 1 & c.TestResultID == 3).Count();
                }
            }


            return pactest;
        }

        public ActionResult GetDepo(string name, string datest, string dateend)
        {
            var depo = new Depo();
            depo.name = name;
            
            
            using (var fas = new FASEntities())
            {
                var lotid = fas.Contract_LOT.Where(c => c.FullLOTCode == name).FirstOrDefault().ID;

                var dates = GetUserDate(datest, dateend);
                var _top = GetUserTOP(lotid, dates);

                for (int i = 0; i < _top.Count; i++)             
                    _top[i].NUM = i + 1;
               
                //List<UserTop> _top = fas.Ct_OperLog.Where(c => c.LOTID == lotid &  c.StepDate >= date & c.StepID == 1).GroupBy(c => c.StepByID).Select(c => new UserTop() { 
                //    UserName = fas.FAS_Users.Where(b=>b.UserID == c.Key.Value).Select(b=>b.UserName).FirstOrDefault(),
                //    Count = c.Count(),
                //}).OrderByDescending(c=>c.Count).ToList();

                depo.userTops.AddRange(_top);

                if (dates.Count > 1)  depo.Cycle = (36600 / (_top.Sum(c => c.Count) + _top.Sum(c => c.DisCount) )).ToString("##.##") + " Сек.";
               

                var pactest = GetData(lotid,dates);

                var countPac = pactest[0];
                var counttest = pactest[1];
                var Pass = pactest[2];
                var Fail = pactest[3];

                depo.CountTest = counttest.ToString() + "шт.";
                depo.Pass = Pass.ToString();
                depo.Fail = Fail.ToString();
                depo.CountPacking = countPac.ToString() + "шт.";

                if (dates.Count != 1)
                {
                    ViewData["Type"] = "False";
                }
            }
         


            return View(depo);
        }       

        double BreaksFor(double r,TimeSpan time, List<TimeSpan?> Ls, List<TimeSpan?> Le, int index)
        {
            double t = r;
            if (time <= Ls[index])
            {
                if (index == 0){
                    t = time.TotalSeconds;
                }
                else {

                    int i = index;
                    while (i == 0)
                    {
                        time -= (TimeSpan)Le[i] - (TimeSpan)Ls[i];
                        i -= 1;
                    }

                    t = time.TotalSeconds;
                }
            }
            else if (time >= Ls[index])
            {        
                
                if (index == 0){
                    if (time <= Le[index]) { 
                        t = (Ls[index] - TimeSpan.Parse("08:00:00")).Value.TotalSeconds;
                    }
                    else {
                        t = (time - (Ls[index] - TimeSpan.Parse("08:00:00"))).Value.TotalSeconds;
                    }
                }
                else
                {
                    int i = index;
                    while (i == 0)
                    {
                        time -= (TimeSpan)Le[i] - (TimeSpan)Ls[i];
                        i -= 1;
                    }

                    t = time.TotalSeconds;
                }
            }



            //r -= ((TimeSpan)Le[index] - (TimeSpan)Ls[index]).TotalSeconds;



            return t;
        }

        //decimal dis(int lineid, DateTime time)
        //{
        //    using (var fas = new FASEntities())
        //    {
        //        return 
        //    }
        //}
    }
}