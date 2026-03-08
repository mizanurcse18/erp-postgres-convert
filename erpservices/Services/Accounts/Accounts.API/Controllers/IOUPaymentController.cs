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

using System.Collections.Generic;
using API.Core.Hubs;
using API.Core.Interface;
using Microsoft.AspNetCore.SignalR;
using Accounts.Manager;
using Accounts.Manager.Dto;

namespace Accounts.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class IOUPaymentController : BaseController
    {
        private readonly IIOUManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public IOUPaymentController(IIOUManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        [HttpGet("GetIOUForReAssessment/{IOUMasterID:int}")]
        public async Task<IActionResult> GetIOUForReAssessment(int IOUMasterID)
        {

            var iou = await Manager.GetIOU(IOUMasterID);
            var iouChild = await Manager.GetIOUChild(IOUMasterID);
            return OkResult(new { Master = iou, ChildList = iouChild });
        }
        [HttpPost("GetAll")]
        public async Task<IActionResult> GetAll([FromBody] GridParameter parameters)
        {
            var list = Manager.GetIOUClaimList(parameters);
            return OkResult(new { parentDataSource = list });
        }

        [HttpPost("GetAllApprovedHistory")]
        public async Task<IActionResult> GetAllApprovedHistory([FromBody] GridParameter parameters)
        {
            var list = Manager.GetIOUClaimList(parameters);
            return OkResult(new { parentDataSource = list });
        }
        [HttpGet("Get/{IOUMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> Get(int IOUMasterID, int ApprovalProcessID)
        {
            //if (!HasViewPrivilege())
            //    return ValidationResult("You have no privilege to view any information.");
            var iou = await Manager.GetIOU(IOUMasterID);
            var iouChild = await Manager.GetIOUChild(IOUMasterID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            //var attachments = Manager.GetAttachments(NFAID);  yarn
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(IOUMasterID, (int)Util.ApprovalType.IOUClaim, (int)Util.ApprovalPanel.IOUClaimAboveTheLimit).Result;
            return OkResult(new { Master = iou, ChildList = iouChild, Comments = comments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers });
        }

        [HttpGet("Get/{IOUMasterID:int}")]
        public async Task<IActionResult> Get(int IOUMasterID)
        {
            //if (!HasViewPrivilege())
            //    return ValidationResult("You have no privilege to view any information.");
            var iou = await Manager.GetIOU(IOUMasterID);
            var iouChild = await Manager.GetIOUChild(IOUMasterID);
            return OkResult(new { Master = iou, ChildList = iouChild });
        }

        [HttpPost("SaveIOUMaster")]
        public IActionResult SaveIOUMaster([FromBody] IOUDto dto)
        {
            var response = Manager.SaveChanges(dto).Result;
            _notificationHub.Clients.All.ReceiveNotification("IOUClaim");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpGet("RemoveIOUMaster/{IOUMasterID:int}")]
        public IActionResult RemoveIOUMaster(int IOUMasterID)
        {
            Manager.RemoveIOUMaster(IOUMasterID);
            return OkResult(IOUMasterID);
        }
    }
}
