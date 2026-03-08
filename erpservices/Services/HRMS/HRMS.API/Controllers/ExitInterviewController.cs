using API.Core;
using API.Core.Hubs;
using API.Core.Interface;
using Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using System.Threading.Tasks;
using Core.Extensions;
using Core.AppContexts;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class ExitInterviewController : BaseController
    {
        private readonly IExitInterviewManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public ExitInterviewController(IExitInterviewManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        [HttpPost("SaveExitInterview")]
        public IActionResult SaveExitInterview([FromBody] ExitInterviewDto ExitInterview)
        {
            var response = Manager.SaveChanges(ExitInterview).Result;
            _notificationHub.Clients.All.ReceiveNotification("ExitInterview");

            return OkResult(new { status = response.Item1, message = response.Item2 });
        }


        [HttpPost("GetAll")]
        public async Task<IActionResult> GetAll([FromBody] GridParameter parameters)
        {
            var list = Manager.GetExitInterviewList(parameters);
            return OkResult(new { parentDataSource = list });
        }
        [HttpGet("Get/{EEIID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> Get(int EEIID, int ApprovalProcessID)
        {
            var master = await Manager.GetExitInterviewMaster(EEIID);
            EEIID = master.IsNullOrDbNull() || master.EEIID.IsZero() ? 0 : EEIID;
            ApprovalProcessID = EEIID.IsZero() ? 0 : ApprovalProcessID;

            var attachments = Manager.GetAttachments(EEIID);

            var mapList = Manager.GetExitInterviewApprovalPanelDefault(EEIID).Result;
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberListApprovalService(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberListApprovalService(ApprovalProcessID).Result;
            var approvalFeedback = Manager.ReportForExitInterviewFeedback(EEIID);
            return OkResult(new { Master = master, ExitInterviewApprovalPanelList = mapList, Comments = comments, Attachments = attachments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ApprovalFeedback = approvalFeedback });

        }

        [HttpGet("GetForReAssessment/{EEIID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetForReAssessment(int EEIID, int ApprovalProcessID)
        {
            var master = await Manager.GetExitInterviewMaster(EEIID);

            EEIID = master.IsNullOrDbNull() || master.EEIID.IsZero() ? 0 : EEIID;
            ApprovalProcessID = EEIID.IsZero() ? 0 : ApprovalProcessID;

            var attachments = Manager.GetAttachments(EEIID);

            var mapList = Manager.GetExitInterviewApprovalPanelDefault(EEIID).Result;
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberListApprovalService(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberListApprovalService(ApprovalProcessID).Result;
            return OkResult(new { Master = master, ExitInterviewApprovalPanelList = mapList, Comments = comments, Attachments = attachments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers });
        }


        [HttpGet("LoadExistingPanelByTemplateID/{id:int}")]
        public async Task<IActionResult> LoadExistingPanelByTemplateID(int id)
        {
            var data = Manager.LoadExistingPanelByTemplateID(id);
            return OkResult(new { data });
        }

        [HttpGet("GetExitInterviewByID/{ExitInterviewMID:int}")]
        public async Task<IActionResult> GetExitInterviewByID(int ExitInterviewMID)
        {
            var master = await Manager.GetExitInterviewMaster(ExitInterviewMID);
            var attachments = Manager.GetAttachments(ExitInterviewMID);
            var mapList = Manager.GetGRNApprovalPanelDefault(ExitInterviewMID);
            return OkResult(new { Master = master, Attachments = attachments, GRNApprovalPanelList = mapList });
        }

        [HttpGet("GetNotificaiton")]
        [AllowAnonymous]
        public async Task<IActionResult> GetNotificaiton()
        {
            _notificationHub.Clients.All.ReceiveNotification("ExitInterview");
            return OkResult(new { Master = "Data" });
        }

        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await Manager.Delete(id);
            return OkResult(new { success = true });

        }

        [HttpGet("GetExitInterviewTemplate/{DATID:int}")]
        public async Task<IActionResult> Get(int DATID)
        {

            var res = await Manager.GetExitInterviewTemplate(DATID);

            return OkResult(res);
        }

    }
}