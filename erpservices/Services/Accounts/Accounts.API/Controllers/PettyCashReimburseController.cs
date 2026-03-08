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
    public class PettyCashReimburseController : BaseController
    {
        private readonly IPettyCashReimburseManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public PettyCashReimburseController(IPettyCashReimburseManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }



        //[HttpPost("GetAll")]
        //public async Task<IActionResult> GetAll([FromBody] GridParameter parameters)
        //{
        //    var list = Manager.GetPettyCashExpenseClaimList(parameters);
        //    return OkResult(new { parentDataSource = list });
        //}
        //[HttpGet("GetExpenseForReAssessment/{PCEMID:int}")]
        //public async Task<IActionResult> GetExpenseForReAssessment(int PCEMID)
        //{

        //    var expenseClaim = await Manager.GetPettyCashExpenseClaim(PCEMID);
        //    var expenseClaimChild = await Manager.GetPettyCashExpenseClaimChild(PCEMID);
        //    return OkResult(new { Master = expenseClaim, ChildList = expenseClaimChild });
        //}

       

        //[HttpGet("GetReport/{PCEMID:int}/{ApprovalProcessID:int}")]
        //public async Task<IActionResult> GetReport(int PCEMID, int ApprovalProcessID)
        //{
        //    var expenseClaim = await Manager.GetPettyCashExpenseClaim(PCEMID);
        //    var expenseClaimChild = await Manager.GetPettyCashExpenseClaimChild(PCEMID);
        //    var comments = Manager.GetApprovalComment(ApprovalProcessID);
        //    var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
        //    var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
        //    var approvalFeedback = Manager.ReportApprovalFeedback(PCEMID);
        //    var attachments = Manager.GetAttachments(PCEMID, "PettyCashExpenseChild");
        //    return OkResult(new { Master = expenseClaim, ChildList = expenseClaimChild, Comments = comments, RejectedMembers = rejectedMembers, ApprovalFeedback = approvalFeedback, ForwardingMembers = forwardingMembers, Attachments = attachments });
        //}


        //[HttpPost("GetAllForDisbursement")]
        //public async Task<IActionResult> GetAllForDisbursement([FromBody] GridParameter parameters)
        //{
        //    var list = Manager.GetPettyCashExpenseAndAdvanceList(parameters);
        //    return OkResult(new { parentDataSource = list });
        //}





        [HttpPost("GetListForGrid")]
        public async Task<IActionResult> GetListForGrid([FromBody] GridParameter parameters)
        {
            var list = Manager.GetPettyCashReimburseAllList(parameters);
            return OkResult(new { parentDataSource = list });
        }


        [HttpGet("GetPettyCashReimburseClaimData/{InvoiceMasterID:int}/{IPaymentMasterID:int}/{PCRMID:int}")]
        public IActionResult GetPettyCashReimburseClaimData( int InvoiceMasterID, int IPaymentMasterID, int PCRMID)
        {

            var InvoiceList = Manager.GetPettyCashReimburseClaimData( InvoiceMasterID, IPaymentMasterID, PCRMID).Result;
            return OkResult(new { InvoiceList });
        }
        [HttpPost("SaveChanges")]
        public IActionResult SaveChanges([FromBody] PettyCashReimburseClaimDto expense)
        {
            var response = Manager.SaveChanges(expense).Result;
            _notificationHub.Clients.All.ReceiveNotification("PettyCashReimburse");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpGet("Get/{PCRMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> Get(int PCRMID, int ApprovalProcessID)
        {
            var master = await Manager.GetPettyCashReimburseMaster(PCRMID);
            var child = await Manager.GetPettyCashReimburseChild(PCRMID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            var attachments = Manager.GetAttachments(PCRMID, "PettyCashReimburseMaster");
            return OkResult(new { Master = master, ChildList = child, Comments = comments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, Attachments = attachments });
        }

        [HttpGet("GetForReAssessment/{PCRMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetForReAssessment(int PCRMID, int ApprovalProcessID)
        {
            var master = await Manager.GetPettyCashReimburseMaster(PCRMID);
            var child = await Manager.GetPettyCashReimburseChild(PCRMID);
            var attachments = Manager.GetAttachments(PCRMID, "PettyCashReimburseMaster");
            return OkResult(new { Master = master, ChildList = child, Attachments = attachments });
        }

        [HttpGet("GetReimburseReport/{PCRMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetReimburseReport(int PCRMID, int ApprovalProcessID)
        {
            var master = await Manager.GetPettyCashReimburseMaster(PCRMID);
            var child = await Manager.GetPettyCashReimburseChild(PCRMID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            var attachments = Manager.GetAttachments(PCRMID, "PettyCashReimburseMaster");
            var approvalFeedback = Manager.ReportApprovalFeedback(PCRMID);

            return OkResult(new { Master = master, ChildList = child, Comments = comments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, Attachments = attachments, approvalFeedback });
        }

    }
}
