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

using Security.Manager.Dto;
using Security.Manager.Interfaces;
using Security.API.Models;
using Security.Manager;
using System.Collections.Generic;
using API.Core.Hubs;
using API.Core.Interface;
using Microsoft.AspNetCore.SignalR;

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class NFAController : BaseController
    {
        private readonly INFAManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public NFAController(INFAManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        [HttpGet("GetAll/{filterData}")]
        public async Task<IActionResult> GetAll(string filterData)
        {
            var nfas = await Manager.GetNFAList(filterData);
            return OkResult(nfas);
        }

        // POST: /Grid/GetListForGrid
        [HttpPost("GetListForGrid")]
        public IActionResult GetListForGrid([FromBody] GridParameter parameters)
        {            
            var model = Manager.GetListForGrid(parameters);
            model.IsSubmittedFromPopup = parameters.IsSubmittedFromPopup;
            return OkResult(new { parentDataSource = model });
        }
        [HttpPost("GetListForGridNFADashboard")]
        public IActionResult GetListForGridNFADashboard([FromBody] GridParameter parameters)
        {
            var model = Manager.GetListForGridNFADashboard(parameters);
            model.IsSubmittedFromPopup = parameters.IsSubmittedFromPopup;
            return OkResult(new { parentDataSource = model });
        }
        
        [HttpGet("Get/{NFAID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> Get(int NFAID, int ApprovalProcessID)
        {
            var nfa = await Manager.GetNFA(NFAID);
            NFAID = nfa.IsNullOrDbNull() || nfa.NFAID.IsZero() ? 0 : NFAID;
            ApprovalProcessID = NFAID.IsNotZero() ? ApprovalProcessID : 0;
            //var nfaChild = await Manager.GetNFAChild(NFAID);
            //var nfaStrategicChild = await Manager.GetNFAChildStrategic(NFAID);
            var nfaChild = (dynamic)null;
            if (nfa.TemplateID == (int)Util.NFAType.CreateNFA)
            {
                nfaChild = await Manager.GetNFAChild(NFAID);
            }
            else
            {
                nfaChild = await Manager.GetNFAChildStrategic(NFAID);
            }
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var attachments = Manager.GetAttachments(NFAID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(NFAID, (int)Util.ApprovalType.NFA, (int)Util.ApprovalPanel.NFAApprovalPanel).Result;
            return OkResult(new { Master = nfa, ChildList = nfaChild, Comments = comments, Attachments = attachments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers });
        }

        [HttpPost("SaveNFA")]
        public async Task<IActionResult> SaveNFA([FromBody] NFADto NFA)
        {
            var response = await Manager.SaveChanges(NFA);
            _notificationHub.Clients.All.ReceiveNotification("NFA");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        //private async Task<IActionResult> SaveNfa(NFADto NFA)
        //{
        //    var response = Manager.SaveChanges(NFA).Result;
        //    _notificationHub.Clients.All.ReceiveNotification("NFA");
        //    return OkResult(new { status = response.Item1, message = response.Item2 });
        //}

            [HttpGet("GetNFAForReAssessment/{NFAID:int}")]
        public async Task<IActionResult> GetNFAForReAssessment(int NFAID)
        {
            var NFA = await Manager.GetNFAForReAssessment(NFAID);
            return OkResult(NFA);
        }
        [HttpGet("GetNFAReport/{NFAID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetNFAReport(int NFAID, int ApprovalProcessID)
        {
            var nfa = await Manager.GetNFA(NFAID);
            NFAID = nfa.IsNullOrDbNull() || nfa.NFAID.IsZero() ? 0 : NFAID;
            ApprovalProcessID = NFAID.IsNotZero() ? ApprovalProcessID : 0;
            var master = Manager.ReportForNFAMaster(NFAID);
            var child = Manager.ReportForNFAChild(NFAID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var attachments = Manager.ReportForNFAAttachments(NFAID);
            var approvalFeedback = Manager.ReportForNFAApprovalFeedback(NFAID);
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.NFA, NFAID);
            return OkResult(new { Master = master, ChildList = child, Comments = comments, Attachments = attachments, ApprovalFeedback = approvalFeedback, ForwardInfoComments = forwardInfoComments });
        }

        [HttpGet("RemoveNFA/{NFAID:int}/{ApprovalProcessID:int}")]
        public IActionResult RemoveNFA(int NFAID, int ApprovalProcessID)
        {
            Manager.RemoveNFA(NFAID, ApprovalProcessID);
            _notificationHub.Clients.All.ReceiveNotification("NFA");
            return OkResult(NFAID);
        }

        [HttpPost("SaveStrategicNFA")]
        public async Task<IActionResult> SaveStrategicNFA([FromBody] NFADto NFA)
        {
            var response = await Manager.SaveChanges(NFA);
            _notificationHub.Clients.All.ReceiveNotification("NFA");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpGet("GetStrategicNFAForReAssessment/{NFAID:int}")]
        public async Task<IActionResult> GetStrategicNFAForReAssessment(int NFAID)
        {
            var NFA = await Manager.GetStrategicNFAForReAssessment(NFAID);
            return OkResult(NFA);
        }

    }
}
