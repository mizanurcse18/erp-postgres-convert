using Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class ShiftDto
    {
        public int ShiftingMasterID { get; set; }
        public string ShiftingName { get; set; }
        public int? FirstDayOfWeekId { get; set; }
        public string FirstDayOfWeekName => ((DayOfWeek)FirstDayOfWeekId.Value).ToString();
        public int ShiftingSlotId { get; set; }
        public int BufferTimeInMinute { get; set; }
        public DateTime EffectFrom { get; set; }
        public List<int> AssignedDepartments { get; set; }
        public List<ComboModel> AssignedDepartmentModel { get; set; }
        public List<Slots> Slots { get; set; }
        public List<LeaveSlots> LeaveSlots { get; set; }
        public string AssignedDepartmentString { get; set; }
        public string EfectedFromType { get; set; }
    }

    public class Slots
    {
        public int dayId { get; set; }
        public string dayName { get; set; }
        public List<shifts> shifts { get; set; }
    }
    public class shifts
    {
        public int ShiftingChildID { get; set; }
        public bool IsWorkingDay { get; set; }
        public DateTime? FromTime { get; set; }
        public TimeSpan FromTimeLocal { get { return FromTime.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(FromTime.Value, TimeZoneInfo.FindSystemTimeZoneById("Bangladesh Standard Time")).TimeOfDay : DateTime.Now.TimeOfDay; } }
        public DateTime? ToTime { get; set; }
        public TimeSpan ToTimeLocal { get { return FromTime.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(ToTime.Value, TimeZoneInfo.FindSystemTimeZoneById("Bangladesh Standard Time")).TimeOfDay : DateTime.Now.TimeOfDay; } }
    }

    //Leave
    public class LeaveSlots
    {
        public int shiftingLeaveCategoryID { get; set; }
        public string leaveCategoryName { get; set; }
        public List<leaveShifts> leaveShifts { get; set; }
    }
    public class leaveShifts
    {
        public int ShiftingLeaveChildID { get; set; }
        public DateTime? FromTime { get; set; }
        public TimeSpan FromTimeLocal { get { return FromTime.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(FromTime.Value, TimeZoneInfo.FindSystemTimeZoneById("Bangladesh Standard Time")).TimeOfDay : DateTime.Now.TimeOfDay; } }
        public DateTime? ToTime { get; set; }
        public TimeSpan ToTimeLocal { get { return FromTime.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(ToTime.Value, TimeZoneInfo.FindSystemTimeZoneById("Bangladesh Standard Time")).TimeOfDay : DateTime.Now.TimeOfDay; } }
    }
    //Leave

    public class ShiftListDto
    {
        public int ShiftingMasterID { get; set; }
        public string ShiftingName { get; set; }
        public int? FirstDayOfWeekId { get; set; }
        public string FirstDayOfWeekName => ((DayOfWeek)FirstDayOfWeekId.Value).ToString();
        public int ShiftingSlotId { get; set; }
        public int BufferTimeInMinute { get; set; }
        public DateTime EffectFrom { get; set; }
        public string EffectFromString { get { return EffectFrom.ToShortDateString(); } }
        public bool IsRemovable { get; set; }
        public List<ShiftDetails> ShiftDetails { get; set; }

    }

    public class ShiftDetails
    {
        public int DayId { get; set; }
        public string DayName { get; set; }
        public int ShiftingChildID { get; set; }
        public bool IsWorkingDay { get; set; }
        public DateTime? FromTime { get; set; }
        public string FromTimString { get { return FromTime.HasValue ? FromTime.Value.ToString("hh:mm tt") : ""; } }
        public DateTime? ToTime { get; set; }
        public string ToTimString { get { return ToTime.HasValue ? ToTime.Value.ToString("hh:mm tt") : ""; } }
    }
}
