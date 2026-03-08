using API.Core;
using API.Core.Hubs;
using API.Core.Interface;
using Core;
using Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Approval.Manager.Dto;
using Approval.Manager.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Approval.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class DOAController : BaseController
    {
        private readonly IDOAManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public DOAController(IDOAManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] DOADto DOA)
        {
            var response = Manager.SaveChanges(DOA).Result;
            _notificationHub.Clients.All.ReceiveNotification("DOA");

            return OkResult(new { status = response.Item1, message = response.Item2 });
        }
        [HttpPost("SaveForHR")]
        public IActionResult SaveForHR([FromBody] DOADto DOA)
        {
            var response = Manager.SaveChanges(DOA).Result;
            _notificationHub.Clients.All.ReceiveNotification("DOA");

            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpPost("GetAll")]
        public IActionResult GetAll([FromBody] GridParameter parameters)
        {
            var list = Manager.GetDOAList(parameters);
            return OkResult(new { parentDataSource = list });
        }

        [HttpPost("GetAllForHR")]
        public IActionResult GetAllForHR([FromBody] GridParameter parameters)
        {
            var list = Manager.GetDOAList(parameters);
            return OkResult(new { parentDataSource = list });
        }



        [HttpGet("Get/{DOAMID:int}")]
        public async Task<IActionResult> Get(int DOAMID)
        {
            var master = await Manager.GetDOAMaster(DOAMID);

            var doaApprovalPanelEmployee = await Manager.GetDOAApprovalPanelEmployee(DOAMID);
            var attachments = Manager.GetAttachments(DOAMID);

            return OkResult(new { Master = master, DOAApprovalPanelEmployeeList = doaApprovalPanelEmployee, Attachments = attachments });
        }

        [HttpGet("GetForHR/{DOAMID:int}")]
        public async Task<IActionResult> GetForHR(int DOAMID)
        {
            var master = await Manager.GetDOAMaster(DOAMID);

            var doaApprovalPanelEmployee = await Manager.GetDOAApprovalPanelEmployee(DOAMID);
            var attachments = Manager.GetAttachments(DOAMID);

            return OkResult(new { Master = master, DOAApprovalPanelEmployeeList = doaApprovalPanelEmployee, Attachments = attachments });
        }


    }
}