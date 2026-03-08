using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HRMS.DAL.Entities
{
    [Table("WalletConfiguration"), Serializable]
    public class WalletConfiguration : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int WalletConfigureID { get; set; }
        [Required]
        [Loggable]
        public decimal CashOutRate { get; set; }
        [Required]
        [Loggable]
        public int DesignationID { get; set; }
        [Required]
        [Loggable]
        public decimal Percentage { get; set; }
        [Required]
        [Loggable]
        public int TypeID { get; set; }
        [Required]
        [Loggable]
        public bool ExceptionFlag { get; set; }
        public bool IsCurrent { get; set; }

    }
}
