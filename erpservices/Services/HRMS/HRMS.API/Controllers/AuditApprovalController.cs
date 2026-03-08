using API.Core;
using API.Core.Hubs;
using API.Core.Interface;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using HRMS.Manager.Interfaces;
using System.Collections.Generic;
using HRMS.Manager.Dto;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class AuditApprovalController : BaseController
    {
        private readonly IAuditApprovalManager Manager;

        public AuditApprovalController(IAuditApprovalManager manager)
        {
            Manager = manager;
        }

        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] List<AuditApprovalDto> settings)
        {
            var response = await Manager.Save(settings);
            return OkResult(new { status = response.Item1, message = response.Item2 });
            
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var data = await Manager.GetAll();
            return OkResult(data);
        }
    }
}
