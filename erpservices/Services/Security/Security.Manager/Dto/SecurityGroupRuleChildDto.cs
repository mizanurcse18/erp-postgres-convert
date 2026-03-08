
using System;
using Security.DAL.Entities;
using Manager.Core.Mapper;
using DAL.Core.EntityBase;

namespace Security.Manager
{
	[AutoMap(typeof(SecurityGroupRuleChild)), Serializable]
	public class SecurityGroupRuleChildDto : Auditable
	{
		public int SecurityGroupRuleChildID { get; set; }
		public int SecurityGroupID { get; set; }
		public int SecurityRuleID { get; set; }
		public string SecurityRuleName { get; set; }
	}
}