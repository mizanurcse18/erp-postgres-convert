using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Accounts.DAL.Entities
{
    [Table("ChequeBook"), Serializable]
    public class ChequeBook : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CBID { get; set; }
        [Loggable]
        [Required]
        public long GLID { get; set; }
        [Loggable]
        [Required]
        public long BankID { get; set; }
        [Loggable]
        [Required]
        public string ChequeBookNo { get; set; }
        [Loggable]
        [Required]
        public int NoOfPage { get; set; }
        [Loggable]
        [Required]
        public long AccountNo { get; set; }
        [Loggable]
        [Required]
        public string AccountName { get; set; }
        [Loggable]
        [Required]
        public string BranchName { get; set; }
        [Loggable]
        [Required]
        public string RoutingName { get; set; }
        [Loggable]
        [Required]
        public string SwiftCode { get; set; }
        [Loggable]
        public bool IsActive { get; set; }
        [Loggable]
        [Required]
        public int StartLeaf { get; set; }
        [Loggable]
        [Required]
        public int EndLeaf { get; set; }
    }
}
