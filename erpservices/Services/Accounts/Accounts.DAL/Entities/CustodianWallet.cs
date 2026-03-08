using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Accounts.DAL.Entities
{
    [Table("CustodianWallet"), Serializable]
    public class CustodianWallet : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long CWID { get; set; }
        [Loggable]
        [Required]
        public string WalletName { get; set; }
        [Loggable]
        [Required]
        public long EmployeeID{ get; set; }
        [Loggable]
        [Required]
        public decimal ReimbursementThreshold { get; set; }
        [Loggable]
        [Required]
        public Decimal OpeningBalance { get; set; }
        [Loggable]
        [Required]
        public Decimal CurrentBalance { get; set; }
        [Loggable]
        [Required]
        public Decimal Limit { get; set; }
        [Required]
        [Loggable]
        public string DivisionIDs { get; set; }
        [Required]
        [Loggable]
        public string DepartmentIDs { get; set; }
        [Loggable]
        public bool IsActive { get; set; }
        
    }
}
