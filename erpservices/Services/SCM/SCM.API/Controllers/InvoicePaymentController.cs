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
using SCM.Manager.Dto;
using SCM.Manager.Interfaces;

namespace SCM.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class InvoicePaymentController : BaseController
    {
        private readonly IInvoicePaymentManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public InvoicePaymentController(IInvoicePaymentManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        [HttpGet("GetInvoiceForPayment/{FromDate}/{ToDate}/{SupplierID?}/{InvoiceMasterID:int}/{IPaymentMasterID:int}/{PaymentTypeId:int}")]
        public IActionResult GetInvoiceForPayment(string FromDate, string ToDate, string? SupplierID, int InvoiceMasterID,int IPaymentMasterID,int PaymentTypeId)
        {
            DateTime fromDate = FromDate.Trim() == "default" ? DateTime.MinValue : DateTime.ParseExact(FromDate, "dd-MM-yyyy", null);
            DateTime toDate = ToDate.Trim() == "default" ? DateTime.MaxValue : DateTime.ParseExact(ToDate, "dd-MM-yyyy", null);
            var InvoiceList = Manager.GetFilteredInvoiceList(fromDate, toDate, SupplierID, InvoiceMasterID, IPaymentMasterID, PaymentTypeId).Result;
            return OkResult(new { InvoiceList  });
        }

        [HttpGet("GetInvoicePaymentDataForTaxationPayment/{SupplierID:int}/{InvoiceMasterID:int}/{IPaymentMasterID:int}/{TVPID:int}")]
        public IActionResult GetInvoicePaymentDataForTaxationPayment(int SupplierID, int InvoiceMasterID, int IPaymentMasterID, int TVPID)
        {
            
            var InvoiceList = Manager.GetFilteredInvoicePaymentList( SupplierID, InvoiceMasterID, IPaymentMasterID, TVPID).Result;
            return OkResult(new { InvoiceList });
        }

        [HttpPost("SaveChanges")]
        public IActionResult SaveChanges([FromBody] InvoicePaymentDto invoicePayment)
        {
            var response = Manager.SaveChanges(invoicePayment).Result;
            _notificationHub.Clients.All.ReceiveNotification("InvoicePayment");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpGet("GetAll/{filterData}")]
        public async Task<IActionResult> GetAll(string filterData)
        {
            var nfas = Manager.GetMasterList(filterData);
            return OkResult(nfas);
        }

        [HttpPost("GetListForGrid")]
        public IActionResult GetListForGrid([FromBody] GridParameter parameters)
        {
            var model = Manager.GetListForGrid(parameters);
            return OkResult(new { parentDataSource = model });
        }

        [HttpGet("Get/{IPaymentMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> Get(int IPaymentMasterID, int ApprovalProcessID)
        {
            var master = await Manager.GetMaster(IPaymentMasterID);
            var child = await Manager.GetChildList(IPaymentMasterID);
            var paymentDetails= await Manager.GetPaymentMethodDetails(IPaymentMasterID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            //if (paymentDetails.Count > 0)
            //{
            //    var attachments = Manager.GetAttachments(paymentDetails[].PaymentMethodID);
            //}
            
            return OkResult(new { Master = master, ChildList = child, PaymentDetails = paymentDetails, Comments = comments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers });
        }

        [HttpGet("GetInvoicePaymentDataReport/{IPaymentMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetInvoicePaymentDataReport(int IPaymentMasterID, int ApprovalProcessID)
        {
            var master = await Manager.GetMaster(IPaymentMasterID);
            var child = await Manager.GetChildList(IPaymentMasterID);
            var paymentDetails = await Manager.GetPaymentMethodDetails(IPaymentMasterID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var approvalFeedback = Manager.ReportApprovalFeedback(IPaymentMasterID);
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.InvoicePayment, IPaymentMasterID);
            return OkResult(new { Master = master, ChildList = child, PaymentDetails = paymentDetails, Comments = comments, ForwardInfoComments = forwardInfoComments, ApprovalFeedback = approvalFeedback });
        }

        [HttpGet("GetForReAssessment/{IPaymentMasterID:int}")]
        public async Task<IActionResult> GetForReAssessment(int IPaymentMasterID)
        {
            var master = await Manager.GetMaster(IPaymentMasterID);
            var child = await Manager.GetChildList(IPaymentMasterID);
            var paymentDetails = await Manager.GetPaymentMethodDetails(IPaymentMasterID);
            return OkResult(new { Master = master, ChildList = child, PaymentDetails= paymentDetails });
        }
    }
}
