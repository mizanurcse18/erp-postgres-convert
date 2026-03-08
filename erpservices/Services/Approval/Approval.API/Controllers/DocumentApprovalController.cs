using API.Core;
using API.Core.Hubs;
using API.Core.Interface;
using Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Approval.Manager.Dto;
using Approval.Manager.Interfaces;
using System.Threading.Tasks;
using Core.Extensions;
using Core.AppContexts;

namespace Approval.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class DocumentApprovalController : BaseController
    {
        private readonly IDocumentApprovalManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public DocumentApprovalController(IDocumentApprovalManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }
        [HttpPost("SaveDocumentApproval")]
        public IActionResult SaveDocumentApproval([FromBody] DocumentApprovalDto DocumentApproval)
        {
            var response = Manager.SaveChanges(DocumentApproval).Result;
            _notificationHub.Clients.All.ReceiveNotification("DocumentApproval");

            return OkResult(new { status = response.Item1, message = response.Item2 });
        }
        [HttpPost("SaveDocumentApprovalHR")]
        public IActionResult SaveDocumentApprovalHR([FromBody] DocumentApprovalDto DocumentApproval)
        {
            var response = Manager.SaveChangesForHR(DocumentApproval).Result;
            _notificationHub.Clients.All.ReceiveNotification("DocumentApproval");

            return OkResult(new { status = response.Item1, message = response.Item2 });
        }
        

        [HttpPost("GetAll")]
        public async Task<IActionResult> GetAll([FromBody] GridParameter parameters)
        {
            var list = Manager.GetDocumentApprovalList(parameters);
            return OkResult(new { parentDataSource = list });
        }
        [HttpGet("Get/{DAMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> Get(int DAMID, int ApprovalProcessID)
        {
            var master = await Manager.GetDocumentApprovalMaster(DAMID);
            DAMID = master.IsNullOrDbNull() || master.DAMID.IsZero() ? 0 : DAMID;
            ApprovalProcessID = DAMID.IsZero() ? 0 : ApprovalProcessID;

            var attachments = Manager.GetAttachments(DAMID);

            var mapList = Manager.GetDocumentApprovalApprovalPanelDefault(DAMID).Result;
            var comments = master.TemplateID > 0 ? Manager.GetApprovalCommentHR(ApprovalProcessID) : Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberListApprovalService(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberListApprovalService(ApprovalProcessID).Result;
            var approvalFeedback = master.TemplateID > 0 ? Manager.ReportForDocumentApprovalFeedbackHR(DAMID) : Manager.ReportForDocumentApprovalFeedback(DAMID);
            return OkResult(new { Master = master, DocumentApprovalApprovalPanelList = mapList, Comments = comments, Attachments = attachments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ApprovalFeedback = approvalFeedback });

        }


        //[HttpGet("GetForHR/{DAMID:int}/{ApprovalProcessID:int}")]
        //public async Task<IActionResult> GetForHR(int DAMID, int ApprovalProcessID)
        //{
        //    var master = await Manager.GetDocumentApprovalMasterHR(DAMID);
        //    DAMID = master.IsNullOrDbNull() || master.DAMID.IsZero() ? 0 : DAMID;
        //    ApprovalProcessID = DAMID.IsZero() ? 0 : ApprovalProcessID;

        //    var attachments = Manager.GetAttachments(DAMID);

        //    var mapList = Manager.GetDocumentApprovalApprovalPanelDefault(DAMID).Result;
        //    var comments = Manager.GetApprovalComment(ApprovalProcessID);
        //    var rejectedMembers = Manager.GetRejectedMemeberListApprovalService(ApprovalProcessID).Result;
        //    var forwardingMembers = Manager.GetForwardingMemberListApprovalService(ApprovalProcessID).Result;
        //    var approvalFeedback = Manager.ReportForDocumentApprovalFeedback(DAMID);
        //    return OkResult(new { Master = master, DocumentApprovalApprovalPanelList = mapList, Comments = comments, Attachments = attachments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ApprovalFeedback = approvalFeedback });

        //}

        [HttpGet("GetForReAssessment/{DAMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetForReAssessment(int DAMID, int ApprovalProcessID)
        {
            var master = await Manager.GetDocumentApprovalMaster(DAMID);

            DAMID = master.IsNullOrDbNull() || master.DAMID.IsZero() ? 0 : DAMID;
            ApprovalProcessID = DAMID.IsZero() ? 0 : ApprovalProcessID;

            var attachments = Manager.GetAttachments(DAMID);

            var mapList = Manager.GetDocumentApprovalApprovalPanelDefault(DAMID).Result;
            var comments = master.TemplateID > 0 ? Manager.GetApprovalCommentHR(ApprovalProcessID) : Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberListApprovalService(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberListApprovalService(ApprovalProcessID).Result;
            return OkResult(new { Master = master, DocumentApprovalApprovalPanelList = mapList, Comments = comments, Attachments = attachments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers });
        }


        [HttpGet("LoadExistingPanelByTemplateID/{id:int}")]
        public async Task<IActionResult> LoadExistingPanelByTemplateID(int id)
        {
            var data = Manager.LoadExistingPanelByTemplateID(id);
            return OkResult(new { data });
        }

        [HttpGet("GetDocumentApprovalByID/{DocumentApprovalMID:int}")]
        public async Task<IActionResult> GetDocumentApprovalByID(int DocumentApprovalMID)
        {
            var master = await Manager.GetDocumentApprovalMaster(DocumentApprovalMID);
            var attachments = Manager.GetAttachments(DocumentApprovalMID);
            var mapList = Manager.GetGRNApprovalPanelDefault(DocumentApprovalMID);
            return OkResult(new { Master = master, Attachments = attachments, GRNApprovalPanelList = mapList });
        }

        [HttpGet("GetNotificaiton")]
        [AllowAnonymous]
        public async Task<IActionResult> GetNotificaiton()
        {
            _notificationHub.Clients.All.ReceiveNotification("DocumentApproval");
            return OkResult(new { Master = "Data" });
        }
        [HttpGet("Delete/{id:int}")]
        public IActionResult Delete(int id)
        {
            Manager.DeleteDocumentApproval(id);
            return OkResult(new { Success = true });
        }

    }
}