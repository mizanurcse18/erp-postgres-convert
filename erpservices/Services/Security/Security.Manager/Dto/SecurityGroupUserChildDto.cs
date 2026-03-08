
using System;
using Security.DAL.Entities;
using Manager.Core.Mapper;
using DAL.Core.EntityBase;

namespace Security.Manager
{
	[AutoMap(typeof(SecurityGroupUserChild)), Serializable]
	public class SecurityGroupUserChildDto : Auditable
	{
		public int SecurityGroupUserChildID { get; set; }
		public int SecurityGroupID { get; set; }
		public int UserID { get; set; }
		//public string SecurityGroupName { get; set; }
		public string SecurityGroupName { get; set; }
	}
}