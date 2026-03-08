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
using System;
using System.Threading.Tasks;

namespace SCM.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class PurchaseOrderController : BaseController
    {
        private readonly IPurchaseOrderManager Manager;
        private readonly IPurchaseRequisitionManager PRManager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public PurchaseOrderController(IPurchaseOrderManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub, IPurchaseRequisitionManager prManager)
        {
            Manager = manager;
            PRManager = prManager;
            _notificationHub = notificationHub;
        }

        [HttpPost("SavePurchaseOrder")]
        public IActionResult SavePurchaseOrder([FromBody] PurchaseOrderDto PO)
        {
            var response = Manager.SaveChanges(PO).Result;
            _notificationHub.Clients.All.ReceiveNotification("PO");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        //[HttpGet("GetsSupplierFromPOQuotation/{POMasterID:int}")]
        //public async Task<IActionResult> GetsSupplierFromPOQuotation(int POMasterID)
        //{
        //    var list = await Manager.GetsSupplierFromPOQuotation(POMasterID);
        //    return OkResult(list);
        //}
        //[HttpGet("GetAllApproved")]
        //public async Task<IActionResult> GetAllApproved()
        //{
        //    var list = await Manager.GetAllApproved();
        //    return OkResult(list);
        //}

        [HttpGet("GetPOByID/{POMasterID:int}")]
        public async Task<IActionResult> GetPOByID(int POMasterID)
        {
            var master = await Manager.GetPurchaseOrderMaster(POMasterID);
            var child = await Manager.GetPurchaseOrderChildForQC(POMasterID);
            var attachments = Manager.GetAttachments(POMasterID);
            var mapList = Manager.GetQCApprovalPanelDefault(POMasterID);
            return OkResult(new { Master = master, ChildList = child, Attachments = attachments, QCApprovalPanelList = mapList });
        }
        [HttpGet("GetPOByIDForSCC/{POMasterID:int}")]
        public async Task<IActionResult> GetPOByIDForSCC(int POMasterID)
        {
            var master = await Manager.GetPurchaseOrderMaster(POMasterID);
            var child = await Manager.GetPurchaseOrderChildForSCC(POMasterID);
            var attachments = Manager.GetAttachments(POMasterID);
            var mapList = Manager.GetSCCApprovalPanelDefault(POMasterID);
            return OkResult(new { Master = master, ChildList = child, Attachments = attachments, SCCApprovalPanelList = mapList });
        }


        [HttpGet("GetAll/{filterData}")]
        public async Task<IActionResult> GetAll(string filterData)
        {
            var list = await Manager.GetPurchaseOrderList(filterData);
            return OkResult(list);
        }

        [HttpPost("GetPOListForGrid")]
        public async Task<IActionResult> GetPOListForGrid([FromBody] GridParameter parameters)
        {
            var model = await Manager.GetPOListForGrid(parameters);
            return OkResult(new { parentDataSource = model });
        }

        [HttpGet("GetWithTerms/{POMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetWithTerms(int POMasterID, int ApprovalProcessID)
        {
            var master = await Manager.GetPurchaseOrderMaster(POMasterID);
            var child = await Manager.GetPurchaseOrderChild(POMasterID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var attachments = Manager.GetAttachments(POMasterID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(POMasterID, (int)Util.ApprovalType.PO, (int)Util.ApprovalPanel.NFAApprovalPanel).Result;
            var approvalFeedback = Manager.ReportForPOApprovalFeedback(POMasterID);
            var approvalFeedbackWithTerm = Manager.ReportForPOApprovalFeedbackWithTerm(POMasterID);
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.PO, POMasterID);
            var supplier = Manager.GetSupplierByID(POMasterID).Result;
            var companyInfo = Manager.GetCompanyInfo().Result;
            var terms = Manager.GetTerms().Result;
            return OkResult(new
            {
                Master = master,
                ChildList = child,
                Comments = comments,
                Attachments = attachments,
                ApprovalFeedback = approvalFeedback,
                RejectedMembers = rejectedMembers,
                ForwardingMembers = forwardingMembers,
                ForwardInfoComments = forwardInfoComments
            ,
                SupplierInfo = supplier,
                CompanyInfo = companyInfo,
                Terms = terms,
                ApprovalFeedbackWithTerm = approvalFeedbackWithTerm
            });
        }
        [HttpGet("Get/{POMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> Get(int POMasterID, int ApprovalProcessID)
        {
            var master = await Manager.GetPurchaseOrderMaster(POMasterID);

            var child = await Manager.GetPurchaseOrderChild(POMasterID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var attachments = Manager.GetAttachments(POMasterID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            var approvalFeedback = Manager.ReportForPOApprovalFeedback(POMasterID);
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.PO, POMasterID);
            return OkResult(new { Master = master, ChildList = child, Comments = comments, Attachments = attachments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ForwardInfoComments = forwardInfoComments });
        }
        [HttpGet("GetPOForSCC/{POMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetPOForSCC(int POMasterID, int ApprovalProcessID)
        {
            return await GetPOSCC(POMasterID, ApprovalProcessID);
        }

        [HttpGet("GetPOForRegularInvoiceSCC/{POMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetPOForRegularInvoiceSCC(int POMasterID, int ApprovalProcessID)
        {
            return await GetPOSCC(POMasterID, ApprovalProcessID);
        }

        private async Task<IActionResult> GetPOSCC(int POMasterID, int ApprovalProcessID)
        {
            var master = await Manager.GetPurchaseOrderMaster(POMasterID);

            var child = await Manager.GetPurchaseOrderChildSCC(POMasterID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var attachments = Manager.GetAttachments(POMasterID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            var approvalFeedback = Manager.ReportForPOApprovalFeedback(POMasterID);
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.PO, POMasterID);
            return OkResult(new { Master = master, ChildList = child, Comments = comments, Attachments = attachments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ForwardInfoComments = forwardInfoComments });

        }

        [HttpGet("GetPOFromApprovedPR/{POMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetPOFromApprovedPR(int POMasterID, int ApprovalProcessID)
        {
            return await GetPO(POMasterID, ApprovalProcessID);
        }
        [HttpGet("GetPOForInvoiceList/{POMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetPOForInvoiceList(int POMasterID, int ApprovalProcessID)
        {
            return await GetPO(POMasterID, ApprovalProcessID);
        }
        [HttpGet("GetPOForAdvanceInvoiceList/{POMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetPOForAdvanceInvoiceList(int POMasterID, int ApprovalProcessID)
        {
            return await GetPO(POMasterID, ApprovalProcessID);
        }
        [HttpGet("GetPOForRegularInvoiceList/{POMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetPOForRegularInvoiceList(int POMasterID, int ApprovalProcessID)
        {
            return await GetPO(POMasterID, ApprovalProcessID);
        }

        [HttpGet("GetPOForTaxationVetting/{POMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetPOForTaxationVetting(int POMasterID, int ApprovalProcessID)
        {
            return await GetPO(POMasterID, ApprovalProcessID);
        }
        [HttpGet("GetPOForInvoicePaymentList/{POMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetPOForInvoicePaymentList(int POMasterID, int ApprovalProcessID)
        {
            return await GetPO(POMasterID, ApprovalProcessID);
        }
        [HttpGet("GetPOForTaxationPayment/{POMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetPOForTaxationPayment(int POMasterID, int ApprovalProcessID)
        {
            return await GetPO(POMasterID, ApprovalProcessID);
        }
        [HttpGet("GetPOForGRNList/{POMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetPOForGRNList(int POMasterID, int ApprovalProcessID)
        {
            return await GetPO(POMasterID, ApprovalProcessID);
        }
        [HttpGet("GetPOForAllGRNList/{POMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetPOForAllGRNList(int POMasterID, int ApprovalProcessID)
        {
            return await GetPO(POMasterID, ApprovalProcessID);
        }

        [HttpGet("GetPOForQCList/{POMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetPOForQCList(int POMasterID, int ApprovalProcessID)
        {
            return await GetPO(POMasterID, ApprovalProcessID);
        }
        [HttpGet("GetPOForAllQCList/{POMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetPOForAllQCList(int POMasterID, int ApprovalProcessID)
        {
            return await GetPO(POMasterID, ApprovalProcessID);
        }
        [HttpGet("GetPOForCreateQC/{POMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetPOForCreateQC(int POMasterID, int ApprovalProcessID)
        {
            return await GetPO(POMasterID, ApprovalProcessID);
        }

        [HttpGet("GetPO/{POMasterID:int}/{ApprovalProcessID:int}")]
        private async Task<IActionResult> GetPO(int POMasterID, int ApprovalProcessID)
        {
            var master = await Manager.GetPurchaseOrderMasterFromApprovedPR(POMasterID);
            var child = await Manager.GetPurchaseOrderChild(POMasterID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var attachments = Manager.GetAttachments(POMasterID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            var approvalFeedback = Manager.ReportForPOApprovalFeedback(POMasterID);
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.PO, POMasterID);
            return OkResult(new { Master = master, ChildList = child, Comments = comments, Attachments = attachments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ForwardInfoComments = forwardInfoComments });
        }

        [HttpGet("GetPurchaseOrderForReAssessment/{POMasterID:int}")]
        public async Task<IActionResult> GetPurchaseOrderForReAssessment(int POMasterID)
        {
            var master = await Manager.GetPurchaseOrderMasterReassessment(POMasterID);
            POMasterID = master.IsNullOrDbNull() || master.POMasterID.IsZero() ? 0 : POMasterID;

            var child = await Manager.GetPurchaseOrderChild(POMasterID);
            var attachments = Manager.GetAttachments(POMasterID);
            var pr = PRManager.GetPurchaseRequisitionForReassessment((int)master.PRMasterID, POMasterID).Result;
            return OkResult(new { Master = master, ChildList = child, Attachments = attachments, PRData = pr });
        }

        [HttpGet("RemovePurchaseOrder/{POMasterID:int}/{ApprovalProcessID:int}")]
        public IActionResult RemovePurchaseOrder(int POMasterID, int ApprovalProcessID)
        {
            Manager.RemovePurchaseOrder(POMasterID, ApprovalProcessID);
            _notificationHub.Clients.All.ReceiveNotification("PO");
            return OkResult(POMasterID);
        }
        [HttpPost("ClosePurchaseOrder")]
        public IActionResult ClosePurchaseOrder([FromBody] PurchaseOrderDto po)
        {
            Manager.ClosePurchaseOrder(po);
            return OkResult(true);
        }
        //[HttpPost("GetAllApproved")]
        //public async Task<IActionResult> GetAllApproved(string FromDate, string ToDate)
        //{
        //    var list = await Manager.GetAllApproved(FromDate, ToDate);
        //    return OkResult(list);
        //}

        [HttpPost("GetAllApproved")]
        public async Task<IActionResult> GetAllApproved([FromBody] GridParameter parameters)
        {
            var list = Manager.GetAllApproved(parameters);
            return OkResult(new { parentDataSource = list }); ;
        }
        [HttpPost("GetAllApprovedForSCC")]
        public async Task<IActionResult> GetAllApprovedForSCC([FromBody] GridParameter parameters)
        {
            var list = Manager.GetAllApprovedForSCC(parameters);
            return OkResult(new { parentDataSource = list }); ;
        }


        [HttpPost("UpdatePurchaseOrderMasterAfterReset")]
        public IActionResult UpdatePurchaseOrderMasterAfterReset([FromBody] PurchaseOrderDto po)
        {
            Manager.UpdatePurchaseOrderMasterAfterReset(po);
            return OkResult(true);
        }

        [HttpPost("GetPOListForSCCGrid")]
        public async Task<IActionResult> GetPOListForSCCGrid([FromBody] GridParameter parameters)
        {
            var model = await Manager.GetPOListForSCCGrid(parameters);
            return OkResult(new { parentDataSource = model });
        }


    }
}