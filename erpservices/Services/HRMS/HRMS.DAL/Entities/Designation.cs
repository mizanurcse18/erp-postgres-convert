using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HRMS.DAL.Entities
{
    [Table("Designation")]
    public class Designation : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DesignationID { get; set; }
        [Required]
        [Loggable]
        public string DesignationName { get; set; }
        [Loggable]
        public string DesignationCode { get; set; }
    }
}
