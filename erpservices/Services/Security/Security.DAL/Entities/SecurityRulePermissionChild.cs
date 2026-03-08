using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Security.DAL.Entities
{
    [Table("SecurityRulePermissionChild")]
    public class SecurityRulePermissionChild : Auditable
    {
        [Key]
        public int SecurityRulePermissionID { get; set; }
        public int SecurityRuleID { get; set; }
        //[Required]
        //public string ObjectType { get; set; }
        [Loggable]
        public int MenuID { get; set; }
        [Loggable]
        public bool? CanRead { get; set; }
        [Loggable]
        public bool? CanCreate { get; set; }
        [Loggable]
        public bool? CanUpdate { get; set; }
        [Loggable]
        public bool? CanDelete { get; set; }
        [Loggable]
        public bool? CanReport { get; set; }

    }
}
