using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("QCMaster"), Serializable]
    public class QCMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long QCMID { get; set; }        
        [Loggable]
        [Required]
        public string ReferenceNo { get; set; }
        [Loggable]
        public string ReferenceKeyword { get; set; }
        [Loggable]
        [Required]
        public DateTime ReceiptDate { get; set; }
        [Loggable]
        [Required]
        public long WarehouseID { set; get; }
        [Loggable]
        [Required]
        public long SupplierID { get; set; }
        [Loggable]
        public long PRMasterID { get; set; }
        [Loggable]
        public long POMasterID { get; set; }
        [Loggable]
        public decimal TotalSuppliedQty { get; set; }
        [Loggable]
        public decimal TotalAcceptedQty { get; set; }        
        [Loggable]
        public decimal TotalRejectedQty { get; set; }
        [Loggable]
        public int ApprovalStatusID { get; set; }                               
        [Loggable]
        public string BudgetPlanRemarks { get; set; }
        [Loggable]
        public string ChalanNo { get; set; }
        [Loggable]
        [Required]
        public DateTime ChalanDate { get; set; }        
        [Loggable]
        public bool IsDraft { get; set; }
    }
}
