using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HRMS.DAL.Entities
{
    [Table("Division")]
    public class Division : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DivisionID { get; set; }
        [Required]
        [Loggable]
        public string DivisionName { get; set; }
        [Loggable]
        public string DivisionCode { get; set; }
    }
}
