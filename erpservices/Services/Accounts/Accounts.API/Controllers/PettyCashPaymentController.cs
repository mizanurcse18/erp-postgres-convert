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
    public class PettyCashPaymentController : BaseController
    {
        private readonly IPettyCashPaymentManager Manager;
        private readonly IPettyCashReimburseManager ReimburseManager;

        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public PettyCashPaymentController(IPettyCashPaymentManager manager, IPettyCashReimburseManager reimburseManager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            ReimburseManager = reimburseManager;
            _notificationHub = notificationHub;
        }

        [HttpPost("GetAllApprovedReimburseClaimList")]
        public async Task<IActionResult> GetAllApprovedReimburseClaimList([FromBody] GridParameter parameters)
        {
            var list = Manager.GetAllApprovedReimburseClaimList(parameters);
            return OkResult(new { parentDataSource = list });
        }


        [HttpPost("GetListForGrid")]
        public async Task<IActionResult> GetListForGrid([FromBody] GridParameter parameters)
        {
            var list = Manager.GetPettyCashPaymenteAllList(parameters);
            return OkResult(new { parentDataSource = list });
        }
        //[HttpGet("GetInvoiceForTaxationVetting/{InvoiceMasterID:int}/{IsAdvanceInvoice:bool}/{POMasterID:int}")]
        [HttpGet("GetReimburseDataForPayment/{PCRMID:int}")]
        public async Task<IActionResult> GetReimburseDataForPayment(int PCRMID)
        {
            var master = await ReimburseManager.GetPettyCashReimburseMaster(PCRMID);
            var invoiceChild = await ReimburseManager.GetPettyCashReimburseChild(PCRMID);
            return OkResult(new { Master = master, InvoiceChild = invoiceChild });

        }
        [HttpPost("SaveChanges")]
        public IActionResult SaveChanges([FromBody] PettyCashPaymentClaimDto payment)
        {
            var response = Manager.SaveChanges(payment).Result;
            _notificationHub.Clients.All.ReceiveNotification("PettyCashPayment");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpGet("Get/{PCPMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> Get(int PCPMID, int ApprovalProcessID)
        {

            var payment = await Manager.GetPettyCashPaymentMaster(PCPMID);
            var child = await Manager.GetPettyCashPaymentChild(PCPMID);
            var reimburse = await ReimburseManager.GetPettyCashReimburseMaster((int)child[0].PCRMID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            var attachments = Manager.GetAttachments(PCPMID, "PettyCashPaymentMaster");
            return OkResult(new { Master = payment, InvoiceChild = child, Reimburse = reimburse, Comments = comments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, Attachments = attachments });
        }


        [HttpGet("GetForRessessment/{PCPMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetForRessessment(int PCPMID,int ApprovalProcessID)
        {
            var payment = await Manager.GetPettyCashPaymentMaster(PCPMID);
            var child = await Manager.GetPettyCashPaymentChild(PCPMID);
            var reimburse = await ReimburseManager.GetPettyCashReimburseMaster((int)child[0].PCRMID);
            var attachments = Manager.GetAttachments(PCPMID, "PettyCashPaymentMaster");
            return OkResult(new { Master = payment, InvoiceChild = child, Reimburse = reimburse, Attachments = attachments});

        }

        [HttpGet("GetPettyCashPaymentReport/{PCPMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetPettyCashPaymentReportGet(int PCPMID, int ApprovalProcessID)
        {

            var payment = await Manager.GetPettyCashPaymentMaster(PCPMID);
            var child = await Manager.GetPettyCashPaymentChild(PCPMID);
            var reimburse = await ReimburseManager.GetPettyCashReimburseMaster((int)child[0].PCRMID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            var attachments = Manager.GetAttachments(PCPMID, "PettyCashPaymentMaster");
            var approvalFeedback = Manager.ReportForApprovalFeedback(PCPMID, (int)Util.ApprovalType.PettyCashPaymentClaim);
            return OkResult(new { Master = payment, ChildList = child, Reimburse = reimburse, Comments = comments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, Attachments = attachments, ApprovalFeedback = approvalFeedback });
        }

        [HttpGet("GetAllExport")]
        public IActionResult GetAllExport(string WhereCondition, string FromDate, string ToDate)
        {
            var model = Manager.GetAllExport(WhereCondition, FromDate, ToDate);
            return OkResult(model.Result);
        }


        [HttpGet("GetPettyCashApprovedReimburseClaimData/{InvoiceMasterID:int}/{IPaymentMasterID:int}/{PCPMID:int}")]
        public IActionResult GetPettyCashApprovedReimburseClaimData(int InvoiceMasterID, int IPaymentMasterID, int PCPMID)
        {

            var InvoiceList = Manager.GetPettyCashApprovedReimburseClaimData(InvoiceMasterID, IPaymentMasterID, PCPMID).Result;
            return OkResult(new { InvoiceList });
        }
    }
}
