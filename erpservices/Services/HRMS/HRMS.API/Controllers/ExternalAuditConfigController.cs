using API.Core;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class ExternalAuditConfigController : BaseController
    {
        private readonly IExternalAuditConfigManager Manager;
        
        public ExternalAuditConfigController(IExternalAuditConfigManager manager)
        {
            Manager = manager;
        }
        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAll()
        {
            var mapList = await Manager.GetAll();
            return OkResult(mapList[0]);
        }

        [HttpPost("Save")]
        public IActionResult SaveChanges([FromBody] ExternalAuditConfig obj)
        {
            var response = Manager.SaveChanges(obj).Result;
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

    }
}
