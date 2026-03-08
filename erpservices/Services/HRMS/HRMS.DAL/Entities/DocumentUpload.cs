using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("DocumentUpload")]
    public class DocumentUpload : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DUID { get; set; }
        [Required]
        [Loggable]
        public int EmployeeID { get; set; }
        [Required]
        [Loggable]
        public string TINNumber { get; set; }
        [Required]
        [Loggable]
        public int DocumentTypeID { get; set; }
        [Required]
        [Loggable]
        public int IncomeYear { get; set; }
        [Loggable]
        public int AssessmentYear { get; set; }
        [Required]
        [Loggable]
        public string RegSlNo { get; set; }
        [Loggable]
        public string TaxZone { get; set; }
        [Loggable]
        public string TaxCircle { get; set; } 
        [Loggable]
        public string TaxUnit { get; set; }
        [Required]
        [Loggable]
        public decimal PayableAmount { get; set; }
        [Required]
        [Loggable]
        public decimal PaidAmount { get; set; }
        [Required]
        [Loggable]
        public DateTime SubmissionDate { get; set; }
        [Required]
        [Loggable]
        public int ApprovalStatusID { get; set; }
        [Required]
        [Loggable]
        public bool IsDraft { get; set; }
        [Required]
        [Loggable]
        public bool IsUploaded { get; set; }
        [Loggable]
        public string ApiResponse { get; set; }
    }
}
