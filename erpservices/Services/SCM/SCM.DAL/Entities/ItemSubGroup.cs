using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("ItemSubGroup"), Serializable]
    public class ItemSubGroup : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ItemSubGroupID { get; set; }
        [Loggable]
        [Required]
        public string ItemSubGroupName { get; set; }
        [Loggable]        
        public string ItemSubGroupDescription { get; set; }
        [Loggable]        
        public string ItemSubGroupCode { get; set; }
        [Loggable]
        [Required]
        public long ItemGroupID { get; set; }
        [Loggable]       
        public long? GLID { get; set; }
        [Loggable]
        public string ExternalID { set; get; }
    }
}
