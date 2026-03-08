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
    public class PettyCashAdvanceController : BaseController
    {
        private readonly IPettyCashAdvanceManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public PettyCashAdvanceController(IPettyCashAdvanceManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        [HttpGet("GetPettyCashAdvanceForReAssessment/{PCAMID:int}")]
        public async Task<IActionResult> GetPettyCashAdvanceForReAssessment(int PCAMID)
        {

            var pettycashAdvance = await Manager.GetPettyCashAdvance(PCAMID);
            var pettycashAdvanceChild = await Manager.GetPettyCashAdvanceChild(PCAMID);
            return OkResult(new { Master = pettycashAdvance, ChildList = pettycashAdvanceChild });
        }
        [HttpPost("GetAll")]
        public async Task<IActionResult> GetAll([FromBody] GridParameter parameters)
        {
            var list = Manager.GetPettyCashAdvanceClaimList(parameters);
            return OkResult(new { parentDataSource = list });
        }
        [HttpGet("Get/{PCAMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> Get(int PCAMID, int ApprovalProcessID)
        {
            //if (!HasViewPrivilege())
            //    return ValidationResult("You have no privilege to view any information.");
            var pettycashAdvance = await Manager.GetPettyCashAdvance(PCAMID);
            var pettycashAdvanceChild = await Manager.GetPettyCashAdvanceChild(PCAMID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            //var attachments = Manager.GetAttachments(NFAID);  yarn
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(PCAMID, (int)Util.ApprovalType.PettyCashAdvanceClaim, (int)Util.ApprovalPanel.PettyCashAdvanceClaim).Result;
            return OkResult(new { Master = pettycashAdvance, ChildList = pettycashAdvanceChild, Comments = comments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers });
        }

        [HttpGet("Get/{PCAMID:int}")]
        public async Task<IActionResult> Get(int PCAMID)
        {
            //if (!HasViewPrivilege())
            //    return ValidationResult("You have no privilege to view any information.");
            var pettycashAdvance = await Manager.GetPettyCashAdvance(PCAMID);
            var pettycashAdvanceChild = await Manager.GetPettyCashAdvanceChild(PCAMID);
            return OkResult(new { Master = pettycashAdvance, ChildList = pettycashAdvanceChild });
        }

        [HttpPost("SavePettyCashAdvanceMaster")]
        public IActionResult SavePettyCashAdvanceMaster([FromBody] PettyCashAdvanceDto dto)
        {
            var response = Manager.SaveChanges(dto).Result;
            _notificationHub.Clients.All.ReceiveNotification("PettyCashAdvance");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpGet("RemovePettyCashAdvanceMaster/{PCAMID:int}/{ApprovalProcessID:int}")]
        public IActionResult RemovePettyCashAdvanceMaster(int PCAMID,int ApprovalProcessID)
        {
            Manager.RemovePettyCashAdvanceMaster(PCAMID, ApprovalProcessID);
            return OkResult(PCAMID);
        }


        [HttpGet("GetPettyCashAdvanceReport/{PCAMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetPettyCashAdvanceReport(int PCAMID, int ApprovalProcessID)
        {
            var ead = await Manager.GetPettyCashAdvance(PCAMID);
            PCAMID = ead.IsNullOrDbNull() || ead.PCAMID.IsZero() ? 0 : PCAMID;
            ApprovalProcessID = PCAMID.IsNotZero() ? ApprovalProcessID : 0;
            var master = Manager.GetPettyCashAdvance(PCAMID);

            var itemDetails = Manager.GetPettyCashAdvanceChild(PCAMID).Result;
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            //var attachments = Manager.ReportForRequestSupportAttachments(PCAMID);
            var approvalFeedback = Manager.ReportForPCAApprovalFeedback(PCAMID, (int)Util.ApprovalType.PettyCashAdvanceClaim);
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.PettyCashAdvanceClaim, PCAMID);
            return OkResult(new
            {
                Master = master.Result,
                ChildItem = itemDetails,
                Comments = comments,
                //Attachments = attachments,
                ApprovalFeedback = approvalFeedback,
                ForwardInfoComments = forwardInfoComments
            });
        }


        [HttpPost("GetAllResubmitList")]
        public async Task<IActionResult> GetAllResubmitList([FromBody] GridParameter parameters)
        {
            var list = Manager.GetPettyCashAdvanceResubmitClaimList(parameters);
            return OkResult(new { parentDataSource = list });
        }


        [HttpGet("PettyCashAdvanceResubmit/{PCAMID:int}")]
        public async Task<IActionResult> PettyCashAdvanceResubmit(int PCAMID)
        {
            var pettycashAdvance = await Manager.GetPettyCashAdvance(PCAMID);
            var pettycashAdvanceChild = await Manager.GetPettyCashAdvanceResubmitChild(PCAMID);
            return OkResult(new { Master = pettycashAdvance, ChildList = pettycashAdvanceChild });
        }

        [HttpGet("GetPettyCashAdvanceResubmit/{PCAMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetPettyCashAdvanceResubmit(int PCAMID, int ApprovalProcessID)
        {
            var pettycashAdvance = await Manager.GetPettyCashAdvanceResubmit(PCAMID, ApprovalProcessID);
            var pettycashAdvanceChild = await Manager.GetPettyCashAdvanceResubmitChild(PCAMID);
            var comments = Manager.GetResubmitApprovalComment(ApprovalProcessID);
            //var attachments = Manager.GetAttachments(NFAID);  yarn
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(PCAMID, (int)Util.ApprovalType.PettyCashAdvanceResubmitClaim, (int)Util.ApprovalPanel.PettyCashAdvanceClaim).Result;
            return OkResult(new { Master = pettycashAdvance, ChildList = pettycashAdvanceChild, Comments = comments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers });
        }


        [HttpPost("SavePettyCashAdvanceResubmit")]
        public IActionResult SavePettyCashAdvanceResubmit([FromBody] PettyCashAdvanceDto dto)
        {
            var response = Manager.SaveChangesResubmit(dto).Result;
            _notificationHub.Clients.All.ReceiveNotification("ResubmitAdvanceClaim");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpGet("GetPettyCashAdvanceResubmitReport/{PCAMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetPettyCashAdvanceResubmitReport(int PCAMID, int ApprovalProcessID)
        {
            var pettycashAdvance = await Manager.GetPettyCashAdvanceResubmit(PCAMID, ApprovalProcessID);
            var pettycashAdvanceChild = await Manager.GetPettyCashAdvanceResubmitChild(PCAMID);
            var comments = Manager.GetResubmitApprovalComment(ApprovalProcessID);
            var attachments = Manager.GetAttachments(PCAMID, "PettyCashAdvanceChild");
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(PCAMID, (int)Util.ApprovalType.PettyCashAdvanceResubmitClaim, (int)Util.ApprovalPanel.PettyCashAdvanceClaim).Result;
            var approvalFeedback = Manager.ReportForPCAApprovalFeedback(PCAMID, (int)Util.ApprovalType.PettyCashAdvanceResubmitClaim);
            return OkResult(new { Master = pettycashAdvance, ChildList = pettycashAdvanceChild, Comments = comments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ApprovalFeedback= approvalFeedback, Attachments = attachments });
        }
    }
}
