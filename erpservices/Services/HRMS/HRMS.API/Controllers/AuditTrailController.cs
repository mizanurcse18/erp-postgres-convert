using API.Core;
using API.Core.Hubs;
using API.Core.Interface;
using Core;
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
    public class AuditTrialController : BaseController
    {
        private readonly IAuditTrialManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;
        public AuditTrialController(IAuditTrialManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        #region Audit Trial

        [HttpPost("GetAuditTraialList")]
        public async Task<IActionResult> GetAuditTraialList([FromBody] GridParameter parameters)
        {
            var model = Manager.GetAllAuditTrialList(parameters);
            return OkResult(new { parentDataSource = model });

        }

        [HttpPost("GetAuditTraialListForExcel")]
        public async Task<IActionResult> GetAuditTraialListForExcel([FromBody] GridParameter parameters)
        {
            var modelExcel = Manager.GetAllAuditTrialListForExcel(parameters).Result;
            return OkResult(modelExcel);

        }

        [HttpGet("GetAuditTrialData/{PATID:int}")]
        public async Task<IActionResult> GetAuditTrialData(int patID)
        {
            var division = await Manager.GetAuditTrialData(patID);
            return OkResult(division);
        }

        #endregion
    }
}
