using Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Security.Manager
{
    public interface ISecurityGroupManager
    {        
        Task<List<SecurityGroupMasterDto>> GetSecurityGroupTables();
        Task<IEnumerable<Dictionary<string, object>>> GetSecurityGroupMasterListWithDetails();
        Task<SecurityGroupMasterDto> GetSecurityGroup(int primaryID);
        Task<List<SecurityGroupRuleChildDto>> GetSecurityRulesForGroup(int groupID);
        GridModel GetSelectedSecurityRules(int groupID);
        GridModel GetSecurityGroupRules(GridParameter parameters);
        GridModel GetMenuPermissions(GridParameter parameters);
        Task<GridModel> GetSecurityGroupSelectedRuleMenuPermissions(GridParameter parameters);
        Task SaveChanges(SecurityGroupMasterDto securityGroup, List<SecurityRuleMasterDto> securityGroupRules);
        Task RemoveSecurityGroup(int SecurityGroupID);
        Task<IEnumerable<Dictionary<string, object>>> GetSecurityRulesByGroupID(int groupID);
    }
}
