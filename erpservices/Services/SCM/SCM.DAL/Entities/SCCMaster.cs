using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("SCCMaster"), Serializable]
    public class SCCMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long SCCMID { get; set; }        
        [Loggable]
        [Required]
        public string ReferenceNo { get; set; }
        [Loggable]
        public string ReferenceKeyword { get; set; }
        [Loggable]
        [Required]
        public long SupplierID { get; set; }
        [Loggable]
        public long PRMasterID { get; set; }
        [Loggable]
        public long POMasterID { get; set; }
        [Loggable]
        public string InvoiceNoFromVendor { get; set; }
        [Loggable]
        public DateTime InvoiceDateFromVendor { get; set; }        
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal InvoiceAmountFromVendor { get; set; }
        [Loggable]
        public DateTime? ServicePeriodFrom { get; set; }                               
        [Loggable]
        public DateTime? ServicePeriodTo { get; set; }
        [Loggable]
        public int PaymentType { get; set; }
        [Loggable]
        public string PaymentFixedOrPercent { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal PaymentFixedOrPercentAmount { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal PaymentFixedOrPercentTotalAmount { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalReceivedQty { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal SCCAmount { get; set; }
        [Loggable]
        public bool IsDraft { get; set; }

        [Loggable]
        public bool PerformanceAssessment1 { get; set; }
        [Loggable]
        public bool PerformanceAssessment2 { get; set; }
        [Loggable]
        public bool PerformanceAssessment3 { get; set; }
        [Loggable]
        public bool PerformanceAssessment4 { get; set; }
        [Loggable]
        public bool PerformanceAssessment5 { get; set; }
        [Loggable]
        public bool PerformanceAssessment6 { get; set; }
        [Loggable]
        public string PerformanceAssessmentComment { get; set; }
        [Loggable]
        public int Lifecycle { get; set; }
        [Loggable]
        public string LifecycleComment { get; set; }
        [Loggable]
        public int ApprovalStatusID { get; set; }
    }
}
