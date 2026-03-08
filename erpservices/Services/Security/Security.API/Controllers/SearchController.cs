using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Core;
using Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Manager;
using Security.Manager.Interfaces;

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class SearchController : BaseController
    {
        private readonly ISearchManager Manager;
        public SearchController(ISearchManager manager)
        {
            Manager = manager;
        }

        // POST: /Search/GetUsers
        [HttpPost("GetUsers")]
        public IActionResult GetUsers([FromBody]GridParameter parameters)
        {
            var model = Manager.GetUsers(parameters);
            return OkResult(new { parentDataSource = model, parentKey = "UserID" });
        }

        // POST: /Search/GetUsers
        [HttpPost("GetSecurityRules")]
        public IActionResult GetSecurityRules([FromBody]GridParameter parameters)
        {
            var model = Manager.GetSecurityRules(parameters);
            return OkResult(new { parentDataSource = model, parentKey = "SecurityRuleID" });
        }

        // POST: /Search/GetUsers
        [HttpPost("GetSecurityGroups")]
        public IActionResult GetSecurityGroups([FromBody]GridParameter parameters)
        {
            var model = Manager.GetSecurityGroups(parameters);
            return OkResult(new { parentDataSource = model, parentKey = "SecurityGroupID" });
        }
    }
}