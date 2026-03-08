
using System;
using Security.DAL.Entities;
using Manager.Core.Mapper;
using DAL.Core.EntityBase;
using System.Collections.Generic;

namespace Security.Manager
{
	[AutoMap(typeof(SecurityRuleMaster)), Serializable]
	public class SecurityRuleMasterDto : Auditable
	{
		public int SecurityRuleID { get; set; }
		public string SecurityRuleName { get; set; }
		public string SecurityRuleDescription { get; set; }
		public short? ApplicationID { get; set; }
		public List<SecurityRulePermissionChildDto> ChildModels { set; get; }

	}
}