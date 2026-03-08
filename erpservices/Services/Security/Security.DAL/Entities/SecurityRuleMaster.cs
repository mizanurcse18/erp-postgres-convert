using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DAL.Core.Attribute;
using DAL.Core.EntityBase;

namespace Security.DAL.Entities
{
	[Table("SecurityRuleMaster")]
	public class SecurityRuleMaster : Auditable
	{
		[Key]
		public int SecurityRuleID { get; set; }
		[Loggable]
		public string SecurityRuleName { get; set; }
		[Loggable]
		public string SecurityRuleDescription { get; set; }
		public short? ApplicationID { get; set; }

	}
}
