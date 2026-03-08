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
    public class TaxationVettingPaymentController : BaseController
    {
        private readonly ITaxationVettingMasterManager VettingManager;
        private readonly ITaxationVettingPaymentManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public TaxationVettingPaymentController(ITaxationVettingMasterManager vettingManager, ITaxationVettingPaymentManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            VettingManager = vettingManager;
            Manager = manager;
            _notificationHub = notificationHub;
        }

        [HttpPost("SaveChanges")]
        public async Task<IActionResult> SaveChanges([FromBody] TaxationVettingPaymentDto model)
        {
            var response = await Manager.SaveChanges(model);
            await _notificationHub.Clients.All.ReceiveNotification("TaxationPayment");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpPost("GetListForGrid")]
        public IActionResult GetListForGrid([FromBody] GridParameter parameters)
        {
            var model = Manager.GetListForGrid(parameters);
            return OkResult(new { parentDataSource = model });
        }

        [HttpGet("GetTaxationVettingDataForPayment/{TVMID:int}")]
        public async Task<IActionResult> GetTaxationVettingDataForPayment(int TVMID)
        {
            var vettinginfo = await Manager.TaxationVettingAndInvoiceInfo(TVMID);
            //var paymentDetails = await Manager.GetPaymentMethodDetails(IPaymentMasterID);
            var invoiceAttachments = VettingManager.GetAttachmentsInvoice(Convert.ToInt32(vettinginfo["InvoiceMasterID"]));
            return OkResult(new { Vettinginfo = vettinginfo, InvoiceAttachments = invoiceAttachments });
        }

        [HttpGet("Get/{TVPID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> Get(int TVPID, int ApprovalProcessID)
        {
            var vettinginfo = await Manager.GetTaxationVettingPayment(TVPID);
            TVPID = vettinginfo.IsNull() ? 0 : TVPID;
            ApprovalProcessID = vettinginfo.IsNull() ? 0 : ApprovalProcessID;

            var child = Manager.GetChildList(TVPID);
            var paymentDetails = await Manager.GetPaymentMethodDetails(TVPID);
            var invoiceAttachments = VettingManager.GetAttachmentsInvoice(vettinginfo.InvoiceMasterID);
            var attachments = Manager.GetAttachments(TVPID, "TaxationVettingPayment");
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            return OkResult(new { Vettinginfo = vettinginfo, ChildList = child, PaymentDetails = paymentDetails, InvoiceAttachments = invoiceAttachments, Attachments = attachments, Comments = comments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers });
        }


        [HttpGet("GetForReAssessment/{TVPID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetForReAssessment(int TVPID, int ApprovalProcessID)
        {
            var vettinginfo = await Manager.GetTaxationVettingPayment(TVPID);
            var child =  Manager.GetChildList(TVPID);
            var paymentDetails = await Manager.GetPaymentMethodDetails(TVPID);
            var attachments = Manager.GetAttachments(TVPID, "TaxationVettingPayment");

            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var approvalFeedback = Manager.ReportApprovalFeedback(TVPID);
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.TaxationVettingPayment, TVPID);
            return OkResult(new { Vettinginfo = vettinginfo, ChildList = child, PaymentDetails = paymentDetails, Attachments = attachments, Comments = comments, ForwardInfoComments = forwardInfoComments, ApprovalFeedback = approvalFeedback });
        }


        [HttpGet("GetTaxationPaymentDataReport/{TVPID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetTaxationPaymentDataReport(int TVPID, int ApprovalProcessID)
        {
            var master = await Manager.GetTaxationVettingPayment(TVPID);
            var child =  Manager.GetChildList(TVPID);
            var paymentDetails = await Manager.GetPaymentMethodDetails(TVPID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var approvalFeedback = Manager.ReportApprovalFeedback(TVPID);
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.TaxationVettingPayment, TVPID);
            return OkResult(new { Master = master, ChildList = child, PaymentDetails = paymentDetails, Comments = comments, ForwardInfoComments = forwardInfoComments, ApprovalFeedback = approvalFeedback });
        }
    }
}
