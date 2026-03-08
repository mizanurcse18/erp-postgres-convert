using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("NFAMaster"), Serializable]
    public class NFAMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NFAID { get; set; }
        [Loggable]
        public DateTime? NFADate { get; set; }
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
        public decimal GrandTotal { get; set; }
        [Loggable]
        public int ApprovalStatusID { get; set; }
        [Loggable]
        public int TemplateID { get; set; }
        [Loggable]
        public string Description { get; set; }
        [Loggable]
        public string DescriptionImageURL { get; set; }
        [Loggable]
        public string ReferenceKeyword { get; set; }
        [Loggable]
        public bool IsDraft{ get; set; }
        [Loggable]
        public int BudgetPlanCategoryID { get; set; }
    }
}
