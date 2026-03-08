using System;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;
using DAL.Core.JsonConverter;

namespace DAL.Core.EntityBase
{
    [Serializable]
    public class Auditable : EntityBase
    {
        public string CompanyID { get; set; }
        public int CreatedBy { get; set; }
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime CreatedDate { get; set; }
        public string CreatedIP { get; set; }
        [JsonIgnore]
        public int? UpdatedBy { get; set; }
        [JsonIgnore]
        public DateTime? UpdatedDate { get; set; }
        [JsonIgnore]
        public string UpdatedIP { get; set; }
        [ConcurrencyCheck]
        public short RowVersion { get; set; }
    }
}
