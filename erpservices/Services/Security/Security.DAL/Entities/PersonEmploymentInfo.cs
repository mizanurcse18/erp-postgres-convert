using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("PersonEmploymentInfo")]
    public class PersonEmploymentInfo : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PEIID { get; set; }
        [Required]
        [Loggable]
        public int PersonID { get; set; }
        [Required]
        [Loggable]
        public string CompanyName { get; set; }
        [Required]
        [Loggable]
        public string CompanyBusiness { get; set; }
        [Required]
        [Loggable]
        public string Designation { get; set; }
        [Loggable]
        public string Department { get; set; }
        [Loggable]
        public string Responsibilities { get; set; }
        [Loggable]
        public string CompanyLocation { get; set; }
        [Required]
        [Loggable]
        public DateTime FromDate { get; set; }
        [Required]
        [Loggable]
        public DateTime ToDate { get; set; }
        [Loggable]
        public bool IsCurrentlyWorking { get; set; }
        [Loggable]
        public string AreaOfExperiences { get; set; }
    }
}
