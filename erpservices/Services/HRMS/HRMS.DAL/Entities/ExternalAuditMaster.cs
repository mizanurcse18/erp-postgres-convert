using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("ExternalAuditMaster")]
    public class ExternalAuditMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EAMID { get; set; }
        [Loggable]
        public string ReferenceNo { get; set; }
        [Loggable]
        public string ReferenceKeyword { get; set; }
        [Required]
        [Loggable]
        public int MercentOrUdoktaID { get; set; }
        [Required]
        [Loggable]
        public string MercentOrUdoktaNumber { get; set; }
        [Required]
        [Loggable]
        public DateTime AuditDate { get; set; }
        [Required]
        [Loggable]
        public string Remarks { get; set; }
        [Required]
        [Loggable]
        public int AuditableEmployeeID { get; set; }
        [Required]
        [Loggable]
        public int ApprovalStatusID { get; set; }
        [Loggable]
        public bool IsDraft { get; set; }
        [Loggable]
        public string ReturnDepartmentIDs { get; set; }
        [Loggable]
        public string Requirements { get; set; }
        [Loggable]
        public string Longtitude { get; set; }
        [Loggable]
        public string Latitude { get; set; }

        [Loggable]
        public string CapturedImage { get; set; }

    }
}
