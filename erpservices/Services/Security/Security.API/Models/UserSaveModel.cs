using Security.Manager;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Security.API.Models
{
    public class UserSaveModel
    {
        public UserDto UserInformation { get; set; }
        public List<SecurityGroupUserChildDto> UserGroups { get; set; }
        public List<UserCompanyDto> UserCompanies { get; set; }
    }
}
