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
using System.Collections.Generic;

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class SecurityRuleController : BaseController
    {
        private readonly ISecurityRuleManager Manager;

        public SecurityRuleController(ISecurityRuleManager manager)
        {
            Manager = manager;
        }

        // GET: /SecurityRule/GetAll
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var securityRules = await Manager.GetSecurityRuleMasterListWithDetails();
            //if (securityRules.Count.IsZero()) return NoContentResult();
            return OkResult(securityRules);
        }

        // GET: /SecurityRule/Get/{primaryID}
        [HttpGet("Get/{securityRuleID:int}")]
        public async Task<IActionResult> Get(int securityRuleID)
        {
            //if (!HasViewPrivilege())
            //    return ValidationResult("You have no privilege to view any information.");
            var rule = await Manager.GetSecurityRuleTable(securityRuleID);
            rule.ChildModels = new List<SecurityRulePermissionChildDto>();
            return OkResult(rule);
        }

        // POST: /SecurityRule/GetRuleMenuPermissions
        [HttpPost("GetRuleMenuPermissions")]
        public async Task<IActionResult> GetRuleMenuPermissions([FromBody]GridParameter parameters)
        {
            //if (!HasViewPrivilege())
            //    return ValidationResult("You have no privilege to view any information.");
            var model = await Manager.GetSecurityRuleChilds(parameters);
            return OkResult(new { parentDataSource = model, parentKey = "SecurityRulePermissionID" });
        }

        // GET: /SecurityRule/GetRuleMenuPermissionList
        [HttpGet("GetRuleMenuPermissionList/{securityRuleID:int}")]
        public async Task<IActionResult> GetRuleMenuPermissionList(int securityRuleID)
        {
            //if (!HasViewPrivilege())
            //    return ValidationResult("You have no privilege to view any information.");
            var list = await Manager.GetSecurityRuleChilds(securityRuleID);
            return OkResult(list);
        }


        // POST: /SecurityRule/CreateSecurityRule
        [HttpPost("SaveSecurityRule")]
        public async Task<IActionResult> SaveSecurityRule([FromBody]SecurityRuleSaveModel securityRule)
        {
            //await Manager.CreateAsync(user);
            securityRule.MasterModel = await Manager.SaveChanges(securityRule.MasterModel, securityRule.ChildModels);
            return await Get(securityRule.MasterModel.SecurityRuleID);
        }

        // POST: /SecurityRule/DeleteSecurityRule
        [HttpDelete("DeleteSecurityRule")]
        public async Task<IActionResult> DeleteSecurityRule([FromBody]SecurityRuleSaveModel securityRule)
        {
            //await Manager.UpdateAsync(user);
            await Manager.Delete(securityRule.MasterModel, securityRule.ChildModels);
            return DeletedResult();
        }

        // POST: /SecurityRule/RemoveSecurityGroup
        [HttpGet("RemoveSecurityRule/{SecurityRuleID:int}")]
        public async Task<IActionResult> RemoveSecurityRule(int SecurityRuleID)
        {
            //await Manager.UpdateAsync(user);
            await Manager.RemoveSecurityRule(SecurityRuleID);
            return OkResult(new { success = true });
        }
    }
}
