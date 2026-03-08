using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("PurchaseRequisitionMaster"), Serializable]
    public class PurchaseRequisitionMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PRMasterID { get; set; }
        [Loggable]
        public DateTime? PRDate { get; set; }
        [Loggable]
        public string ReferenceNo { get; set; }
        [Loggable]
        public string Subject { get; set; }
        [Loggable]
        public string Preamble { get; set; }
        [Loggable]
        public string PriceAndCommercial { get; set; }
        [Loggable]
        public string Solicitation { get; set; }
        [Loggable]
        public string BudgetPlanRemarks { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal GrandTotal { get; set; }
        [Loggable]
        public int ApprovalStatusID { get; set; }
        [Loggable]
        public string Description { get; set; }
        [Loggable]
        public string ReferenceKeyword { get; set; }
        [Loggable]
        public int? DeliveryLocation { set; get; }
        [Loggable]
        public string SCMRemarks { get; set; }
        [Loggable]
        public bool IsSingleQuotation { get; set; }
        [Loggable]
        public DateTime? RequiredByDate { get; set; }
        [Loggable]
        public bool IsDraft { get; set; }
        [Loggable]
        public long MRMasterID { get; set; }
        [Loggable]
        public bool IsArchive { get; set; }
        [Loggable]
        public int BudgetPlanCategoryID { get; set; }
    }
}
