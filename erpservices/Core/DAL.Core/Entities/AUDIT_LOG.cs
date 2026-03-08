using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DAL.Core.JsonConverter;
using Newtonsoft.Json;

namespace DAL.Core.Entities
{
    [Table("AUDIT_LOG"), Serializable]
    public class AUDIT_LOG : EntityBase.EntityBase
    {
        [Key]
        public long AUDIT_ID { get; set; }
        [Required]
        [Column(TypeName = "varchar(50)")]
        public string TABLE_NAME { get; set; }
        public DateTime AUDIT_DATE { get; set; }
        [Required]
        [Column(TypeName = "varchar(50)")]
        public string KEY_VALUES { get; set; }
        public string OLD_VALUES { get; set; }
        public string NEW_VALUES { get; set; }
        [Required]
        [Column(TypeName = "varchar(20)")]
        public string ROW_STATE { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string COMPANY_ID { get; set; }
        public int CREATED_BY { get; set; }
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime CREATED_DATE { get; set; }
        [Required]
        [Column(TypeName = "varchar(20)")]
        public string CREATED_IP { get; set; }
    }
}