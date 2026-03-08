using System;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

using Core;
using API.Core;
using Core.Extensions;
using Core.AppContexts;

using Security.Manager.Dto;
using Security.Manager.Interfaces;
using Security.API.Models;
using Security.Manager;

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class SecurityGroupController : BaseController
    {
        private readonly ISecurityGroupManager Manager;

        public SecurityGroupController(ISecurityGroupManager manager)
        {
            Manager = manager;
        }

        // GET: /SecurityGroup/GetAll
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var securityGroups = await Manager.GetSecurityGroupMasterListWithDetails();
            //if (securityGroups.Count.IsZero()) return NoContentResult();
            return OkResult(securityGroups);
        }

        // GET: /SecurityGroup/Get/{primaryID}
        [HttpGet("Get/{securityGroupID:int}")]
        public async Task<IActionResult> Get(int securityGroupID)
        {
            //if (!HasViewPrivilege())
            //    return ValidationResult("You have no privilege to view any information.");
            var group = await Manager.GetSecurityGroup(securityGroupID);
            var groupRuleRows = Manager.GetSecurityRulesByGroupID(securityGroupID);

            return OkResult(new { SecurityGroup = group, groupRuleRows });
        }

        // POST: /SecurityGroup/GetGroupRules
        [HttpPost("GetGroupRules")]
        public async Task<IActionResult> GetGroupRules([FromBody] GridParameter parameters)
        {
            //if (!HasViewPrivilege())
            //    return ValidationResult("You have no privilege to view any information.");
            var model = Manager.GetSecurityGroupRules(parameters);
            return OkResult(new { parentDataSource = model, parentKey = "SecurityRuleID" });
        }

        // POST: /SecurityGroup/GetMenuPermissions
        [HttpPost("GetMenuPermissions")]
        public async Task<IActionResult> GetMenuPermissions([FromBody] GridParameter parameters)
        {
            //if (!HasViewPrivilege())
            //    return ValidationResult("You have no privilege to view any information.");
            var model = Manager.GetMenuPermissions(parameters);
            return OkResult(new { parentDataSource = model, parentKey = "MenuID" });
        }

        // POST: /SecurityGroup/GetSelectedRuleMenuPermissions
        [HttpPost("GetSelectedRuleMenuPermissions")]
        public async Task<IActionResult> GetSelectedRuleMenuPermissions([FromBody] GridParameter parameters)
        {
            //if (!HasViewPrivilege())
            //    return ValidationResult("You have no privilege to view any information.");
            var model = await Manager.GetSecurityGroupSelectedRuleMenuPermissions(parameters);
            return OkResult(new { parentDataSource = model, parentKey = "MenuID" });
        }

        // POST: /SecurityGroup/CreateSecurityGroup
        [HttpPost("SaveSecurityGroup")]
        public async Task<IActionResult> SaveSecurityGroup([FromBody] SecurityGroupSaveModel securityGroup)
        {
            await Manager.SaveChanges(securityGroup.SecurityGroup, securityGroup.SecurityGroupRules);
            return await Get(securityGroup.SecurityGroup.SecurityGroupID);
        }

        // PUT: /SecurityGroup/UpdateSecurityGroup
        [HttpPut("UpdateSecurityGroup")]
        public async Task<IActionResult> UpdateSecurityGroup([FromBody] SecurityGroupSaveModel securityGroupUpdate)
        {
            await Manager.SaveChanges(securityGroupUpdate.SecurityGroup, securityGroupUpdate.SecurityGroupRules);
            return await Get(securityGroupUpdate.SecurityGroup.SecurityGroupID);
        }

        // Delete: /SecurityGroup/DeleteSecurityGroup
        [HttpGet("RemoveSecurityGroup/{SecurityGroupID:int}")]
        public async Task<IActionResult> RemoveSecurityGroup(int SecurityGroupID)
        {
            await Manager.RemoveSecurityGroup(SecurityGroupID);
            return OkResult(new { Success = true });
        }
    }
}
