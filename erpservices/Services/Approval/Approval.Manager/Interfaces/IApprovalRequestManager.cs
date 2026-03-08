using Approval.Manager.Dto;
using Core;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Approval.Manager.Interfaces
{
    public interface IApprovalRequestManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetApprovalRequestListDic();
        Task<IEnumerable<Dictionary<string, object>>> GetApprovalRequestListDicForNFA();

        Task<(bool, string)> BulkApproveOrRejectLeaveApplication(BulkSubmissionDto dto);
        Task<NotificationResponseDto> SubmitApproval(ApprovalSubmissionDto model);
        Task<(bool, string)> SubmitNFAApproval(ApprovalSubmissionDto model);
        Task<(bool, string)> SubmitExpenseClaimApproval(ApprovalSubmissionDto model);
        Task<(bool, string)> SubmitIOUClaimApproval(ApprovalSubmissionDto model);
        Task<(bool, string)> SubmitIOUExpenseClaimSattlementApproval(ApprovalSubmissionDto model);
        Task<(bool, string)> SubmitPurchaseRequisitionApproval(SCMApprovalSubmissionDto model);
        Task<(bool, string)> SubmitSCCApproval(SCCApprovalSubmissionDto model); 
        Task<(bool, string)> SubmitLeaveEncashApproval(LEApprovalSubmissionDto model); 
        Task<(bool, string, bool)> ApprovalSubmissionForDocumentUpload(ApprovalSubmissionDto model); 
        Task<(bool, string, bool)> ApprovalSubmissionForSupportRequest(SRApprovalSubmissionDto model); 
         Task<(bool, string)> SubmitMicroSiteApproval(ApprovalSubmissionDto model);
        Task<(bool, string)> SubmitEPA(ApprovalSubmissionDto model); 
         Task<(bool, string)> SubmitMaterialRequisitionApproval(SCMApprovalSubmissionForMRDto model);
        Task<(bool, string)> SubmitPurchaseOrderApproval(ApprovalSubmissionDto model);
        Task<(bool, string)> SubmitGRNApproval(ApprovalSubmissionDto model);
        Task<(bool, string)> SubmitQCApproval(ApprovalSubmissionDto model);
        Task<(bool, string)> SubmitDocumentApprovalApproval(ApprovalSubmissionDto model);
        Task<(bool, string)> ApprovalSubmissionForExitInterview(ExitInterviewApprovalSubmissionDto model); 
         Task<(bool, string)> SubmitTaxationVettingApproval(ApprovalSubmissionDto model);
        Task<(bool, string)> SubmitApprovalCommon(ApprovalSubmissionDto model);
        Task<(bool, string)> ResetPurchaseOrderApproval(ApprovalSubmissionDto model);
        Task<(bool, string)> ResetApproval(ApprovalSubmissionDto model);
        Task<(bool, string)> ApprovalSubmissionForAccessDeactivation(ApprovalSubmissionDto model);
        Task<(bool, string)> ApprovalSubmissionForDivisionClearence(EADApprovalSubmissionDto model);
        Task<(bool, string)> SubmitPettyCashExpenseApproval(ApprovalSubmissionDto model);
        Task<(bool, string)> SubmitPettyCashAdvanceApproval(ApprovalSubmissionDto model);
        Task<(bool, string)> SubmitPettyCashAdvanceApprovalResubmit(ApprovalSubmissionDto model);
        Task<(bool, string)> SubmitPettyCashReimburseApproval(ApprovalSubmissionDto model);
        Task<(bool, string)> SubmitPettyCashPaymentApproval(ApprovalSubmissionDto model);
        Task<(bool, string)> SubmitSupportRequisitionApproval(ApprovalSubmissionDto model);
        Task<(bool, string)> SubmitExternalAuditApproval(ApprovalSubmissionDto model);
    }
}
