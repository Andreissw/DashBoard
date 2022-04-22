using DashBoard.Models;
using DashBoard.MyClass;
using DashBoard.MyClass.SMT.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace DashBoard.Controllers
{
    public class SMTController : Controller
    {
        // GET: SMT
        Entities ent = new Entities();
        FASEntities fas = new FASEntities();
        public ActionResult Index()
        {
            var ResLines = Connect.GetData(@"SELECT DISTINCT USR_MACHINE_NAME FROM CONNECTION_MACHINE_INFO WHERE USR_MACHINE_NAME NOT IN ('VT-S530-1087','VP9000-MC1')");
            return View(new Pages(ResLines.Tables[0].Rows));
        }

        public ActionResult GetDashBoard(string NameLine)
        {
            DashBoardSMT DashBoard = new DashBoardSMT(NameLine);
            var M = DashBoard.Machine;
            Time time = new Time();
            var date = DateTime.Parse(time.DateStart + " " + time.TimeStart);

            var result = ent.DTR_Downtime.Where(c => c.TRC_dtProductionArea.Desc == NameLine & c.CloseDate == null & c.StartDate > date).Select(c => c.ProductionAreaID).Distinct().FirstOrDefault();
            DashBoard.IsStatus = result == null ? true : false;

            DashBoard.NameProblem = ent.DTR_Downtime.Where(c => c.TRC_dtProductionArea.Desc == NameLine & c.CloseDate == null & c.StartDate > date)
                .Select(c => ent.DTR_DowntimeCode.Where(b => b.ID == c.CodeID).Select(b => b.Name).FirstOrDefault()).FirstOrDefault();

            var _line = fas.FAS_Lines.Where(c => c.Description == NameLine).Select(c => c.ShrtName).FirstOrDefault();

            var TOPBOT = DashBoard.Machine.ProgrammName.Contains("BOT") ? "BOT" : DashBoard.Machine.ProgrammName.Contains("TOP") ? "TOP" : "";

            var PGNameResult = fas.EP_PGName.Where(c => c.Name == DashBoard.Machine.ProgrammName).Select(c => c.Name == c.Name).FirstOrDefault();

            if (PGNameResult) { 

                var r = fas.EP_ProtocolsInfo.Where(c => c.EP_Protocols.NameProtocol == DashBoard.Machine.ProgrammName)
                   .Where(c => c.line == _line & c.EP_TypeVerification.Manufacter == "Цех поверхностного монтажа" & c.TOPBOT == TOPBOT & c.Visible == true & c.Start == false
              
                   ).ToList();

                var resl = fas.EP_ProtocolsInfo.Where(c => c.EP_Protocols.NameProtocol == DashBoard.Machine.ProgrammName)
                    .Where(c => c.line == _line & c.EP_TypeVerification.Manufacter == "Цех поверхностного монтажа" & c.TOPBOT == TOPBOT & c.Visible == true & c.Start == false
                    & (c.Result == null || c.Result == "NOK")
                    ).Count();

                DashBoard.IsStatusPrt = resl == 0 ? "true" : "false";
            }
            else
            {
                DashBoard.IsStatusPrt = "null";
            }

            



            ////if (DashBoard.Machine.ShrtPGName == "SberBox")
            ////{
            //    DashBoard.IsStatus = true;
            ////DashBoard.Table.
            ////}


            return View(DashBoard);
        }


    }

}