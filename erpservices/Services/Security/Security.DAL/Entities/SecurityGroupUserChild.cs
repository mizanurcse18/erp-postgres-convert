using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Security.DAL.Entities
{
	[Table("SecurityGroupUserChild")]
	public class SecurityGroupUserChild : Auditable
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int SecurityGroupUserChildID { get; set; }
		[Loggable]
		public int SecurityGroupID { get; set; }
		[Loggable]
		public int UserID { get; set; }

	}
}
