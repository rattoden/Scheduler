/*using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchedulerV4.Models
{
    public class ScheduleNPublEntity
    {
        public int LESSON_ID { get; set; }
        public int GROUPNO { get; set; }
        public int GROUPID { get; set; }
        public string DEN { get; set; }
        public string VREM { get; set; }
        public string DATA { get; set; }
        public int DISCIPL_NUM { get; set; }
        public string FORM_ZAN { get; set; }
        public string AUDITORIYA { get; set; }
        public string ZDANIE { get; set; }
        public string DOLZNOST { get; set; }
        public string PREPODAVATEL { get; set; }
        public string TABNUM { get; set; }
        public int YEARF { get; set; }
        public int SEMESTR { get; set; }
        public int TIP { get; set; }
        public int DEN_POS { get; set; }
        public int TIME_POS { get; set; }
        public int BUILDING_ID { get; set; }
        public int AUDITORY_ID { get; set; }
        public int NUM_DISCIPL_GUIDE { get; set; }
        public int PREPOD_ID { get; set; }

     

    }
}
*/

using System;
using System.ComponentModel.DataAnnotations;

namespace SchedulerV4.Models
{
    public class ScheduleNPublEntity
    {
        public int LESSON_ID { get; set; }

        public int GROUPNO { get; set; }

        public int GROUPID { get; set; }

        public string DEN { get; set; }

        public string VREM { get; set; }

        public string DATA { get; set; }

        public int DISCIPL_NUM { get; set; }
        public string FORM_ZAN { get; set; }
        public string AUDITORIYA { get; set; }
        public string ZDANIE { get; set; }
        public string DOLZNOST { get; set; }
        public string PREPODAVATEL { get; set; }
        public string TABNUM { get; set; }
        public int YEARF { get; set; }
        public int SEMESTR { get; set; }
        public int TIP { get; set; }
        public int DEN_POS { get; set; }
        public int TIME_POS { get; set; }
        public int BUILDING_ID { get; set; }
        public int AUDITORY_ID { get; set; }
        public int NUM_DISCIPL_GUIDE { get; set; }
        public int PREPOD_ID { get; set; }

        public DisciplineEntity Discipline { get; set; }
    }


}
