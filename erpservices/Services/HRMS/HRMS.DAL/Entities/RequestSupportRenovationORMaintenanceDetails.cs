using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("RequestSupportRenovationORMaintenanceDetails")]
    public class RequestSupportRenovationORMaintenanceDetails : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RSRMDID { get; set; }
        [Required]
        [Loggable]
        public int RSMID { get; set; }
        [Loggable]
        public int RenoOrMainCategoryID { get; set; }
        [Loggable]
        public DateTime NeededByDate { get; set; }
        [Loggable]
        public string Remarks { get; set; }
    }
}
