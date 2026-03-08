using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Accounts.DAL.Entities
{
    [Table("VoucherChild"), Serializable]
    public class VoucherChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long VoucherChildID { get; set; }

        [Loggable]
        [Required]
        public long VoucherMasterID { get; set; }

        [Loggable]
        [Required]
        public int TxnTypeID { get; set; }

        [Loggable]
        [Required]
        public long COAID { get; set; }

        [Loggable]
        [Required]
        public int CostCenterID { get; set; }

        [Loggable]
        [Required]
        public int BudgetHeadID { get; set; }

        [Loggable]
        public string Narration { get; set; }

        [Loggable]
        [Required]
        public int ModeOfPaymentID { get; set; }

        [Loggable]
        public long CBID { get; set; }

        [Loggable]
        public long CBCID { get; set; }

        [Loggable]
        public long LeafNo { get; set; }

        [Loggable]
        public decimal DebitAmount { get; set; }

        [Loggable]
        public decimal CreditAmount { get; set; }

        [Loggable]
        public bool IsActive { get; set; }
    }


}
