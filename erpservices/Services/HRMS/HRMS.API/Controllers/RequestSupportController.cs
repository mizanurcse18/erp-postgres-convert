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
    public class RequestSupportController : BaseController
    {
        private readonly IRequestSupportManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;
        public RequestSupportController(IRequestSupportManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
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
        [HttpPost("GetListForGridForEmp")]
        public async Task<IActionResult> GetListForGridForEmp([FromBody] GridParameter parameters)
        {
            var model = await Manager.GetListForGridForEmp(parameters);
            model.IsSubmittedFromPopup = parameters.IsSubmittedFromPopup;
            return OkResult(new { parentDataSource = model });
        }
        [HttpPost("SaveRequestSupport")]
        public IActionResult SaveRequestSupport([FromBody] RequestSupportDto requestSupport)
        {
            var response = Manager.SaveChanges(requestSupport).Result;
            _notificationHub.Clients.All.ReceiveNotification("RequestSupport");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpGet("GetRequestSupport/{RSMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetRequestSupport(int RSMID, int ApprovalProcessID)
        {
            var ead = await Manager.GetRequestSupport(RSMID, ApprovalProcessID);
            RSMID = ead.IsNullOrDbNull() || ead.RSMID.IsZero() ? 0 : RSMID;
            ApprovalProcessID = RSMID.IsNotZero() ? ApprovalProcessID : 0;

            var itemDetails = await Manager.GetitemDetails(RSMID);
            
            var vehicleDetails = Manager.GetVehicleDetails(RSMID).Result;
            var facilitiesDetails = Manager.GetFacilitiesDetails(RSMID).Result;
            var renovationDetails = Manager.GetRenovationORMaintenanceDetails(RSMID).Result;

            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            //var attachments = Manager.GetAttachments(RSMID);
            var approvalFeedback = Manager.EmployeeApprovalMemberFeedbackForRSM(RSMID, ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = ead.IsNullOrDbNull() || ead.RSMID.IsZero() ? new List<Dictionary<string,object>> () : await Manager.GetAllEmployeesForRequestSupport();

            return OkResult(new { Master = ead, ItemDetails= itemDetails, VehicleDetails= vehicleDetails, FacilitiesDetails= facilitiesDetails,
                RenovationORMaintenanceDetails = renovationDetails, Comments = comments //, Attachments = attachments
                , ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers });
        }

        [HttpGet("GetRequestSupportForReAssessment/{RSMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetRequestSupportForReAssessment(int RSMID, int ApprovalProcessID)
        {
            var master = await Manager.GetRequestSupport(RSMID, ApprovalProcessID);
            var itemDetails = await Manager.GetitemDetails(RSMID);
            var vehicleDetails = await Manager.GetVehicleDetails(RSMID);
            var facilitiesDetails = await Manager.GetFacilitiesDetails(RSMID);
            var renovationDetails = await Manager.GetRenovationORMaintenanceDetails(RSMID);
            //var attachments = Manager.GetAttachments(RSMID);
            return OkResult(new { Master = master, ChildItem = itemDetails, VehicleDetails = vehicleDetails, FacilitiesDetails = facilitiesDetails,
                RenovationORMaintenanceDetails = renovationDetails//, Attachments = attachments 
            });
        }

        [HttpGet("GetSupportRequestReport/{RSMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetSupportRequestReport(int RSMID, int ApprovalProcessID)
        {
            var ead = await Manager.GetRequestSupport(RSMID, ApprovalProcessID);
            RSMID = ead.IsNullOrDbNull() || ead.RSMID.IsZero() ? 0 : RSMID;
            ApprovalProcessID = RSMID.IsNotZero() ? ApprovalProcessID : 0;
            var master = Manager.ReportForRequestSupportMaster(RSMID);

            var itemDetails = Manager.GetitemDetails(RSMID).Result;
            var vehicleDetails = Manager.GetVehicleDetails(RSMID).Result;
            var facilitiesDetails = Manager.GetFacilitiesDetails(RSMID).Result;
            var renovationDetails = Manager.GetRenovationORMaintenanceDetails(RSMID).Result;
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var attachments = Manager.ReportForRequestSupportAttachments(RSMID);
            var approvalFeedback = Manager.ReportForRSMApprovalFeedback(RSMID);
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.AdminSupportRequest, RSMID);
            return OkResult(new { Master = master, ChildItem = itemDetails, VehicleDetails = vehicleDetails, FacilitiesDetails = facilitiesDetails,
                RenovationORMaintenanceDetails = renovationDetails, Comments = comments, Attachments = attachments, ApprovalFeedback = approvalFeedback, ForwardInfoComments = forwardInfoComments });
        }

        [HttpGet("RemoveRequestSupport/{RSMID:int}/{ApprovalProcessID:int}")]
        public IActionResult RemoveRequestSupport(int RSMID, int ApprovalProcessID)
        {
            Manager.RemoveRequestSupport(RSMID);
            _notificationHub.Clients.All.ReceiveNotification("RequestSupport");
            return OkResult(RSMID);
        }

        [HttpGet("SettleRequestSupport/{RSMID:int}/{ApprovalProcessID:int}/{SettlementRemarks}")]
        public IActionResult SettleRequestSupport(int RSMID, int ApprovalProcessID, string SettlementRemarks)
        {
            var response = Manager.SettleRequestSupport(RSMID, SettlementRemarks).Result;
            _notificationHub.Clients.All.ReceiveNotification("RequestSupport");
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
        //    var requestSupportList = Manager.DownloadRequestSupport();
        //    return OkResult(requestSupportList);
        //}

        //#endregion


        [HttpGet("GetAllSupportRequestListByWhereCondition")]
        public ActionResult GetAllSupportRequestListByWhereCondition(string WhereCondition, string FromDate, string ToDate)
        {
            var employeeList = Manager.GetAllSupportRequestListByWhereCondition(WhereCondition,FromDate,ToDate);

            return OkResult(employeeList.Result);
        }

        [HttpPost("GetAllListForGrid")]
        public IActionResult GetAllListForGrid([FromBody] GridParameter parameters)
        {
            var model = Manager.GetAllListForGrid(parameters);
            model.IsSubmittedFromPopup = parameters.IsSubmittedFromPopup;
            return OkResult(new { parentDataSource = model });
        }
    }
}