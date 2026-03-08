using DAL.Core.EntityBase;
using Manager.Core;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    [AutoMap(typeof(PersonWorkExperience)), Serializable]
    public class PersonWorkExperienceDto : Auditable
    {
        public int PWEID { get; set; }
        public int PersonID { get; set; }
        public string CompanyName { get; set; }
        public string CompanyBusiness { get; set; }
        public string Responsibilities { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public string CompanyLocation { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsLastEmployer { get; set; }
        public string LeavingReason { get; set; }
        public string TotalSpend { get { return ManagerBase.GetDateDifferenceYearMonthDay(StartDate, EndDate); } }
    }
}
