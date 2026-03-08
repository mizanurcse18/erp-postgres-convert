using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("BranchInfo")]
    public class BranchInfo : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BranchID { get; set; }
        [Required]
        [Loggable]
        public string BranchName { get; set; }
        [Loggable]
        public string BranchCode { get; set; }
        [Loggable]
        public string PermanentAddress { get; set; }
        [Loggable]
        public string CurrentAddress { get; set; }
        [Loggable]
        public string BranchEmail{ get; set; }
        [Loggable]
        public int? RegionID { get; set; }
        
    }
}
