
using System;
using Security.DAL.Entities;
using Manager.Core.Mapper;
using DAL.Core.EntityBase;

namespace Security.Manager
{
	[AutoMap(typeof(UserCompany)), Serializable]
	public class UserCompanyDto : Auditable
	{
		public int UserID { get; set; }
		public bool IsDefault { get; set; }
		public string CompanyAddress { get; set; }
	}
}