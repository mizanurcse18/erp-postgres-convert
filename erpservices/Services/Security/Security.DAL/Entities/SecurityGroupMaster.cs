using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
	[Table("SecurityGroupMaster")]
	public class SecurityGroupMaster : Auditable
	{
		[Key]
		public int SecurityGroupID { get; set; }
		[Loggable]
		public string SecurityGroupName { get; set; }
		[Loggable]
		public string SecGroupDescription { get; set; }
		//public string CompanyID { get; set; }

	}
}
