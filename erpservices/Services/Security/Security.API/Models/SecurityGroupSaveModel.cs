using Security.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Security.API.Models
{
    public class SecurityGroupSaveModel
    {
        public SecurityGroupMasterDto SecurityGroup { get; set; }
        public List<SecurityRuleMasterDto> SecurityGroupRules { get; set; }
    }
}
