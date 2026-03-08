using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("PersonWorkExperience")]
    public class PersonWorkExperience : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PWEID { get; set; }
        [Required]
        [Loggable]
        public int PersonID { get; set; }
        [Required]
        [Loggable]
        public string CompanyName { get; set; }
        [Required]
        [Loggable]
        public string CompanyBusiness  { get; set; }
        [Required]
        [Loggable]
        public string Responsibilities { get; set; }
        [Required]
        [Loggable]
        public string Designation { get; set; }  
        [Loggable]
        public string Department { get; set; }
        [Loggable]
        public string CompanyLocation { get; set; }
        [Loggable]
        public DateTime StartDate { get; set; }
        [Loggable]
        public DateTime EndDate { get; set; }
        [Loggable]
        public bool IsLastEmployer { get; set; }
        [Loggable]
        public string LeavingReason{ get; set; }
    }
}
