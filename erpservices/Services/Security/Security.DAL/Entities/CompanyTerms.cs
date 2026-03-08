using Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;
using DAL.Core.JsonConverter;
using System;
using DAL.Core.EntityBase;

namespace Security.DAL.Entities
{
    [Table("company_terms")]
    public class CompanyTerms : Auditable
	{
		[Key]
		[Column("id")]
		public string CTID { get; set; }
		[Required]
		public string Terms { get; set; }
		[Required]
		public string TermsType { get; set; }

	}
}
