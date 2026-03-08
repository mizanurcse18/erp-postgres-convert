using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("Unit"), Serializable]
    public class Unit : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UnitID { get; set; }
        [Loggable]
        [Required]
        public string UnitName { get; set; }
        [Loggable]        
        public string UnitDescription { get; set; }        
    }
}
