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
    public class TaxationVettingController : BaseController
    {
        private readonly ITaxationVettingMasterManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public TaxationVettingController(ITaxationVettingMasterManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        [HttpPost("SaveChanges")]
        public IActionResult SaveChanges([FromBody] TaxationVettingMasterDto dto)
        {
            var response = Manager.SaveChanges(dto).Result;
            _notificationHub.Clients.All.ReceiveNotification("TaxationVetting");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpPost("GetListForGrid")]
        public IActionResult GetListForGrid([FromBody] GridParameter parameters)
        {
            var model = Manager.GetListForGrid(parameters);
            return OkResult(new { parentDataSource = model });
        }
        [HttpPost("GetAllApproved")]
        public IActionResult GetAllApproved([FromBody] GridParameter parameters)
        {
            var model = Manager.GetListForGridApproved(parameters);
            return OkResult(new { parentDataSource = model });
        }
        
        [HttpGet("GetsVDSListAsCombo")]
        public async Task<ActionResult> GetsVDSListAsCombo()
        {
            var list = await Manager.GetsVDSListAsCombo();
            return OkResult(list);
        }
        

        [HttpGet("GetsTDSListAsCombo/{id:int}")]
        public async Task<ActionResult> GetsTDSListAsCombo(int id)
        {
            var list = await Manager.GetsTDSListAsCombo(id);
            return OkResult(list);
        }
        [HttpGet("Get/{TVMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> Get(int TVMID, int ApprovalProcessID)
        {
            var master = await Manager.GetTaxationVettingMaster(TVMID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var attachments = Manager.GetAttachments(TVMID);
            var invoiceAttachments = Manager.GetAttachmentsInvoice(Convert.ToInt32(master["InvoiceMasterID"]));
            var approvalFeedback = Manager.TaxationVettingApprovalFeedback(TVMID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.TaxationVetting, TVMID);
            if (Convert.ToBoolean(master["IsAdvanceInvoice"]))
            {
                var child = await Manager.GetInvoiceChildListOfDict(Convert.ToInt32(master["InvoiceMasterID"]));
                return OkResult(new { Master = master, ChildList = child, Attachments = attachments, InvoiceAttachments = invoiceAttachments, Comments = comments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ForwardInfoComments = forwardInfoComments });
            }
            else
            {
                var child = Manager.MaterialReceiveMasterDetailsForReassessmentAndView(Convert.ToInt32(master["POMasterID"]), Convert.ToInt32(master["InvoiceMasterID"])).Result;
                var invoiceChild = Manager.GetInvoiceChildList(Convert.ToInt32(master["InvoiceMasterID"])).Result;
                var sccChildList = Manager.SccDetailsForReassessmentAndView(Convert.ToInt32(master["POMasterID"]), Convert.ToInt32(master["InvoiceMasterID"])).Result;
                return OkResult(new { Master = master, ChildList = child, SccChildList = sccChildList, Attachments = attachments, InvoiceAttachments = invoiceAttachments, InvoiceChild = invoiceChild, Comments = comments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ForwardInfoComments = forwardInfoComments });
            }
            //return OkResult(new { Master = master, ChildList = child, Comments = comments, Attachments = attachments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, Quotations = quotations, Assesment = assesments, ForwardInfoComments = forwardInfoComments, BudgetDetails = budgetDetails, IsAssessmentMember = isAssessmentMember });
            //return OkResult(true);
        }
        [HttpGet("Get/{TVMID:int}/{ApprovalProcessID:int}/{InvoiceMasterID:int}/{IsAdvanceInvoice:bool}/{POMasterID:int}")]
        public async Task<IActionResult> Get(int TVMID, int ApprovalProcessID, int InvoiceMasterID, bool IsAdvanceInvoice, int POMasterID)
        {
            var master = await Manager.GetTaxationVettingMaster(TVMID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var attachments = Manager.GetAttachments(TVMID);
            var invoiceAttachments = Manager.GetAttachmentsInvoice(InvoiceMasterID);
            var approvalFeedback = Manager.TaxationVettingApprovalFeedback(TVMID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.TaxationVetting, TVMID);
            if (IsAdvanceInvoice)
            {
                var child = await Manager.GetInvoiceChildListOfDict(InvoiceMasterID);
                return OkResult(new { Master = master, ChildList = child, Attachments = attachments, InvoiceAttachments = invoiceAttachments, Comments = comments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ForwardInfoComments = forwardInfoComments });
            }
            else
            {
                var child = Manager.MaterialReceiveMasterDetailsForReassessmentAndView(POMasterID, InvoiceMasterID).Result;
                var invoiceChild = Manager.GetInvoiceChildList(InvoiceMasterID).Result;
                var sccChildList = Manager.SccDetailsForReassessmentAndView(POMasterID, InvoiceMasterID).Result;
                return OkResult(new { Master = master, ChildList = child, SccChildList = sccChildList, Attachments = attachments, InvoiceAttachments = invoiceAttachments, InvoiceChild = invoiceChild, Comments = comments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ForwardInfoComments = forwardInfoComments });
            }
            //return OkResult(new { Master = master, ChildList = child, Comments = comments, Attachments = attachments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, Quotations = quotations, Assesment = assesments, ForwardInfoComments = forwardInfoComments, BudgetDetails = budgetDetails, IsAssessmentMember = isAssessmentMember });
            //return OkResult(true);
        }
        [HttpGet("GetInvoiceForTaxationVetting/{TVMID:int}/{InvoiceMasterID:int}/{IsAdvanceInvoice:bool}/{POMasterID:int}")]
        public async Task<IActionResult> GetTaxationVettingForRessessment(int TVMID, int InvoiceMasterID, bool IsAdvanceInvoice, int POMasterID)
        {
            var master = await Manager.GetTaxationVettingMaster(TVMID);
            var attachments = Manager.GetAttachments(TVMID);
            var invoiceAttachments = Manager.GetAttachmentsInvoice(InvoiceMasterID);
            if (IsAdvanceInvoice)
            {
                var child = await Manager.GetInvoiceChildListOfDict(InvoiceMasterID);
                return OkResult(new { Master = master, ChildList = child, Attachments = attachments, InvoiceAttachments = invoiceAttachments });
            }
            else
            {
                var child = Manager.MaterialReceiveMasterDetailsForReassessmentAndView(POMasterID, InvoiceMasterID).Result;
                var invoiceChild = Manager.GetInvoiceChildList(InvoiceMasterID).Result;
                var sccChildList = Manager.SccDetailsForReassessmentAndView(POMasterID, InvoiceMasterID).Result;
                return OkResult(new { Master = master, ChildList = child, Attachments = attachments, InvoiceAttachments = invoiceAttachments, SccChildList = sccChildList, InvoiceChild = invoiceChild });
            }
        }
    }
}
