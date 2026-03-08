using Manager.Core.Mapper;
using DAL.Core.EntityBase;
using HRMS.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    [AutoMap(typeof(Holiday)), Serializable]
    public class HolidayDto : Auditable
    {
        public int HolidayID { get; set; }
        public int FinancialYearID { get; set; }
        public string Name { get; set; }
        public DateTime? HolidayDate { get; set; }
        public string HolidayDateString { get { return HolidayDate.HasValue ? HolidayDate.Value.ToShortDateString() : ""; } }
        public string Remarks { get; set; }
        public string ImagePath { get; set; }
        public bool IsFestivalHoliday { get; set; }
    }

    public class HolidayListDto
    {
        public int HolidayID { get; set; }
        public int FinancialYearID { get; set; }
        public string Name { get; set; }
        public DateTime HolidayDate { get; set; }
        public string Remarks { get; set; }
        public string ImagePath { get; set; }
        public int Year { get; set; }
        public bool IsFestivalHoliday { get; set; }
        public int NumberOfHoliday { get; set; }
        public List<HolidayDto> HolidayDetails { get; set; }
    }
}
