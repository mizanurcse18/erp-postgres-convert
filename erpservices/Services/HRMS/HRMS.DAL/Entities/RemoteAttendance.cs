using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HRMS.DAL.Entities
{
    [Table("RemoteAttendance"), Serializable]
    public class RemoteAttendance : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RAID { get; set; }
        [Loggable]
        public DateTime AttendanceDate { get; set; }
        [Loggable]
        public int EmployeeID { get; set; }
        [Loggable]
        public string EmployeeNote { get; set; }
        [Loggable]
        public string EntryType { get; set; }
        [Loggable]
        public int StatusID { get; set; }
        [Loggable]
        public int ApproverID { get; set; }
        [Loggable]
        public string ApproverNote { get; set; }
        [Loggable]
        public DateTime? ApprovalDate { get; set; }
        [Loggable]
        public string Channel { get; set; }
        [Loggable]
        public int DistrictID { get; set; }
        [Loggable]
        public int DivisionID { get; set; }
        [Loggable]
        public int ThanaID { get; set; }
        [Loggable]
        public string Area { get; set; }
        [Loggable]
        public string Longitude { get; set; }
        [Loggable]
        public string Latitude { get; set; }
    }
}
