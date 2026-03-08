using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("RequestSupportMaster")]
    public class RequestSupportMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RSMID { get; set; }
       
        [Loggable]
        [Required]
        public int SupportTypeID { get; set; }
        [Loggable]
        public string LocationOrFloor { get; set; }
        [Loggable]
        public DateTime? NeededByDate { get; set; }
        [Loggable]
        public string RemarksOrCommentsOrPurpose { get; set; }
        [Loggable]
        [Required]
        public int ApprovalStatusID { get; set; }
        [Loggable]
        public string AdminRemarks { get; set; }
        [Loggable]
        [Required]
        public bool IsDraft { get; set; }

        [Loggable]
        public bool? IsSettle { get; set; }
        [Loggable]
        public DateTime? SettlementDate { get; set; }
        [Loggable]
        public string SettlementRemarks { get; set; }
        [Loggable]
        public string ReferenceNo { get; set; }
        [Loggable]
        public string ReferenceKeyword { get; set; }
        [Loggable]
        public int? EmployeeID { get; set; }
    }
}
