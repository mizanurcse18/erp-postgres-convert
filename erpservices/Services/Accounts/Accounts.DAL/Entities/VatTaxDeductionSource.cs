using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("VatTaxDeductionSource"), Serializable]
    public class VatTaxDeductionSource : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long VTDSID { get; set; }
        [Loggable]
        [Required]
        public int SourceTypeID { get; set; }      
        [Loggable]
        [Required]
        public string SectionOrServiceCode { get; set; }
        [Loggable]
        [Required]
        public string ServiceName { get; set; }
        [Loggable]
        [Required]
        public string RatePercent { get; set; }
        [Loggable]
        [Required]
        public long FinancialYearID { get; set; }
    }
}
