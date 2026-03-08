using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("AccessoriesRequisitionCategoryChild")]
    public class AccessoriesRequisitionCategoryChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int AccessoriesRCCID { get; set; }
        [Required]
        [Loggable]
        public int SRMID { get; set; }
        [Required]
        [Loggable]
        public int ItemID { get; set; }
        [Required]
        [Loggable]
        public decimal Quantity { get; set; }
        [Loggable]
        public string Remarks { get; set; }
    }
}
