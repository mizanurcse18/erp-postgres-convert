using DAL.Core.EntityBase;
using HRMS.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    [AutoMap(typeof(AttendanceSummary)), Serializable]
    public class AttendanceSummaryHRMSDto : Auditable
    {
        public long PrimaryID { get; set; }
        public int PersonID { get; set; }


        public string EmployeeCode { get; set; }
        
        public int EmployeeID { get; set; }

        public string InTimeString { get; set; }
        public string OutTimeString { get; set; }
        public DateTime AttendanceDate { get; set; }

        public DateTime? InTime { get; set; }

        public DateTime? InTimeLocal { get { return InTime.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(InTime.Value, TimeZoneInfo.FindSystemTimeZoneById("Bangladesh Standard Time")) : DateTime.Now; } }

        public DateTime? OutTime { get; set; }
        public DateTime? OutTimeLocal { get { return OutTime.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(OutTime.Value, TimeZoneInfo.FindSystemTimeZoneById("Bangladesh Standard Time")) : DateTime.Now; } }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public int TotalTimeInMin { get; set; } = 0;
        
        public int LateInMin { get; set; } = 0;
        
        public int OverTimeInMin { get; set; } = 0;
        
        
        public int AttendanceStatus { get; set; } = 0;
        
        public string CardNo { get; set; }
        
        
        public int ShiftID { get; set; }
        
        public int? LeaveCategoryID { get; set; }
        
        public bool IsEdited { get; set; } = false;
        
        public string Remarks { get; set; }
        
        public string HRNote { get; set; }
        
        public int? ApprovalStatusID { get; set; } = 23;
        
        public string DayStatus { get; set; }
        
        public int? ActualWorkingHourInMin { get; set; }
        public string AttendanceStatusName { get; set; }
        public string LeaveCategoryName { get; set; }
        public string ApprovalStatusName { get; set; }


        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int BufferTimeInMinute { get; set; }
    }
}
