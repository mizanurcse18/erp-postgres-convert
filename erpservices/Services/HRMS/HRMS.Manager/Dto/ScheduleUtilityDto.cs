using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class ScheduleUtilityDto
    {
        public DateTime? FromDate { set; get; }
        public DateTime? ToDate { set; get; }
        public int ScheduleNo { set; get; }
    }
}
