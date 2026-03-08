using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("PurchaseRequisitionChildCostCenterBudget"), Serializable]
    public class PurchaseRequisitionChildCostCenterBudget : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PRCCCBID { get; set; }       
        [Loggable]
        [Required]
        public long PRMasterID { get; set; }        
        [Loggable]
        [Required]
        public int ForID { get; set; }   
        [Loggable]
        public DateTime? FromDate { get; set; }
        [Loggable]
        public DateTime? ToDate { get; set; }
        [Loggable]
        public Decimal? AllocatedBudgetAmount{ get; set; }        
        [Loggable]
        public Decimal? RemainingBudgetAmount { get; set; }       
        [Loggable]
        public string Note { get; set; }
    }
}
