using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Security.DAL.Entities
{
    [Table("SystemVariable")]
    public class SystemVariable : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SystemVariableID { get; set; }
        public int EntityTypeID { get; set; }
        public string EntityTypeName { get; set; }
        [Required]
        public string SystemVariableCode { get; set; }
        public string SystemVariableDescription { get; set; }
        public int? NumericValue { get; set; }
        public int Sequence { get; set; }
        public bool IsSystemGenerated { get; set; }
        public bool IsInactive { get; set; }       

    }
}