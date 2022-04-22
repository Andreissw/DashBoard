using DashBoard.Models;
using DashBoard.MyClass.SMT.Models.Interface;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DashBoard.MyClass.SMT.Models
{
    public class Omron : IMachine
    {
        public Omron(string _nameLine)
        {
            nameLine = _nameLine;
            //Time = new Time();
            GetDataFPY(new Time());
            //GetTop();
            GetPGName();
            ListCharts = new List<Chart>();
        }

        public Omron(string _nameLine, Time _time)
        {
            nameLine = _nameLine;            
            GetDataFPY(_time);           
        }


        FASEntities fas = new FASEntities();

        private string nameLine;
        public string ProgrammName { get; set; }  
        public float CountVer { get; set; }
        public float CountRealease { get; set; }
        public float CountDefAOI { get; set; }
        public float CountDefVer { get; set; }
        public float CountNotVer { get; set; }
        public string ShrtPGName { get; set; }

        public short ModelID { get; set; }

        public List<Chart> ListCharts { get; set; }
        private Time Time { get; }
        public List<TOPDefects> ListTop { get; set; }

        //private void GetTop()
        //{
        //    var _result = Connect.GetData($@"SELECT DISTINCT(four.CIR_NAME), MOD.PG_NAME ,  COUNT(four.CIR_NAME) FROM PRISM.INSP_RESULT_SUMMARY_INFO one  LEFT JOIN COMP_RESULT_INFO two ON one.INSP_ID = two.INSP_ID
        //        LEFT JOIN COMP_INFO three ON one.PG_ITEM_ID = three.PG_ITEM_ID AND two.COMP_ID = three.COMP_ID  
        //        LEFT JOIN USR_INSP_RESULT_NAME six ON one.INSP_RESULT_CODE = six.USR_INSP_RESULT_CODE 
        //        LEFT JOIN CIR_INFO four ON three.CIR_ID = four.CIR_ID AND  three.PG_ITEM_ID = four.PG_ITEM_ID
        //        Left JOIN PG_INFO mod ON one.PG_ITEM_ID = mod.PG_ITEM_ID
        //        INNER JOIN CONNECTION_MACHINE_INFO L ON one.SYS_MACHINE_NAME = l.SYS_MACHINE_NAME
        //        WHERE INSP_END_DATE BETWEEN TO_DATE(CONCAT('{Time.DateStart}','{Time.TimeStart}'),'DD.MM.YY HH24:MI:SS') AND TO_DATE(CONCAT('{Time.DateEnd}','{Time.TimeEnd}'),'DD.MM.YY HH24:MI:SS')    
        //        AND one.INSP_RESULT_CODE<>0 AND one.VC_LAST_RESULT_CODE<>0  AND two.INSP_RESULT_CODE<>0 AND two.VC_LAST_RESULT_CODE<>0 AND six.LANG_ID = 3                 
        //        AND L.USR_MACHINE_NAME ='VT-S730H-0022-line3' 
        //        GROUP BY four.CIR_NAME,MOD.PG_NAME   ORDER BY COUNT(four.CIR_NAME) DESC");

        //    if (_result is null) return;

        //    var _r = _result.Tables[0];

        //    if (_r.Rows.Count != 0) ListTop = GetTop(_r);

        //}

        //private List<TOPDefects> GetTop(DataTable data)
        //{
        //    List<TOPDefects> top = new List<TOPDefects>();

        //    for (int i = 0; i < 3; i++)
        //    {
        //        var R = data.Rows[i].ItemArray;
        //        top.Add( new TOPDefects() { Name = R[0].ToString(), ModelName = R[1].ToString(), Count = R[2].ToString() });
               
        //    }

        //    return top;

        //}

        private void GetPGName()
        {
            ProgrammName = Connect.GetStr($@"SELECT PG_NAME FROM (SELECT pi.PG_NAME FROM PRISM.INSP_RESULT_SUMMARY_INFO insp
            LEFT JOIN PRISM.PG_INFO pi ON pi.PG_ITEM_ID=insp.PG_ITEM_ID
            LEFT JOIN CONNECTION_MACHINE_INFO cmi ON INSP.SYS_MACHINE_NAME = cmi.SYS_MACHINE_NAME
             WHERE cmi.USR_MACHINE_NAME='{nameLine}' ORDER BY INSP_BEGIN_DATE DESC) WHERE ROWNUM=1");

         
            var listModels = fas.FAS_Models;

            var list = listModels.Select(c => c.ModelName).ToList();

            foreach (var item in ProgrammName.Split('_'))           
                if (listModels.Select(c=>c.ModelName).Contains(item))
                {
                    var result = listModels.Where(c => c.ModelName == item).FirstOrDefault();
                    ShrtPGName = result.ModelName;
                    ModelID = result.ModelID;

                    return;
                }

            ShrtPGName = ProgrammName;
                
            

            //ProgrammName = ProgrammName.Split('_');

        }

        private void GetDataFPY(Time d)
        {
             CountDefAOI = Connect.GetInt($@"SELECT COUNT(*) FROM PRISM.INSP_RESULT_SUMMARY_INFO result 
             LEFT JOIN SEG_RESULT_INFO sri ON result.INSP_ID = sri.INSP_ID AND result.SYS_MACHINE_NAME = sri.SYS_MACHINE_NAME 
             LEFT JOIN PG_INFO pi ON result.PG_ITEM_ID = pi.PG_ITEM_ID  
             LEFT JOIN CONNECTION_MACHINE_INFO cmi ON result.SYS_MACHINE_NAME = cmi.SYS_MACHINE_NAME
             WHERE INSP_END_DATE BETWEEN TO_DATE(CONCAT('{d.DateStart}','{d.TimeStart}'),'DD.MM.YY HH24:MI:SS') AND TO_DATE(CONCAT('{d.DateEnd}','{d.TimeEnd}'),'DD.MM.YY HH24:MI:SS') 
             AND cmi.USR_MACHINE_NAME='{nameLine}'
             AND sri.INSP_RESULT_CODE<>0 AND pi.PG_NAME=(SELECT PG_NAME FROM (SELECT pi.PG_NAME 
             FROM PRISM.INSP_RESULT_SUMMARY_INFO insp LEFT JOIN PRISM.PG_INFO pi ON pi.PG_ITEM_ID=insp.PG_ITEM_ID 
             LEFT JOIN CONNECTION_MACHINE_INFO cmi ON INSP.SYS_MACHINE_NAME = cmi.SYS_MACHINE_NAME
             WHERE cmi.USR_MACHINE_NAME='{nameLine}' ORDER BY INSP_BEGIN_DATE DESC) WHERE ROWNUM=1)");

            CountDefVer = Connect.GetInt($@"SELECT COUNT(*) FROM PRISM.INSP_RESULT_SUMMARY_INFO result 
            LEFT JOIN SEG_RESULT_INFO sri ON result.INSP_ID = sri.INSP_ID AND result.SYS_MACHINE_NAME = sri.SYS_MACHINE_NAME
             LEFT JOIN PG_INFO pi ON result.PG_ITEM_ID = pi.PG_ITEM_ID   
             LEFT JOIN CONNECTION_MACHINE_INFO cmi ON result.SYS_MACHINE_NAME = cmi.SYS_MACHINE_NAME
             WHERE INSP_END_DATE BETWEEN TO_DATE(CONCAT('{d.DateStart}','{d.TimeStart}'),'DD.MM.YY HH24:MI:SS') AND TO_DATE(CONCAT('{d.DateEnd}','{d.TimeEnd}'),'DD.MM.YY HH24:MI:SS') 
             AND cmi.USR_MACHINE_NAME='{nameLine}'
             AND sri.INSP_RESULT_CODE<>0 
             AND pi.PG_NAME=(SELECT PG_NAME FROM (SELECT pi.PG_NAME FROM PRISM.INSP_RESULT_SUMMARY_INFO insp 
             LEFT JOIN PRISM.PG_INFO pi ON pi.PG_ITEM_ID=insp.PG_ITEM_ID 
              LEFT JOIN CONNECTION_MACHINE_INFO cmi ON INSP.SYS_MACHINE_NAME = cmi.SYS_MACHINE_NAME
             WHERE cmi.USR_MACHINE_NAME='{nameLine}' ORDER BY INSP_BEGIN_DATE DESC) WHERE ROWNUM=1) 
             AND sri.VC_LAST_RESULT_CODE<>0  AND sri.VC_LAST_RESULT_CODE IS NOT NULL");

            CountRealease = Connect.GetInt($@"SELECT COUNT(*) FROM PRISM.INSP_RESULT_SUMMARY_INFO result 
            LEFT JOIN SEG_RESULT_INFO sri ON result.INSP_ID = sri.INSP_ID AND result.SYS_MACHINE_NAME = sri.SYS_MACHINE_NAME 
            LEFT JOIN PG_INFO pi ON RESULT.PG_ITEM_ID = pi.PG_ITEM_ID 
             LEFT JOIN CONNECTION_MACHINE_INFO cmi ON result.SYS_MACHINE_NAME = cmi.SYS_MACHINE_NAME
             WHERE result.INSP_END_DATE BETWEEN TO_DATE(CONCAT('{d.DateStart}','{d.TimeStart}'),'DD.MM.YY HH24:MI:SS') AND TO_DATE(CONCAT('{d.DateEnd}','{d.TimeEnd}'),'DD.MM.YY HH24:MI:SS') 
             AND cmi.USR_MACHINE_NAME='{nameLine}' AND pi.PG_NAME=(SELECT PG_NAME 
             FROM (SELECT pi.PG_NAME FROM PRISM.INSP_RESULT_SUMMARY_INFO insp 
             LEFT JOIN PRISM.PG_INFO pi ON pi.PG_ITEM_ID=insp.PG_ITEM_ID 
             LEFT JOIN CONNECTION_MACHINE_INFO cmi ON INSP.SYS_MACHINE_NAME = cmi.SYS_MACHINE_NAME
             WHERE cmi.USR_MACHINE_NAME='{nameLine}' ORDER BY INSP_BEGIN_DATE DESC) WHERE ROWNUM=1)");

            CountNotVer = Connect.GetInt($@"SELECT count(*) FROM PRISM.INSP_RESULT_SUMMARY_INFO result 
            LEFT JOIN SEG_RESULT_INFO sri ON sri.INSP_ID=result.INSP_ID AND result.SYS_MACHINE_NAME = sri.SYS_MACHINE_NAME 
            LEFT JOIN CONNECTION_MACHINE_INFO cmi ON result.SYS_MACHINE_NAME = cmi.SYS_MACHINE_NAME
            WHERE sri.INSP_RESULT_CODE<>0 AND sri.VC_LAST_RESULT_CODE IS NULL AND  cmi.USR_MACHINE_NAME='{nameLine}'
            AND result.INSP_BEGIN_DATE BETWEEN TO_DATE(CONCAT('{d.DateStart}','{d.TimeStart}'),'DD.MM.YY HH24:MI:SS') AND TO_DATE(CONCAT('{d.DateEnd}','{d.TimeEnd}'),'DD.MM.YY HH24:MI:SS') 
            ORDER BY INSP_BEGIN_DATE DESC");

            #region

            //CountDefAOI = Connect.GetInt($@"SELECT COUNT(*) FROM PRISM.INSP_RESULT_SUMMARY_INFO result 
            // LEFT JOIN SEG_RESULT_INFO sri ON result.INSP_ID = sri.INSP_ID AND result.SYS_MACHINE_NAME = sri.SYS_MACHINE_NAME 
            // LEFT JOIN PG_INFO pi ON result.PG_ITEM_ID = pi.PG_ITEM_ID  
            // LEFT JOIN CONNECTION_MACHINE_INFO cmi ON result.SYS_MACHINE_NAME = cmi.SYS_MACHINE_NAME
            // WHERE INSP_END_DATE BETWEEN TO_DATE('{Time.DateStart + " " + Time.TimeStart}','DD.MM.YY HH24:MI:SS') 
            // AND SYSDATE AND cmi.USR_MACHINE_NAME='{nameLine}'
            // AND sri.INSP_RESULT_CODE<>0 AND pi.PG_NAME=(SELECT PG_NAME FROM (SELECT pi.PG_NAME 
            // FROM PRISM.INSP_RESULT_SUMMARY_INFO insp LEFT JOIN PRISM.PG_INFO pi ON pi.PG_ITEM_ID=insp.PG_ITEM_ID 
            // LEFT JOIN CONNECTION_MACHINE_INFO cmi ON INSP.SYS_MACHINE_NAME = cmi.SYS_MACHINE_NAME
            // WHERE cmi.USR_MACHINE_NAME='{nameLine}' ORDER BY INSP_BEGIN_DATE DESC) WHERE ROWNUM=1)");

            //CountDefVer = Connect.GetInt($@"SELECT COUNT(*) FROM PRISM.INSP_RESULT_SUMMARY_INFO result 
            //LEFT JOIN SEG_RESULT_INFO sri ON result.INSP_ID = sri.INSP_ID AND result.SYS_MACHINE_NAME = sri.SYS_MACHINE_NAME
            // LEFT JOIN PG_INFO pi ON result.PG_ITEM_ID = pi.PG_ITEM_ID   
            // LEFT JOIN CONNECTION_MACHINE_INFO cmi ON result.SYS_MACHINE_NAME = cmi.SYS_MACHINE_NAME
            // WHERE INSP_END_DATE BETWEEN TO_DATE('{Time.DateStart + " " + Time.TimeStart}','DD.MM.YY HH24:MI:SS') AND SYSDATE AND cmi.USR_MACHINE_NAME='{nameLine}'
            // AND sri.INSP_RESULT_CODE<>0 
            // AND pi.PG_NAME=(SELECT PG_NAME FROM (SELECT pi.PG_NAME FROM PRISM.INSP_RESULT_SUMMARY_INFO insp 
            // LEFT JOIN PRISM.PG_INFO pi ON pi.PG_ITEM_ID=insp.PG_ITEM_ID 
            //  LEFT JOIN CONNECTION_MACHINE_INFO cmi ON INSP.SYS_MACHINE_NAME = cmi.SYS_MACHINE_NAME
            // WHERE cmi.USR_MACHINE_NAME='{nameLine}' ORDER BY INSP_BEGIN_DATE DESC) WHERE ROWNUM=1) 
            // AND sri.VC_LAST_RESULT_CODE<>0  AND sri.VC_LAST_RESULT_CODE IS NOT NULL");

            //CountRealease = Connect.GetInt($@"SELECT COUNT(*) FROM PRISM.INSP_RESULT_SUMMARY_INFO result 
            //LEFT JOIN SEG_RESULT_INFO sri ON result.INSP_ID = sri.INSP_ID AND result.SYS_MACHINE_NAME = sri.SYS_MACHINE_NAME 
            //LEFT JOIN PG_INFO pi ON RESULT.PG_ITEM_ID = pi.PG_ITEM_ID 
            // LEFT JOIN CONNECTION_MACHINE_INFO cmi ON result.SYS_MACHINE_NAME = cmi.SYS_MACHINE_NAME
            // WHERE result.INSP_END_DATE BETWEEN TO_DATE('{Time.DateStart + " " + Time.TimeStart}','DD.MM.YY HH24:MI:SS') AND SYSDATE 
            // AND cmi.USR_MACHINE_NAME='{nameLine}' AND pi.PG_NAME=(SELECT PG_NAME 
            // FROM (SELECT pi.PG_NAME FROM PRISM.INSP_RESULT_SUMMARY_INFO insp 
            // LEFT JOIN PRISM.PG_INFO pi ON pi.PG_ITEM_ID=insp.PG_ITEM_ID 
            // LEFT JOIN CONNECTION_MACHINE_INFO cmi ON INSP.SYS_MACHINE_NAME = cmi.SYS_MACHINE_NAME
            // WHERE cmi.USR_MACHINE_NAME='{nameLine}' ORDER BY INSP_BEGIN_DATE DESC) WHERE ROWNUM=1)");

            //CountNotVer = Connect.GetInt($@"SELECT count(*) FROM PRISM.INSP_RESULT_SUMMARY_INFO result 
            //LEFT JOIN SEG_RESULT_INFO sri ON sri.INSP_ID=result.INSP_ID AND result.SYS_MACHINE_NAME = sri.SYS_MACHINE_NAME 
            //LEFT JOIN CONNECTION_MACHINE_INFO cmi ON result.SYS_MACHINE_NAME = cmi.SYS_MACHINE_NAME
            //WHERE sri.INSP_RESULT_CODE<>0 AND sri.VC_LAST_RESULT_CODE IS NULL AND  cmi.USR_MACHINE_NAME='{nameLine}'
            //AND result.INSP_BEGIN_DATE BETWEEN TO_DATE('{Time.DateStart + " " + Time.TimeStart}','DD.MM.YY HH24:MI:SS') " +
            //"AND SYSDATE ORDER BY INSP_BEGIN_DATE DESC");
            #endregion

        }


    }
}