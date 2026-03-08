using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("SupportRequisitionMaster")]
    public class SupportRequisitionMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SRMID { get; set; }
        [Loggable]
        public string ReferenceNo { get; set; }
        [Loggable]
        public string ReferenceKeyword { get; set; }
        [Loggable]
        public DateTime? RequestDate { get; set; }
        [Loggable]
        [Required]
        public int SupportCategoryID { get; set; }
        [Loggable]
        public int? EmployeeID { get; set; }
        [Loggable]
        public bool? IsOnBehalf { get; set; }
        [Loggable]
        public string BusinessJustification { get; set; }
        [Loggable]
        public string ITRemomandation { get; set; }
        [Loggable]
        [Required]
        public int ApprovalStatusID { get; set; }
        [Loggable]
        [Required]
        public bool IsDraft { get; set; }
        [Loggable]
        public string ITRemarks { get; set; }
        [Loggable]
        public bool? IsNewRequirements { get; set; }
        [Loggable]
        public bool? IsReplacement { get; set; }
        [Loggable]
        public bool? IsSettle { get; set; }
        [Loggable]
        public DateTime? SettlementDate { get; set; }
        [Loggable]
        public string SettlementRemarks { get; set; } 

    }
}
