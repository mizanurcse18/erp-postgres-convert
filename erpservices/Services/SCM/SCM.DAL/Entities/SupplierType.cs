using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("SupplierType"), Serializable]
    public class SupplierType : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long STID { get; set; }
        [Required]
        [Loggable]
        public string TypeName { get; set; }
        [Loggable]
        public long? GLID { get; set; }
        [Loggable]
        public long? ManualGLID { get; set; }
        [Loggable]
        public bool IsActive { get; set; }
    }
}
