using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Security.DAL.Entities
{
    [Table("BankAccountInfo")]
    public class BankAccountInfo : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BankAccountID { get; set; }
        [Required]
        [Loggable]
        public int PersonID { get; set; }
        [Required]
        [Loggable]
        public int BankTypeID { get; set; }
        [Loggable]
        public int BankID { get; set; }
        [Loggable]
        public int BankBranchID { get; set; }
        [Loggable]
        public int? AccountType { get; set; }
        [Loggable]
        public string AccountNo { get; set; }
        [Loggable]
        public string AccountName { get; set; }        
        [Loggable]
        public bool IsPrimary { get; set; }        
    }
}
