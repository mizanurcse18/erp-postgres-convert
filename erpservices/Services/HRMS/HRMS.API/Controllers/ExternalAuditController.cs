using API.Core;
using API.Core.Hubs;
using API.Core.Interface;
using Core;
using Core.Extensions;
using DocumentFormat.OpenXml.Bibliography;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize,ApiController, Route("[controller]")]
    public class ExternalAuditController : BaseController
    {
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;
        private readonly IExternalAuditManager Manager;
        public ExternalAuditController(IExternalAuditManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }
        [HttpPost("SaveExternalAudit")]
        public IActionResult SaveExternalAudit([FromBody] AuditMasterDto dto)
        {
            var response = Manager.SaveChanges(dto).Result;
            _notificationHub.Clients.All.ReceiveNotification("ExternalAudit");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpPost("TagNewWallet")]
        public IActionResult TagNewWallet([FromBody] dynamic wallet)
        {
            string walletNo = Convert.ToString(wallet?.walletNo?.Value);
            string walletName = Convert.ToString(wallet?.walletName?.Value);
            int walletTypeID = Convert.ToInt32(wallet?.walletTypeID.Value);

            var response = Manager.AddNewWallet(walletNo, walletName, walletTypeID).Result;
            _notificationHub.Clients.All.ReceiveNotification("ExternalAudit");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpGet("GetQuestionWiseDeptPOSM")]
        public IActionResult GetQuestionWiseDeptPOSM()
        {
            var model = Manager.GetExternalAuditQuestionDeptPOSM().Result;
            return OkResult(new { model });
        }

        [HttpPost("GetListForGrid")]
        public IActionResult GetListForGrid([FromBody] GridParameter parameters)
        {
            var model = Manager.GetListForGrid(parameters);
            return OkResult(new { parentDataSource = model });
        }

        [HttpPost("GetAllListForGrid")]
        public IActionResult GetAllListForGrid([FromBody] GridParameter parameters)
        {
            var model = Manager.GetAllListForGrid(parameters);
            _notificationHub.Clients.All.ReceiveNotification("ExternalAudit");
            return OkResult(new { parentDataSource = model });
        }
        [HttpGet("Get/{EAMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> Get(int EAMID, int ApprovalProcessID)
        {
            var masterData = await Manager.GetExternalAuditMaster(EAMID);
            var masterAttachments = await Manager.GetMasterAttachments(EAMID);
            var auditDetails = await Manager.GetExternalAuditChild(EAMID);
            var deptData = await Manager.GetDepartmentDetails(EAMID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var approvalFeedback = Manager.EmployeeApprovalMemberFeedbackForExternalAudit(EAMID, ApprovalProcessID);
            return OkResult(new
            {
                Master = masterData,
                MasterAttachments = masterAttachments,
                AuditDetails = auditDetails,
                Departments = deptData,
                RejectedMembers = rejectedMembers,
                ApprovalFeedback = approvalFeedback,
                Comments = comments
            });
        }
    }
}
