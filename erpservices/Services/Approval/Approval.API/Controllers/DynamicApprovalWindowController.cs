using API.Core;
using API.Core.Hubs;
using API.Core.Interface;
using Core;
using Core.Extensions;
using Approval.Manager.Dto;
using Approval.Manager.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Approval.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class DynamicApprovalWindowController : BaseController
    {
        private readonly IDynamicApprovalWindowManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;
        public DynamicApprovalWindowController(IDynamicApprovalWindowManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }
        [HttpGet("GetDynamicApprovalWindowSettings/{id:int}")]
        public async Task<IActionResult> GetDynamicApprovalWindowSettings(int id)
        {
            var master = await Manager.GetDynamicApprovalWindow(id);
            //id = master.IsNullOrDbNull() || master.DAPEID.IsZero() ? 0 : (int)master.DAPEID;
            return OkResult(new { Master = master });
        }
        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] DynamicApprovalPanelWindowDto settings)
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
