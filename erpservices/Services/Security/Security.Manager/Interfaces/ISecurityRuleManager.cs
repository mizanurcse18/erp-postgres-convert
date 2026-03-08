using Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Security.Manager.Interfaces
{
    public interface ISecurityRuleManager
    {
        Task<List<SecurityRuleMasterDto>> GetSecurityRuleTables();
        Task<IEnumerable<Dictionary<string, object>>> GetSecurityRuleMasterListWithDetails();
        Task<SecurityRuleMasterDto> GetSecurityRuleTable(int primaryID);
        Task<GridModel> GetSecurityRuleChilds(GridParameter parameters);
        Task<List<Dictionary<string, object>>> GetSecurityRuleChilds(int SecurityRuleID);
        Task<SecurityRuleMasterDto> SaveChanges(SecurityRuleMasterDto master, List<SecurityRulePermissionChildDto> childs = null);
        Task Delete(SecurityRuleMasterDto master, List<SecurityRulePermissionChildDto> childs);
        Task RemoveSecurityRule(int securityRuleID);
    }
}
