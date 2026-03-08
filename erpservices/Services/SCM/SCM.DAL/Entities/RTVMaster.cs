using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("RTVMaster"), Serializable]
    public class RTVMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long RTVMID { get; set; }        
        [Loggable]
        [Required]
        public string ReferenceNo { get; set; }
        [Loggable]
        public string ReferenceKeyword { get; set; }
        [Loggable]
        [Required]
        public DateTime SupplyDate { get; set; }
        [Loggable]
        [Required]
        public DateTime ReturnDate { get; set; }
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
        public decimal TotalReturnQty { get; set; }               
        [Loggable]
        public int ApprovalStatusID { get; set; }                               
        [Loggable]
        public string BudgetPlanRemarks { get; set; }
        [Loggable]
        public string SupplierDCNo { get; set; }
        [Loggable]
        [Required]
        public DateTime SupplierDCDate { get; set; }
        [Loggable]
        public bool IsDraft { get; set; }
        [Loggable]
        public long QCMID { get; set; }
    }
}
