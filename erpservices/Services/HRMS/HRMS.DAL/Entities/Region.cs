using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("Region")]
    public class Region : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RegionID { get; set; }
        [Required]
        [Loggable]
        public string RegionName { get; set; }
        [Loggable]
        public string RegionCode { get; set; }   
        [Loggable]
        public int? ClusterID { get; set; }
    }
}
