using API.Core;
using API.Core.Hubs;
using API.Core.Interface;
using Core;
using Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SCM.Manager.Dto;
using SCM.Manager.Interfaces;
using System.Threading.Tasks;

namespace SCM.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class PurchaseRequisitionController : BaseController
    {
        private readonly IPurchaseRequisitionManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public PurchaseRequisitionController(IPurchaseRequisitionManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        [HttpPost("SavePurchaseRequisition")]
        public IActionResult SavePurchaseRequisition([FromBody] PurchaseRequisitionDto PR)
        {
            var response = Manager.SaveChanges(PR).Result;
            _notificationHub.Clients.All.ReceiveNotification("PR");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpGet("GetSuppliers/{param}")]
        public async Task<IActionResult> GetSuppliers(string param)
        {
            var list = await Manager.GetSuppliers(param);
            return OkResult(list);
        }
        [HttpGet("GetAllApproved")]
        public async Task<IActionResult> GetAllApproved()
        {
            var list = await Manager.GetAllApproved();
            return OkResult(list);
        }

        [HttpGet("GetPRByID/{PRMasterID:int}")]
        public async Task<IActionResult> GetPRByID(int PRMasterID)
        {
            var master = await Manager.GetPurchaseRequisitionMaster(PRMasterID, 0);
            var child = await Manager.GetPurchaseRequisitionChildForPO(PRMasterID);
            //var attachments = Manager.GetAttachments(PRMasterID);
            return OkResult(new { Master = master, ChildList = child, });
        }
        [HttpGet("GetAll/{filterData}")]
        public async Task<IActionResult> GetAll(string filterData)
        {
            var list = await Manager.GetPurchaseRequisitionList(filterData);
            return OkResult(list);
        }

        [HttpPost("GetListForGrid")]
        public async Task<IActionResult> GetListForGrid([FromBody] GridParameter parameters)
        {
            var model = await Manager.GetPRListForGrid(parameters);
            model.IsSubmittedFromPopup = parameters.IsSubmittedFromPopup;
            return OkResult(new { parentDataSource = model });
        }

        [HttpPost("GetApprovePRListForGrid")]
        public async Task<IActionResult> GetApprovePRListForGrid([FromBody] GridParameter parameters)
        {
            var model = await Manager.GetApprovePRListForGrid(parameters);
            model.IsSubmittedFromPopup = parameters.IsSubmittedFromPopup;
            return OkResult(new { parentDataSource = model });
        }

        [HttpGet("Get/{PRMasterID:int}/{ApprovalProcessID:int}/{IsSCM:int}")]
        public async Task<IActionResult> Get(int PRMasterID, int ApprovalProcessID, int IsSCM)
        {

            var master = await Manager.GetPurchaseRequisitionMaster(PRMasterID, IsSCM);
            PRMasterID = master.IsNullOrDbNull() || master.PRMasterID.IsZero() ? 0 : PRMasterID;
            ApprovalProcessID = PRMasterID.IsNotZero() ? ApprovalProcessID : 0;

            var child = await Manager.GetPurchaseRequisitionChild(PRMasterID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var attachments = Manager.GetAttachments(PRMasterID);
            var approvalFeedback = Manager.ReportForPRApprovalFeedback(PRMasterID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            var quotations = Manager.GetQuotations(PRMasterID);
            var assesments = Manager.GetAssesments(PRMasterID);
            var isAssessmentMember = Manager.GetIsAssessmentMember();
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.PR, PRMasterID);
            var budgetDetails = Manager.GetPurchaseRequisitionChildCostCenterBudget(PRMasterID).Result;
            return OkResult(new { Master = master, ChildList = child, Comments = comments, Attachments = attachments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, Quotations = quotations, Assesment = assesments, ForwardInfoComments = forwardInfoComments, BudgetDetails = budgetDetails, IsAssessmentMember = isAssessmentMember });
        }

        [HttpGet("GetPRFromPOReport/{PRMasterID:int}/{ApprovalProcessID:int}/{IsSCM:int}")]
        public async Task<IActionResult> GetPRFromPOReport(int PRMasterID, int ApprovalProcessID, int IsSCM)
        {
            return await GetPR(PRMasterID, ApprovalProcessID);
        }

        [HttpGet("GetApprovedPR/{PRMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetApprovedPR(int PRMasterID, int ApprovalProcessID)
        {
            return await GetPR(PRMasterID, ApprovalProcessID);
        }


        [HttpGet("GetPRFromPOHistory/{PRMasterID:int}/{ApprovalProcessID:int}/{IsSCM:int}")]
        public async Task<IActionResult> GetPRFromPOHistory(int PRMasterID, int ApprovalProcessID, int IsSCM)
        {
            return await GetPR(PRMasterID, ApprovalProcessID);
        }

        [HttpGet("GetPRForSCC/{PRMasterID:int}/{ApprovalProcessID:int}/{IsSCM:int}")]
        public async Task<IActionResult> GetPRForSCC(int PRMasterID, int ApprovalProcessID, int IsSCM)
        {
            return await GetPR(PRMasterID, ApprovalProcessID);
        }
        [HttpGet("GetPRForInvoiceList/{PRMasterID:int}/{ApprovalProcessID:int}/{IsSCM:int}")]
        public async Task<IActionResult> GetPRForInvoiceList(int PRMasterID, int ApprovalProcessID, int IsSCM)
        {
            return await GetPR(PRMasterID, ApprovalProcessID);
        }

        [HttpGet("GetPRForTaxationVetting/{PRMasterID:int}/{ApprovalProcessID:int}/{IsSCM:int}")]
        public async Task<IActionResult> GetPRForTaxationVetting(int PRMasterID, int ApprovalProcessID, int IsSCM)
        {
            return await GetPR(PRMasterID, ApprovalProcessID);
        }

        [HttpGet("GetPRForTaxationPayment/{PRMasterID:int}/{ApprovalProcessID:int}/{IsSCM:int}")]
        public async Task<IActionResult> GetPRForTaxationPayment(int PRMasterID, int ApprovalProcessID, int IsSCM)
        {
            return await GetPR(PRMasterID, ApprovalProcessID);
        }
        [HttpGet("GetPRForInvoicePaymentList/{PRMasterID:int}/{ApprovalProcessID:int}/{IsSCM:int}")]
        public async Task<IActionResult> GetPRForInvoicePaymentList(int PRMasterID, int ApprovalProcessID, int IsSCM)
        {
            return await GetPR(PRMasterID, ApprovalProcessID);
        }
        [HttpGet("GetPRFromPOVendorReport/{PRMasterID:int}/{ApprovalProcessID:int}/{IsSCM:int}")]
        public async Task<IActionResult> GetPRFromPOVendorReport(int PRMasterID, int ApprovalProcessID, int IsSCM)
        {
            return await GetPR(PRMasterID, ApprovalProcessID);
        }
        [HttpGet("GetPRForGRNList/{PRMasterID:int}/{ApprovalProcessID:int}/{IsSCM:int}")]
        public async Task<IActionResult> GetPRForGRNList(int PRMasterID, int ApprovalProcessID, int IsSCM)
        {
            return await GetPR(PRMasterID, ApprovalProcessID);
        }
        [HttpGet("GetPRForAllGRNList/{PRMasterID:int}/{ApprovalProcessID:int}/{IsSCM:int}")]
        public async Task<IActionResult> GetPRForAllGRNList(int PRMasterID, int ApprovalProcessID, int IsSCM)
        {
            return await GetPR(PRMasterID, ApprovalProcessID);
        }

        [HttpGet("GetPRForQCList/{PRMasterID:int}/{ApprovalProcessID:int}/{IsSCM:int}")]
        public async Task<IActionResult> GetPRForQCList(int PRMasterID, int ApprovalProcessID, int IsSCM)
        {
            return await GetPR(PRMasterID, ApprovalProcessID);
        }
        [HttpGet("GetPRForAllQCList/{PRMasterID:int}/{ApprovalProcessID:int}/{IsSCM:int}")]
        public async Task<IActionResult> GetPRForAllQCList(int PRMasterID, int ApprovalProcessID, int IsSCM)
        {
            return await GetPR(PRMasterID, ApprovalProcessID);
        }
        [HttpGet("GetPRForCreateQC/{PRMasterID:int}/{ApprovalProcessID:int}/{IsSCM:int}")]
        public async Task<IActionResult> GetPRForCreateQC(int PRMasterID, int ApprovalProcessID, int IsSCM)
        {
            return await GetPR(PRMasterID, ApprovalProcessID);
        }
        private async Task<IActionResult> GetPR(int PRMasterID, int ApprovalProcessID)
        {
            var master = await Manager.GetApprovedPurchaseRequisitionMaster(PRMasterID);

            var child = await Manager.GetPurchaseRequisitionChild(PRMasterID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var attachments = Manager.GetAttachments(PRMasterID);
            var approvalFeedback = Manager.ReportForPRApprovalFeedback(PRMasterID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            var quotations = Manager.GetQuotations(PRMasterID);
            var assesments = Manager.GetAssesments(PRMasterID);
            var isAssessmentMember = Manager.GetIsAssessmentMember();
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.PR, PRMasterID);
            var budgetDetails = Manager.GetPurchaseRequisitionChildCostCenterBudget(PRMasterID).Result;
            return OkResult(new { Master = master, ChildList = child, Comments = comments, Attachments = attachments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, Quotations = quotations, Assesment = assesments, ForwardInfoComments = forwardInfoComments, BudgetDetails = budgetDetails, IsAssessmentMember = isAssessmentMember });
        }




        [HttpGet("GetPurchaseRequisitionForReAssessment/{PRMasterID:int}/{IsSCM:int}")]
        public async Task<IActionResult> GetPurchaseRequisitionForReAssessment(int PRMasterID, int IsSCM)
        {
            var master = await Manager.GetPurchaseRequisitionMaster(PRMasterID, IsSCM);
            var child = await Manager.GetPurchaseRequisitionChild(PRMasterID);
            var attachments = Manager.GetAttachments(PRMasterID);
            return OkResult(new { Master = master, ChildList = child, Attachments = attachments });
        }
        [HttpGet("SetPRArhciveStatus/{PRMasterID:int}/{IsArchive:bool}")]
        public async Task<IActionResult> SetPRArhciveStatus(int PRMasterID, bool IsArchive)
        {
            await Manager.SaveArchiveStatus(PRMasterID, IsArchive);
            return OkResult(new { message = IsArchive == true ? "PR Archive Successfully" : "PR Unarchive Successfully", status = true });

        }
        [HttpPost("GetNFABalanceByPRID")]
        public async Task<ActionResult> GetNFABalanceByPRID([FromBody] PurchaseRequisitionDto param)
        {
            var list = await Manager.GetNFABalanceByPRID(param.PRMasterID, param.NFAID);
            return OkResult(list);
        }

        

    }
}