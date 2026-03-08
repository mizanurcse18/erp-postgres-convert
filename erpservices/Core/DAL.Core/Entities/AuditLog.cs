using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DAL.Core.JsonConverter;
using Newtonsoft.Json;

namespace DAL.Core.Entities
{
    [Table("AuditLog"), Serializable]
    public class AuditLog : EntityBase.EntityBase
    {
        [Key]
        public long AuditID { get; set; }
        [Required]
        public string TableName { get; set; }
        public DateTime AuditDate { get; set; }
        [Required]
        public string KeyValues { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        [Required]
        public string RowState { get; set; }
        public string CompanyID { get; set; }
        public int CreatedBy { get; set; }
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime CreatedDate { get; set; }
        [Required]
        public string CreatedIP { get; set; }
    }
}