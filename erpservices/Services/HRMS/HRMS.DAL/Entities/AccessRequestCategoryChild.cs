using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("AccessRequestCategoryChild")]
    public class AccessRequestCategoryChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int AccessRCCID { get; set; }
        [Required]
        [Loggable]
        public int SRMID { get; set; }
        [Required]
        [Loggable]
        public string AccessTypesIds { get; set; }
    }
}
