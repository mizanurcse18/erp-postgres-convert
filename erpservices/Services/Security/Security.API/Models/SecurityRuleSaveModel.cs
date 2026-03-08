using Security.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Security.API.Models
{
    public class SecurityRuleSaveModel
    {
        public SecurityRuleMasterDto MasterModel { get; set; }
        public List<SecurityRulePermissionChildDto> ChildModels { get; set; }
    }
}
