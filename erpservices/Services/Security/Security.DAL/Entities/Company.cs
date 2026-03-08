using Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;
using DAL.Core.JsonConverter;
using System;
using DAL.Core.EntityBase;

namespace Security.DAL.Entities
{
    [Table("Company")]
    public class Company: EntityBase
	{
		[Key]
		public string CompanyID { get; set; }
		[Required]
		public string CompanyName { get; set; }
		public string CompanyReportHead { get; set; }
		public string CompanyShortCode { get; set; }
		public string CompanyAddress { get; set; }
		public string TIN { get; set; }
		public string BIN { get; set; }
		public bool IsDefault { get; set; }
		public string VATRegNo { get; set; }
		public string IRCNo { get; set; }
		public byte[] CompanyLogo { get; set; }
		public string DefaultCompanyInCharge { get; set; }
		public string LicenseCode { get; set; }
		public string CompanyGroupID { get; set; }
		public int SeqNo { get; set; }
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
