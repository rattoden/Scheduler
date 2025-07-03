namespace SchedulerV4.Models
{
    public class SotrudnikEntity
    {

        public int ID_SOTR { get; set; }
        public string FIRSTNAME { get; set; }

        public string MIDDLENAME { get; set; }
        public string LASTNAME { get; set; }
        public string TAB_NO { get; set; }
        public char SEX { get; set; }

        public DateTime BIRTHDAY { get; set; }

        public string VUZ { get; set; }

        public string RANG { get; set; }
        public string DEGREE { get; set; }

        public string SCIENCE { get; set; }
        public char DEKAN { get; set; }
        public int CAF_ID    { get; set; }

    }
}
