using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("RequestSupportVehicleDetails")]
    public class RequestSupportVehicleDetails : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RSVDID { get; set; }
        [Required]
        [Loggable]
        public int RSMID { get; set; }
        [Loggable]
        public DateTime TransportNeededFrom { get; set; }
        [Loggable]
        public DateTime TransportNeededTo { get; set; }
        [Loggable]
        public int TransportTypeID { get; set; }
        [Loggable]
        public int PersonQuantity { get; set; }
        [Loggable]
        public DateTime StartTime { get; set; }
        [Loggable]
        public DateTime EndTime { get; set; }
        [Loggable]
        public string Duration { get; set; }
        [Loggable]
        public string Remarks { get; set; }
        [Loggable]
        public decimal TransportQuantity { get; set; }
        [Loggable]
        public int? FromDivisionID { get; set; }
        [Loggable]
        public int? FromDistrictID { get; set; }
        [Loggable]
        public int? FromThanaID { get; set; }
        [Loggable]
        public string FromArea { get; set; }
        [Loggable]
        public int? ToDivisionID { get; set; }
        [Loggable]
        public int? ToDistrictID { get; set; }
        [Loggable]
        public int? ToThanaID { get; set; }
        [Loggable]
        public string ToArea { get; set; }
        [Loggable]
        public bool? IsOthers { get; set; }
        [Loggable]
        public string Vehicle { get; set; }
        [Loggable]
        public string Driver { get; set; }
        [Loggable]
        public string ContactNumber { get; set; }
    }
}
