using API.Core;
using API.Core.Hubs;
using API.Core.Interface;
using Core;
using Core.Extensions;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class AnnualLeaveEncashmentWindowController : BaseController
    {
        private readonly IAnnualLeaveEncashmentWindowManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;
        public AnnualLeaveEncashmentWindowController(IAnnualLeaveEncashmentWindowManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }
        [HttpGet("GetAnnualLeaveEncashmentSettings/{id:int}")]
        public async Task<IActionResult> GetAnnualLeaveEncashmentSettings(int id)
        {
            var master = await Manager.GetAnnualLeaveEncashmentWindowMaster(id);
            id = master.IsNullOrDbNull() || master.ALEWMasterID.IsZero() ? 0 : (int)master.ALEWMasterID;
            var child = await Manager.GetAnnualLeaveEncashmentWindowChild(id);
            var settings = await Manager.GetAnnualLeaveEncashmentSettings();
            return OkResult(new { Master = master, ChildList = child, PanelSettings = settings });
        }
        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] AnnualLeaveEncashmentPolicySettingsDto settings)
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

        [HttpGet("UpdateLeaveEncashmentStatus/{ALEWMasterID:long}/{Status:int}")]
        public IActionResult UpdateLeaveEncashmentStatus(long ALEWMasterID,int Status)
        {
            Manager.UpdateLeaveEncashmentStatus(ALEWMasterID, Status);
            return Ok(new { Success = true });
        }


    }
}
