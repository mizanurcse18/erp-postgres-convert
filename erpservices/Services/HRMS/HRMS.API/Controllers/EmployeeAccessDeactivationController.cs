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
    public class EmployeeAccessDeactivationController : BaseController
    {
        private readonly IEmployeeAccessDeactivationManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;
        public EmployeeAccessDeactivationController(IEmployeeAccessDeactivationManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
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
        [HttpPost("GetDivClearencetListForGrid")]
        public async Task<IActionResult> GetDivClearencetListForGrid([FromBody] GridParameter parameters)
        {
            var model = await Manager.GetDivClearenceListForGrid(parameters);
            model.IsSubmittedFromPopup = parameters.IsSubmittedFromPopup;
            return OkResult(new { parentDataSource = model });
        }

        [HttpPost("SaveEmployeeAccessDeactivation")]
        public IActionResult SaveEmployeeAccessDeactivation([FromBody] EmployeeAccessDeactivationDto accessDeactivation)
        {
            var response = Manager.SaveChanges(accessDeactivation).Result;
            _notificationHub.Clients.All.ReceiveNotification("AccessDeactivation");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }
        [HttpPost("SaveDivisionClearenceEAD")]
        public IActionResult SaveDivisionClearenceEAD([FromBody] EmployeeAccessDeactivationDto EAD)
        {
            var response = Manager.SaveChangesDivisionClearence(EAD).Result;
            _notificationHub.Clients.All.ReceiveNotification("DivisionClearence");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpGet("GetAccessDeactivation/{EADID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetAccessDeactivation(int EADID, int ApprovalProcessID)
        {
            var ead = await Manager.GetAccessDeactivation(EADID, ApprovalProcessID);
            EADID = ead.IsNullOrDbNull() || ead.EADID.IsZero() ? 0 : EADID;
            ApprovalProcessID = EADID.IsNotZero() ? ApprovalProcessID : 0;
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var attachments = Manager.GetAttachments(EADID);
            var approvalFeedback = Manager.EmployeeApprovalMemberFeedbackForEAD(EADID, ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = ead.IsNullOrDbNull() || ead.EADID.IsZero() ? new List<Dictionary<string,object>> () : await Manager.GetAllEmployeesForAccessDeactivation();
            //var forwardingMembers = Manager.GetForwardingMemberList(EADID, (int)Util.ApprovalType.AccessDeactivation, (int)Util.ApprovalPanel.AccessDeactivation).Result;

            return OkResult(new { Master = ead, Comments = comments, Attachments = attachments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers });
        }


        [HttpGet("GetAccessDeactivationDivisionClearence/{EADID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetAccessDeactivationDivisionClearence(int EADID, int ApprovalProcessID)
        {

            var ead = await Manager.GetAccessDeactivationDivisionClearence(EADID, ApprovalProcessID);
            var eadComments = Manager.GetApprovalComment(ead.ADApprovalProcessID);
            var eadApprovalFeedback = Manager.ReportForEADApprovalFeedback(EADID);
            var eadForwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.AccessDeactivation, EADID);



            EADID = ead.IsNullOrDbNull() || ead.EADID.IsZero() ? 0 : EADID;
            ApprovalProcessID = EADID.IsNotZero() ? ApprovalProcessID : 0;
            var comments = Manager.GetApprovalCommentDivClearence(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            //var forwardingMembers = Manager.GetForwardingMemberList(EADID, (int)Util.ApprovalType.DivisionClearance, (int)Util.ApprovalPanel.DivisionClearance).Result;
            var forwardingMembers = await Manager.GetAllEmployeesForAccessDeactivation();
            return OkResult(new { Master = ead, Comments = comments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, EADComments = eadComments, ApprovalFeedback = eadApprovalFeedback, ForwardInfoComments = eadForwardInfoComments });
        }

        [HttpGet("GetAccessDeactivationForReAssessment/{EADID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetAccessDeactivationForReAssessment(int EADID, int ApprovalProcessID)
        {
            var master = await Manager.GetAccessDeactivation(EADID, ApprovalProcessID);
            var attachments = Manager.GetAttachments(EADID);
            return OkResult(new { Master = master, Attachments = attachments });
        }

        [HttpGet("GetAccessDeactivationReport/{EADID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetAccessDeactivationReport(int EADID, int ApprovalProcessID)
        {
            var ead = await Manager.GetAccessDeactivation(EADID, ApprovalProcessID);
            EADID = ead.IsNullOrDbNull() || ead.EADID.IsZero() ? 0 : EADID;
            ApprovalProcessID = EADID.IsNotZero() ? ApprovalProcessID : 0;
            var master = Manager.ReportForAccessDeactivationMaster(EADID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var attachments = Manager.ReportForAccessDeactivationAttachments(EADID, "EmployeeAccessDeactivation");
            var approvalFeedback = Manager.ReportForEADApprovalFeedback(EADID);
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.AccessDeactivation, EADID);
            return OkResult(new { Master = master, Comments = comments, Attachments = attachments, ApprovalFeedback = approvalFeedback, ForwardInfoComments = forwardInfoComments });
        }

        [HttpGet("GetEADDivisionClearenceReport/{EADID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetEADDivisionClearenceReport(int EADID, int ApprovalProcessID)
        {
            var ead = await Manager.GetAccessDeactivationDivisionClearence(EADID, ApprovalProcessID);

            var eadComments = Manager.GetApprovalComment(ead.ADApprovalProcessID);
            var eadApprovalFeedback = Manager.ReportForEADApprovalFeedback(EADID);
            var eadForwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.AccessDeactivation, EADID);


            EADID = ead.IsNullOrDbNull() || ead.EADID.IsZero() ? 0 : EADID;
            ApprovalProcessID = EADID.IsNotZero() ? ApprovalProcessID : 0;
            var master = Manager.ReportForAccessDeactivationMaster(EADID);
            var comments = Manager.GetApprovalCommentDivClearence(ApprovalProcessID);
            var approvalFeedback = Manager.ReportForDivClearenceApprovalFeedback(EADID);
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.DivisionClearance, EADID);
            return OkResult(new { Master = master, Comments = comments, ApprovalFeedback = approvalFeedback, ForwardInfoComments = forwardInfoComments, EADComments = eadComments, EADApprovalFeedback = eadApprovalFeedback, EADForwardInfoComments = eadForwardInfoComments });
        }


        [HttpGet("GetDivisionClearenceForReAssessment/{EADID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetDivisionClearenceForReAssessment(int EADID, int ApprovalProcessID)
        {
            var master = await Manager.GetAccessDeactivationDivisionClearence(EADID, ApprovalProcessID);
            var mapList = Manager.GetDivisionClearenceApprovalPanelDefault(EADID).Result;

            var comments = Manager.GetApprovalComment(master.ADApprovalProcessID);
            var approvalFeedback = Manager.ReportForEADApprovalFeedback(EADID);
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.AccessDeactivation, EADID);

            return OkResult(new { Master = master, DivisionClearenceApprovalPanelList = mapList, EADComments = comments, ApprovalFeedback = approvalFeedback, ForwardInfoComments = forwardInfoComments });
        }

        [HttpGet("RemoveAccessDeactivation/{EADID:int}/{ApprovalProcessID:int}")]
        public IActionResult RemoveAccessDeactivation(int EADID, int ApprovalProcessID)
        {
            Manager.RemoveAccessDeactivation(EADID, ApprovalProcessID);
            _notificationHub.Clients.All.ReceiveNotification("AccessDeactivation");
            return OkResult(EADID);
        }


        [HttpGet("LoadExistingPanelByEADID/{id:int}")]
        public async Task<IActionResult> LoadExistingPanelByEADID(int id)
        {
            var data = Manager.LoadExistingPanelByEADID(id);
            return OkResult(new { data });
        }


        #region Download
        [HttpGet("DownloadAccessDeactivation")]
        public async Task<ActionResult> DownloadAccessDeactivation()
        {
            var accessDeactivationList = await Manager.DownloadAccessDeactivation();
            return OkResult(accessDeactivationList);
        }

        #endregion
    }
}