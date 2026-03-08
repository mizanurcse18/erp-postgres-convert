using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HRMS.DAL.Entities
{
    [Table("WageCodeConfiguration"), Serializable]
    public class WageCodeConfiguration : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int WageCodeConfigurationID { get; set; }
        [Required]
        [Loggable]
        public string WageCode { get; set; }
        [Required]
        [Loggable]
        public string Description { get; set; }
        [Required]
        [Loggable]
        public int TypeID { get; set; }
        [Required]
        [Loggable]
        public bool ExceptionFlag { get; set; }
        
    }
}
