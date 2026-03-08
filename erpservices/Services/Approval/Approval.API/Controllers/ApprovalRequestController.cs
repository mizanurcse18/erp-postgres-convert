using API.Core;
using API.Core.Hubs;
using API.Core.Interface;
using Approval.Manager.Dto;
using Approval.Manager.Interfaces;
using Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Approval.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class ApprovalRequestController : BaseController
    {

        private readonly IApprovalRequestManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;
        public ApprovalRequestController(IApprovalRequestManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        [HttpGet("GetApprovalRequests")]
        public async Task<IActionResult> GetApprovalRequests()
        {
            var approvalrequests = await Manager.GetApprovalRequestListDic();
            return OkResult(approvalrequests);
        }
        [HttpGet("GetApprovalRequestsForNFA")]
        public async Task<IActionResult> GetApprovalRequestsForNFA()
        {
            var approvalrequests = await Manager.GetApprovalRequestListDicForNFA();
            return OkResult(approvalrequests);
        }

        #region Bulk Approve Rject


        [HttpPost("BulkApproveOrRejectLeaveApplication")]
        public async Task<IActionResult> BulkApproveOrRejectLeaveApplication([FromBody] BulkSubmissionDto dto)
        {
            var data =await Manager.BulkApproveOrRejectLeaveApplication(dto);
            return OkResult(new { status = data.Item1, message = data.Item2 });

        }
        #endregion

        [HttpPost("ApprovalSubmission")]
        public async Task<IActionResult> ApprovalSubmission(ApprovalSubmissionDto model)
        {
            //await Manager.SubmitApproval(model);
            //await _notificationHub.Clients.All.ReceiveNotification("LeaveApplication");
            //return OkResult(model);

            var data = await Manager.SubmitApproval(model);

            switch (model.APTypeID)
            {
                case (int)Util.ApprovalType.LeaveEncashmentApplication:
                    await _notificationHub.Clients.All.ReceiveNotification("LeaveEncashmentApplication");
                    break;
                case (int)Util.ApprovalType.LeaveApplication:
                    await _notificationHub.Clients.All.ReceiveNotificationUserWise("LeaveApplication", data.CurrentNotificaitonEmplyeeID);
                    break;
            }

            return OkResult(new { status = data.Success, message = data.Message });
        }

        [HttpPost("ApprovalSubmissionForNFA")]
        public async Task<IActionResult> ApprovalSubmissionForNFA(ApprovalSubmissionDto model)
        {
            var data = await Manager.SubmitNFAApproval(model);
            await _notificationHub.Clients.All.ReceiveNotification("NFA");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
        [HttpPost("ApprovalSubmissionForExpenseClaim")]
        public async Task<IActionResult> ApprovalSubmissionForExpenseClaim(ApprovalSubmissionDto model)
        {
            var data = await Manager.SubmitExpenseClaimApproval(model);
            await _notificationHub.Clients.All.ReceiveNotification("ExpenseClaim");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
        [HttpPost("ApprovalSubmissionForIOUClaim")]
        public async Task<IActionResult> ApprovalSubmissionForIOUClaim(ApprovalSubmissionDto model)
        {
            var data = await Manager.SubmitIOUClaimApproval(model);
            await _notificationHub.Clients.All.ReceiveNotification("IOUClaim");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
        [HttpPost("ApprovalSubmissionForIOUExpenseSattlement")]
        public async Task<IActionResult> ApprovalSubmissionForIOUExpenseSattlement(ApprovalSubmissionDto model)
        {
            var data = await Manager.SubmitIOUExpenseClaimSattlementApproval(model);
            await _notificationHub.Clients.All.ReceiveNotification("IOUExpenseSattlement");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
        [HttpPost("ApprovalSubmissionForPurchaseRequisition")]
        public async Task<IActionResult> ApprovalSubmissionForPurchaseRequisition(SCMApprovalSubmissionDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(model);
            }
            var data = await Manager.SubmitPurchaseRequisitionApproval(model);
            await _notificationHub.Clients.All.ReceiveNotification("PR");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }

        [HttpPost("GetApprovalRequestsForMicroSite")]
        public async Task<IActionResult> GetApprovalRequestsForMicroSite(ApprovalSubmissionDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(model);
            }
            var data = await Manager.SubmitMicroSiteApproval(model);
            await _notificationHub.Clients.All.ReceiveNotification("MicroSite");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
        [HttpPost("ApprovalSubmissionForEmployeeProfileApproval")]
        public async Task<IActionResult> ApprovalSubmissionForEmployeeProfileApproval(ApprovalSubmissionDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(model);
            }
            var data = await Manager.SubmitEPA(model);
            await _notificationHub.Clients.All.ReceiveNotification("Person");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }

        [HttpPost("ApprovalSubmissionForMaterialRequisition")]
        public async Task<IActionResult> ApprovalSubmissionForMaterialRequisition(SCMApprovalSubmissionForMRDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(model);
            }
            var data = await Manager.SubmitMaterialRequisitionApproval(model);
            await _notificationHub.Clients.All.ReceiveNotification("MR");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
        [HttpPost("ApprovalSubmissionForPurchaseOrder")]
        public async Task<IActionResult> ApprovalSubmissionForPurchaseOrder(ApprovalSubmissionDto model)
        {
            var data = await Manager.SubmitPurchaseOrderApproval(model);
            await _notificationHub.Clients.All.ReceiveNotification("PO");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
        [HttpPost("ApprovalSubmissionForGRN")]
        public async Task<IActionResult> ApprovalSubmissionForGRN(ApprovalSubmissionDto model)
        {
            var data = await Manager.SubmitGRNApproval(model);
            await _notificationHub.Clients.All.ReceiveNotification("GRN");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
        [HttpPost("ApprovalSubmissionForQC")]
        public async Task<IActionResult> ApprovalSubmissionForQC(ApprovalSubmissionDto model)
        {
            var data = await Manager.SubmitQCApproval(model);
            await _notificationHub.Clients.All.ReceiveNotification("QC");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
        [HttpPost("ApprovalSubmissionForDocumentApproval")]
        public async Task<IActionResult> ApprovalSubmissionForDocumentApproval(ApprovalSubmissionDto model)
        {
            var data = await Manager.SubmitDocumentApprovalApproval(model);
            await _notificationHub.Clients.All.ReceiveNotification("DocumentApproval");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
        [HttpPost("ApprovalSubmissionForExitInterview")]
        public async Task<IActionResult> ApprovalSubmissionForExitInterview(ExitInterviewApprovalSubmissionDto model)
        {
            var data = await Manager.ApprovalSubmissionForExitInterview(model);
            await _notificationHub.Clients.All.ReceiveNotification("ExitInterview");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
        [HttpPost("ApprovalSubmissionForInvoicePayment")]
        public async Task<IActionResult> ApprovalSubmissionForInvoicePayment(ApprovalSubmissionDto model)
        {
            var data = await Manager.SubmitApprovalCommon(model);
            await _notificationHub.Clients.All.ReceiveNotification("InvoicePayment");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
        [HttpPost("ApprovalSubmissionForInvoice")]
        public async Task<IActionResult> ApprovalSubmissionForInvoice(ApprovalSubmissionDto model)
        {
            var data = await Manager.SubmitApprovalCommon(model);
            await _notificationHub.Clients.All.ReceiveNotification("Invoice");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
        [HttpPost("ApprovalSubmissionForTaxationVetting")]
        public async Task<IActionResult> ApprovalSubmissionForTaxationVetting(ApprovalSubmissionDto model)
        {
            var data = await Manager.SubmitTaxationVettingApproval(model);
            await _notificationHub.Clients.All.ReceiveNotification("TaxationVetting");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }

        [HttpPost("ResetPurchaseOrderApproval")]
        public IActionResult ResetPurchaseOrderApproval([FromBody] ApprovalSubmissionDto app)
        {
            var response = Manager.ResetPurchaseOrderApproval(app).Result;
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpPost("ResetApproval")]
        public IActionResult ResetApproval([FromBody] ApprovalSubmissionDto app)
        {
            var response = Manager.ResetApproval(app).Result;
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpPost("ApprovalSubmissionForTaxationPayment")]
        public async Task<IActionResult> ApprovalSubmissionForTaxationPayment(ApprovalSubmissionDto model)
        {
            var data = await Manager.SubmitApprovalCommon(model);
            await _notificationHub.Clients.All.ReceiveNotification("TaxationPayment");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
        [HttpPost("ApprovalSubmissionForAccessDeactivation")]
        public async Task<IActionResult> ApprovalSubmissionForAccessDeactivation(ApprovalSubmissionDto model)
        {
            var data = await Manager.ApprovalSubmissionForAccessDeactivation(model);
            await _notificationHub.Clients.All.ReceiveNotification("AccessDeactivation");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
        [HttpPost("ApprovalSubmissionForDivisionClearence")]
        public async Task<IActionResult> ApprovalSubmissionForDivisionClearence(EADApprovalSubmissionDto model)
        {
            var data = await Manager.ApprovalSubmissionForDivisionClearence(model);
            await _notificationHub.Clients.All.ReceiveNotification("DivisionClearence");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
        [HttpPost("ApprovalSubmissionForSCC")]
        public async Task<IActionResult> ApprovalSubmissionForSCC(SCCApprovalSubmissionDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(model);
            }
            var data = await Manager.SubmitSCCApproval(model);
            await _notificationHub.Clients.All.ReceiveNotification("SCC");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
        [HttpPost("ApprovalSubmissionForLeaveEncashment")]
        public async Task<IActionResult> ApprovalSubmissionForLeaveEncashment(LEApprovalSubmissionDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(model);
            }
            var data = await Manager.SubmitLeaveEncashApproval(model);
            await _notificationHub.Clients.All.ReceiveNotification("LeaveEncashmentApplication");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }

        [HttpPost("ApprovalSubmissionForDocumentUpload")]
        public async Task<IActionResult> ApprovalSubmissionForDocumentUpload(ApprovalSubmissionDto model)
        {
            var data = await Manager.ApprovalSubmissionForDocumentUpload(model);
            await _notificationHub.Clients.All.ReceiveNotification("DocumentUpload");
            return OkResult(new { status = data.Item1, message = data.Item2, IsLastApproval = data.Item3});
        }
        [HttpPost("ApprovalSubmissionForSupportRequest")]
        public async Task<IActionResult> ApprovalSubmissionForSupportRequest(SRApprovalSubmissionDto model)
        {
            var data = await Manager.ApprovalSubmissionForSupportRequest(model);
            await _notificationHub.Clients.All.ReceiveNotification("SupportRequest");
            return OkResult(new { status = data.Item1, message = data.Item2, IsLastApproval = data.Item3});
        }

        [HttpPost("ApprovalSubmissionForPettyCashExpense")]
        public async Task<IActionResult> ApprovalSubmissionForPettyCashExpense(ApprovalSubmissionDto model)
        {
            var data = await Manager.SubmitPettyCashExpenseApproval(model);
            await _notificationHub.Clients.All.ReceiveNotification("PettyCashExpense");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
        [HttpPost("ApprovalSubmissionForPettyCashAdvance")]
        public async Task<IActionResult> ApprovalSubmissionForPettyCashAdvance(ApprovalSubmissionDto model)
        {
            var data = await Manager.SubmitPettyCashAdvanceApproval(model);
            await _notificationHub.Clients.All.ReceiveNotification("PettyCashAdvance");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
        [HttpPost("ApprovalSubmissionForPettyCashAdvanceResubmit")]
        public async Task<IActionResult> ApprovalSubmissionForPettyCashAdvanceResubmit(ApprovalSubmissionDto model)
        {
            var data = await Manager.SubmitPettyCashAdvanceApprovalResubmit(model);
            await _notificationHub.Clients.All.ReceiveNotification("ResubmitAdvanceClaim");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
        [HttpPost("ApprovalSubmissionForPettyCashReimburse")]
        public async Task<IActionResult> ApprovalSubmissionForPettyCashReimburse(ApprovalSubmissionDto model)
        {
            var data = await Manager.SubmitPettyCashReimburseApproval(model);
            await _notificationHub.Clients.All.ReceiveNotification("PettyCashReimburse");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }

        [HttpPost("ApprovalSubmissionForPettyCashPayment")]
        public async Task<IActionResult> ApprovalSubmissionForPettyCashPayment(ApprovalSubmissionDto model)
        {
            var data = await Manager.SubmitPettyCashPaymentApproval(model);
            await _notificationHub.Clients.All.ReceiveNotification("PettyCashPayment");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
         [HttpPost("ApprovalSubmissionForSupportRequisition")]
        public async Task<IActionResult> ApprovalSubmissionForSupportRequisition(ApprovalSubmissionDto model)
        {
            var data = await Manager.SubmitSupportRequisitionApproval(model);
            await _notificationHub.Clients.All.ReceiveNotification("SupportRequisition");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }

        [HttpPost("ApprovalSubmissionForExternalAudit")]
        public async Task<IActionResult> ApprovalSubmissionForExternalAudit(ApprovalSubmissionDto model)
        {
            var data = await Manager.SubmitExternalAuditApproval(model);
            await _notificationHub.Clients.All.ReceiveNotification("ExternalAudit");
            return OkResult(new { status = data.Item1, message = data.Item2 });
        }
    }
}
