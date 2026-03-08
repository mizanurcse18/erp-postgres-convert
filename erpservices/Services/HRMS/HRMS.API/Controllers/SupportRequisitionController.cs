using API.Core;
using API.Core.Hubs;
using API.Core.Interface;
using Core;
using Core.AppContexts;
using Core.Extensions;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class SupportRequisitionController : BaseController
    {
        private readonly ISupportRequisitionManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;
        public SupportRequisitionController(ISupportRequisitionManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }
        [HttpPost("GetListForGrid")]
        public async Task<IActionResult> GetListForGrid([FromBody] GridParameter parameters)
        {
            var model = await Manager.GetListForGrid(parameters);
            model.IsSubmittedFromPopup = parameters.IsSubmittedFromPopup;
            return OkResult(new { parentDataSource = model });
        }

        [HttpPost("GetAllListForGrid")]
        public async Task<IActionResult> GetAllListForGrid([FromBody] GridParameter parameters)
        {
            var model = await Manager.GetAllListForGrid(parameters);
            model.IsSubmittedFromPopup = parameters.IsSubmittedFromPopup;
            return OkResult(new { parentDataSource = model });
        }
        [HttpPost("GetListForGridForEmp")]
        public async Task<IActionResult> GetListForGridForEmp([FromBody] GridParameter parameters)
        {
            var model = await Manager.GetListForGridForEmp(parameters);
            model.IsSubmittedFromPopup = parameters.IsSubmittedFromPopup;
            return OkResult(new { parentDataSource = model });
        }
        [HttpPost("SaveSupportRequisition")]
        public async Task<IActionResult> SaveSupportRequisition([FromBody] SupportRequisitionDto requestSupport)
        {
            var response =await Manager.SaveChanges(requestSupport);
            _notificationHub.Clients.All.ReceiveNotification("SupportRequisition");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpGet("GetSupportRequisition/{SRMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetSupportRequisition(int SRMID, int ApprovalProcessID)
        {
            var ead = await Manager.GetSupportRequisition(SRMID, ApprovalProcessID);
            SRMID = ead.IsNullOrDbNull() || ead.SRMID.IsZero() ? 0 : SRMID;
            ApprovalProcessID = SRMID.IsNotZero() ? ApprovalProcessID : 0;

            //var accessoriesItemDetails = Manager.GetAccessoriesItemDetails(SRMID).Result;
            var assetItemDetails = await Manager.GetAssetItemDetails(SRMID);
            var accessDetails = await Manager.GetAccessDetails(SRMID);

            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            //var attachments = Manager.GetAttachments(SRMID);
            var approvalFeedback = Manager.EmployeeApprovalMemberFeedbackForRSM(SRMID, ApprovalProcessID);
            var rejectedMembers = await Manager.GetRejectedMemeberList(ApprovalProcessID);
            var forwardingMembers = ead.IsNullOrDbNull() || ead.SRMID.IsZero() ? new List<Dictionary<string,object>> () : await Manager.GetAllEmployeesForSupportRequisition();

            return OkResult(new { Master = ead,
                //AccessoriesItem = accessoriesItemDetails,
                AssetItem = assetItemDetails,
                AccessRequestDetails = accessDetails, 
                Comments = comments //, Attachments = attachments
                , ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers });
        }

        [HttpGet("GetSupportRequisitionForReAssessment/{SRMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetSupportRequisitionForReAssessment(int SRMID, int ApprovalProcessID)
        {
            var master = await Manager.GetSupportRequisition(SRMID, ApprovalProcessID);
            //var accessoriesItemDetails = Manager.GetAccessoriesItemDetails(SRMID).Result;
            var assetItemDetails = await Manager.GetAssetItemDetails(SRMID);
            var accessDetails = await Manager.GetAccessDetails(SRMID);
            //var attachments = Manager.GetAttachments(SRMID);
            return OkResult(new { Master = master,
                //AccessoriesItem = accessoriesItemDetails,
                AssetItem = assetItemDetails,
                AccessDetails = accessDetails//, Attachments = attachments 
            });
        }

        [HttpGet("GetSupportRequestReport/{SRMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetSupportRequestReport(int SRMID, int ApprovalProcessID)
        {
            var ead = await Manager.GetSupportRequisition(SRMID, ApprovalProcessID);
            SRMID = ead.IsNullOrDbNull() || ead.SRMID.IsZero() ? 0 : SRMID;
            ApprovalProcessID = SRMID.IsNotZero() ? ApprovalProcessID : 0;
            var master = await Manager.GetSupportRequisition(SRMID, ApprovalProcessID);
            //var accessoriesItemDetails = Manager.GetAccessoriesItemDetails(SRMID).Result;
            var assetItemDetails = await Manager.GetAssetItemDetails(SRMID);
            var accessDetails = await Manager.GetAccessDetails(SRMID);

            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            //var attachments = Manager.ReportForSupportRequisitionAttachments(SRMID);
            var approvalFeedback = Manager.ReportForRSMApprovalFeedback(SRMID);
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.SupportRequisition, SRMID);
            return OkResult(new { Master = master,
                //AccessoriesItem = accessoriesItemDetails,
                AssetItem = assetItemDetails,
                AccessDetails = accessDetails, Comments = comments, ApprovalFeedback = approvalFeedback, ForwardInfoComments = forwardInfoComments });
        }

        [HttpGet("RemoveSupportRequisition/{SRMID:int}/{ApprovalProcessID:int}")]
        public IActionResult RemoveSupportRequisition(int SRMID, int ApprovalProcessID)
        {
            Manager.RemoveSupportRequisition(SRMID);
            _notificationHub.Clients.All.ReceiveNotification("SupportRequisition");
            return OkResult(SRMID);
        }

        [HttpGet("SettleRequisitionSupport/{SRMID:int}/{ApprovalProcessID:int}/{SettlementRemarks}")]
        public IActionResult SettleRequisitionSupport(int SRMID, int ApprovalProcessID, string SettlementRemarks)
        {
            var response = Manager.SettleSupportRequisition(SRMID, SettlementRemarks).Result;
            _notificationHub.Clients.All.ReceiveNotification("SupportRequisition");
            return OkResult(new { status = response.Item1, message = response.Item2 });

        }


        [HttpGet("LoadExistingPanelByRSMID/{id:int}")]
        public async Task<IActionResult> LoadExistingPanelByRSMID(int id)
        {
            var data = Manager.LoadExistingPanelByRSMID(id);
            return OkResult(new { data });
        }


        //#region Download
        //[HttpGet("DownloadAdminSupportRequest")]
        //public async Task<ActionResult> DownloadAdminSupportRequest()
        //{
        //    var requestSupportList = Manager.DownloadSupportRequisition();
        //    return OkResult(requestSupportList);
        //}

        //#endregion


        [HttpGet("GetAllSupportRequestListByWhereCondition")]
        public ActionResult GetAllSupportRequestListByWhereCondition(string WhereCondition, string FromDate, string ToDate)
        {
            var reqList = Manager.GetAllSupportRequestListByWhereCondition(WhereCondition,FromDate,ToDate);

            return OkResult(reqList.Result);
        }


        [HttpGet("GetAllSupportRequestList")]
        public ActionResult GetAllSupportRequestList(string WhereCondition, string FromDate, string ToDate)
        {
            var reqList = Manager.GetAllSupportRequestList(WhereCondition, FromDate, ToDate);

            return OkResult(reqList.Result);
        }
    }
}