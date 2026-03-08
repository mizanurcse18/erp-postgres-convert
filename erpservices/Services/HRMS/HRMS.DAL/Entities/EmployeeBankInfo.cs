using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("EmployeeBankInfo")]
    public class EmployeeBankInfo : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BIID { get; set; }
        [Loggable]
        [Required]
        public int EmployeeID { get; set; }
        [Loggable]
        [Required]
        public string BankAccountName { get; set; }
        [Loggable]
        [Required]
        public string BankAccountNumber { get; set; }
        [Loggable]
        [Required]
        public string BankName { get; set; }
        [Loggable]
        [Required]
        public string BankBranchName { get; set; }
        [Loggable]
        [Required]
        public string RoutingNumber { get; set; }
    }
}
