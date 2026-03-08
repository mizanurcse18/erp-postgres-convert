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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCM.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class QCController : BaseController
    {
        private readonly IQCManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public QCController(IQCManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        [HttpPost("SaveQC")]
        public IActionResult SaveQC([FromBody] QCDto QC)
        {
            var response = Manager.SaveChanges(QC).Result;
            _notificationHub.Clients.All.ReceiveNotification("QC");

            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpPost("GetAll")]
        public async Task<IActionResult> GetAll([FromBody] GridParameter parameters)
        {
            var list = Manager.GetQCList(parameters);
            return OkResult(new { parentDataSource = list });
        }
        [HttpPost("GetAllQCList")]
        public async Task<IActionResult> GetAllQCList([FromBody] GridParameter parameters)
        {
            var list = Manager.GetAllQCList(parameters);
            return OkResult(new { parentDataSource = list });
        }


        [HttpGet("Get/{QCMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> Get(int QCMID, int ApprovalProcessID)
        {
            var master = await Manager.GetQCMaster(QCMID);
            QCMID = master.IsNullOrDbNull() || master.QCMID.IsZero() ? 0 : QCMID;
            ApprovalProcessID = QCMID.IsNotZero() ? ApprovalProcessID : 0;

            var child = await Manager.GetQCChild(QCMID);
            var attachments = Manager.GetAttachments(QCMID);

            var mapList = Manager.GetQCApprovalPanelDefault(QCMID).Result;
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            var approvalFeedback = Manager.ReportForQCApprovalFeedback(QCMID);
            return OkResult(new { Master = master, ChildList = child, QCApprovalPanelList = mapList, Comments = comments, Attachments = attachments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ApprovalFeedback = approvalFeedback });
        }

        [HttpGet("GetQCFromAllList/{QCMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetQCFromAllList(int QCMID, int ApprovalProcessID)
        {
            var master = await Manager.GetQCMasterFromAllList(QCMID);
            var child = await Manager.GetQCChild(QCMID);
            var attachments = Manager.GetAttachments(QCMID);

            var mapList = Manager.GetQCApprovalPanelDefault(QCMID).Result;
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            var approvalFeedback = Manager.ReportForQCApprovalFeedback(QCMID);
            return OkResult(new { Master = master, ChildList = child, QCApprovalPanelList = mapList, Comments = comments, Attachments = attachments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ApprovalFeedback = approvalFeedback });
        }

        [HttpGet("GetQCFromPOList/{QCMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetQCFromPOList(int QCMID, int ApprovalProcessID)
        {
            return await GetQCFromAllList( QCMID, ApprovalProcessID);
        }
        [HttpGet("GetRTV/{QCMID:int}")]
        public async Task<IActionResult> GetRTV(int QCMID)
        {
            var master = await Manager.GetRTVMaster(QCMID);
            var child = await Manager.GetRTVChild(QCMID);
            var attachments = Manager.GetAttachments(QCMID);
            return OkResult(new { Master = master, ChildList = child, Attachments = attachments });
        }



        [HttpGet("GetQCForReAssessment/{QCMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetQCForReAssessment(int QCMID, int ApprovalProcessID)
        {
            var master = await Manager.GetQCMaster(QCMID);
            QCMID = master.IsNullOrDbNull() || master.QCMID.IsZero() ? 0 : QCMID;
            ApprovalProcessID = QCMID.IsNotZero() ? ApprovalProcessID : 0;

            var child = await Manager.GetQCChild(QCMID);
            var attachments = Manager.GetAttachments(QCMID);

            var mapList = await Manager.GetQCApprovalPanelDefault(QCMID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = await Manager.GetRejectedMemeberList(ApprovalProcessID);
            var forwardingMembers = await Manager.GetForwardingMemberList(ApprovalProcessID);
            return OkResult(new { Master = master, ChildList = child, QCApprovalPanelList = mapList, Comments = comments, Attachments = attachments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers });
        }
        [HttpGet("GetQCByID/{QCMID:int}")]
        public async Task<IActionResult> GetQCByID(int QCMID)
        {
            var master = await Manager.GetQCMaster(QCMID);
            var child = await Manager.GetQCChild(QCMID);
            var attachments = new List<Attachment>();//Manager.GetAttachments(QCMID);
            var mapList = Manager.GetGRNApprovalPanelDefault(QCMID);
            return OkResult(new { Master = master, ChildList = child, Attachments = attachments, GRNApprovalPanelList = mapList });
        }


        [HttpGet("GetQCForInvoiceList/{QCMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetQCForInvoiceList(int QCMID, int ApprovalProcessID)
        {
            var master = await Manager.GetQCMaster(QCMID);
            QCMID = master.IsNullOrDbNull() || master.QCMID.IsZero() ? 0 : QCMID;
            ApprovalProcessID = QCMID.IsNotZero() ? ApprovalProcessID : 0;

            var child = await Manager.GetQCChild(QCMID);
            var attachments = Manager.GetAttachments(QCMID);

            var mapList = await Manager.GetQCApprovalPanelDefault(QCMID);
            var comments = Manager.GetApprovalComment(master.ApprovalProcessID);
            var rejectedMembers = await Manager.GetRejectedMemeberList(master.ApprovalProcessID);
            var forwardingMembers = await Manager.GetForwardingMemberList(master.ApprovalProcessID);
            var approvalFeedback = Manager.ReportForQCApprovalFeedback(QCMID);
            return OkResult(new { Master = master, ChildList = child, QCApprovalPanelList = mapList, Comments = comments, Attachments = attachments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ApprovalFeedback = approvalFeedback });
        }

    }
}