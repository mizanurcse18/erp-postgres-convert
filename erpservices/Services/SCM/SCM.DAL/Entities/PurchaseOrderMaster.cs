using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("PurchaseOrderMaster"), Serializable]
    public class PurchaseOrderMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long POMasterID { get; set; }        
        [Loggable]
        [Required]
        public string ReferenceNo { get; set; }
        [Loggable]
        public string ReferenceKeyword { get; set; }
        [Loggable]
        [Required]
        public DateTime PODate { get; set; }
        [Loggable]
        public long? DeliveryLocation { set; get; }
        [Loggable]
        [Required]
        public long SupplierID { get; set; }
        [Loggable]
        public long PRMasterID { get; set; }                  
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal GrandTotal { get; set; }
        [Loggable]
        public int ApprovalStatusID { get; set; }                       
        [Loggable]
        [Required]
        public DateTime DeliveryWithinDate { get; set; }
        [Loggable]
        public string ContactPerson { get; set; }
        [Loggable]
        public string ContactNumber { get; set; }
        [Loggable]
        public string Remarks { get; set; }
        [Loggable]
        public string QuotationNo { get; set; }
        [Loggable]
        public DateTime? QuotationDate { get; set; }
        [Loggable]
        public int? PaymentTermsID { get; set; }
        [Loggable]
        public int? InventoryTypeID { get; set; }
        [Loggable]
        public bool IsDraft { get; set; }
        [Loggable]
        public string BudgetPlanRemarks { get; set; }
        [Loggable]
        public string SCMRemarks { get; set; }
        [Loggable]
        public bool IsClosed { get; set; }
        [Loggable]
        public string CloseRemarks { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalVatAmount { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalWithoutVatAmount { get; set; }
        [Loggable]
        public int? CreditDay { get; set; }
    }
}
