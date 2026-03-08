using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("IPAddress")]
    public class IPAddress : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IPAddressID { get; set; }
        [Required]
        [Loggable]
        public string IPNumber { get; set; }
        [Loggable]
        public string IPType { get; set; }
        
    }
}
