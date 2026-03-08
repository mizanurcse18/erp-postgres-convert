using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("RequestSupportFacilitiesDetails")]
    public class RequestSupportFacilitiesDetails : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RSFDID { get; set; }
        [Required]
        [Loggable]
        public int RSMID { get; set; }
        [Loggable]
        public int SupportCategoryID { get; set; }
        [Loggable]
        public DateTime NeededByDate { get; set; }
        [Loggable]
        public string Remarks { get; set; }
    }
}
