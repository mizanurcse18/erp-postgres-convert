using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("NFAChild")]
    public class NFAChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NFACID { get; set; }       
        [Loggable]
        public string ItemName { get; set; }
        [Loggable]
        public string Description { get; set; }
        [Loggable]
        public decimal? Unit { get; set; }
        [Loggable]
        public string UnitType { get; set; }
        [Loggable]
        public Decimal? UnitPrice { get; set; }
        [Loggable]
        public string VatTaxStatus { get; set; }
        [Loggable]
        public string Vendor { get; set; }
        [Loggable]
        public Decimal TotalAmount { get; set; }
        [Loggable]
        public int NFAMasterID { get; set; }
        [Loggable]
        public string Type { get; set; }
        [Loggable]
        public string Duration { get; set; }
        [Loggable]
        public string CostType { get; set; }
        [Loggable]
        public decimal? EstimatedBudgetAmount { get; set; }
        [Loggable]
        public decimal? AITPercent { get; set; }
    }
}
