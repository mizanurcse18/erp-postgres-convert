using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("MaterialReceive"), Serializable]
    public class MaterialReceive : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long MRID { get; set; }
        [Loggable]
        [Required]
        public long QCMID { get; set; }
        [Loggable]
        [Required]
        public string ReferenceNo { get; set; }
        [Loggable]
        public string ReferenceKeyword { get; set; }
        [Loggable]
        [Required]
        public DateTime MRDate { get; set; }
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
        public decimal TotalReceivedQty { get; set; }
        [Loggable]
        public decimal TotalReceivedAmount { get; set; }
        [Loggable]
        public decimal TotalReceivedAvgRate { get; set; }
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
        [Loggable]
        public decimal TotalVatAmount { get; set; }
        [Loggable]
        public decimal TotalWithoutVatAmount { get; set; }
    }
}
