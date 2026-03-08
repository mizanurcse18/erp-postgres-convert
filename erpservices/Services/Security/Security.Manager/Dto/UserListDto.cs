using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Newtonsoft.Json;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    [AutoMap(typeof(User)),Serializable]
    public class UserListDto : EntityBase
    {        
        public int UserID { get; set; }
        [JsonIgnore]
        public string UserName { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }       
        public int? ApplicationID { get; set; }
        public string CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public int AccessFailedCount { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
