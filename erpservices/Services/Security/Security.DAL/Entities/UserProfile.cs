using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DAL.Core.Attribute;
using DAL.Core.EntityBase;

namespace Security.DAL.Entities
{
    [Table("UserProfile")]
    public class UserProfile : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserID { get; set; }
        [Loggable]
        public string UserFullName { get; set; }   
        [Loggable]
        public int? DistributionHouseID { get; set; }
        [Loggable]
        public int? RegionID { get; set; }         
        [Loggable]
        public int? ClusterID { get; set; }
        [Loggable]
        public int? PositionID { get; set; }
        [Loggable]
        public string ContactNumber { get; set; }
        [Loggable]
        public string Email { get; set; }
        [Loggable]
        public DateTime? JoiningDate { get; set; }
        [Loggable]
        public string Longitude { get; set; }
        [Loggable]
        public string Latitude { get; set; }
        [Loggable]
        public int VisitTypeID { get; set; }
        [Loggable]
        public string ParentCode { get; set; }

    }
}