using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("Cluster")]
    public class Cluster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ClusterID { get; set; }
        [Required]
        [Loggable]
        public string ClusterName { get; set; }
        [Loggable]
        public string ClusterCode { get; set; }                        
    }
}
