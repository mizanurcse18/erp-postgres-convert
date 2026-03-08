using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Approval.DAL.Entities;
using System;

namespace Approval.Manager
{
    [AutoMap(typeof(DOAMaster)), Serializable]
    public class DOAMasterDto : Auditable
    {
        public long DOAMasterID { get; set; }
        public long EmployeeID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StatusID { get; set; }
        public string Remarks { get; set; }
        public string DOAStatusName { get; set; }
        public string EmployeeName { get; set; }
    }
}
