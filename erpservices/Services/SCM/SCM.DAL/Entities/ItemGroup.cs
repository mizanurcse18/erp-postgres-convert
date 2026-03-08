using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("ItemGroup"), Serializable]
    public class ItemGroup : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ItemGroupID { get; set; }
        [Loggable]
        [Required]
        public string ItemGroupName { get; set; }
        [Loggable]        
        public string ItemGroupDescription { get; set; }
        [Loggable]        
        public string ItemGroupCode { get; set; }
        [Loggable]
        public string ExternalID { set; get; }
    }
}
