using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Security.DAL.Entities
{
	[Table("SecurityGroupRuleChild")]
	public class SecurityGroupRuleChild : Auditable
	{
		[Key]
		public int SecurityGroupRuleChildID { get; set; }
		[Loggable]
		public int SecurityGroupID { get; set; }
		[Loggable]
		public int SecurityRuleID { get; set; }

	}
}
