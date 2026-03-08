using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("VendorAssessmentMembers"), Serializable]
    public class VendorAssessmentMembers : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PrimaryID { get; set; }
        [Loggable]
        [Required]
        public long EmployeeID { get; set; }
    }
}
