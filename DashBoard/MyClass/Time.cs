using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DashBoard.MyClass
{
    public class Time
    {
        public string DateStart { get; set; }
        public string TimeStart { get; set; }
        public string TimeEnd { get; set; }
        public string DateEnd { get; set; }

        public string Shift { get; }

        public Time()
        {
            var NowFull = DateTime.UtcNow.AddHours(2).AddDays(0);
            var NOW = NowFull.Hour;


            if (NOW >= 8 & NOW <= 20)
            {
                Shift = "Дневная смена";
                DateStart = NowFull.ToString("dd.MM.yyyy");
                TimeStart = "06:00:00";

                DateEnd = DateStart;
                TimeEnd = "18:00:00";
            }
            else
            {
                Shift = "Ночная смена";

                if (NOW >= 20 & NOW <= 23)
                {
                    DateStart = NowFull.ToString("dd.MM.yyyy");
                    TimeStart = "18:00:00";

                    DateEnd = NowFull.AddDays(1).ToString("dd.MM.yyyy");
                    TimeEnd = "06:00:00";
                }
                else
                {
                    DateStart = NowFull.AddDays(-1).ToString("dd.MM.yyyy");
                    TimeStart = "18:00:00";

                    DateEnd = NowFull.ToString("dd.MM.yyyy");
                    TimeEnd = "06:00:00";
                }


              
            }
        }

    }
}