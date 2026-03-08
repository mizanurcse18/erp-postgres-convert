
using System;
using Security.DAL.Entities;
using Manager.Core.Mapper;
using DAL.Core.EntityBase;

namespace Security.Manager
{
	[AutoMap(typeof(SecurityRulePermissionChild)), Serializable]
	public class SecurityRulePermissionChildDto : Auditable
	{
		public int SecurityRulePermissionID { get; set; }
		public int SecurityRuleID { get; set; }
		//public string ObjectType { get; set; }
		public int MenuID { get; set; }		
		public string Description { get; set; }
		public bool? CanRead { get; set; }
		public bool? CanCreate { get; set; }
		public bool? CanUpdate { get; set; }
		public bool? CanDelete { get; set; }
		public bool? CanReport { get; set; }
		public int ParentID { get; set; }
		public string Icon { get; set; }

	}
}