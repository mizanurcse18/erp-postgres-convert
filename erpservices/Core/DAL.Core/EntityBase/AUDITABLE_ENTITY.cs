using System;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;
using DAL.Core.JsonConverter;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Core.EntityBase
{
    [Serializable]
    public class AUDITABLE_ENTITY : EntityBase
    {
        [Column(TypeName = "varchar(50)")]
        public string COMPANY_ID { get; set; }
        public int CREATED_BY { get; set; }
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime CREATED_DATE { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string CREATED_IP { get; set; }
        [JsonIgnore]
        public int? UPDATED_BY { get; set; }
        [JsonIgnore]
        public DateTime? UPDATED_DATE { get; set; }
        [JsonIgnore]
        [Column(TypeName = "varchar(20)")]
        public string UPDATED_IP { get; set; }
        [ConcurrencyCheck]
        public short ROW_VERSION { get; set; }
    }
}
