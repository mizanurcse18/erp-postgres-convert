using API.Core;
using API.Core.Hubs;
using API.Core.Interface;
using Core;
using Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SCM.Manager;
using SCM.Manager.Dto;
using SCM.Manager.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCM.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class SCCController : BaseController
    {
        private readonly ISCCManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public SCCController(ISCCManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        [HttpPost("SaveSCC")]
        public IActionResult SaveSCC([FromBody] SCCDto SCC)
        {
            var response = Manager.SaveChanges(SCC).Result;
            _notificationHub.Clients.All.ReceiveNotification("SCC");
            
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpPost("GetAll")]
        public async Task<IActionResult> GetAll([FromBody] GridParameter parameters)
        {
            var list = Manager.GetSCCList(parameters);
            return OkResult(new { parentDataSource = list });
        }

        [HttpPost("GetSCCListAll")]
        public async Task<IActionResult> GetSCCListAll([FromBody] GridParameter parameters)
        {
            var list = Manager.GetSCCListAll(parameters);
            return OkResult(new { parentDataSource = list });
        }

        [HttpPost("UpdateSCCMasterAfterReset")]
        public IActionResult UpdateSCCMasterAfterReset([FromBody] SCCMasterDto scc)
        {
            Manager.UpdateSCCMasterAfterReset(scc);
            return OkResult(true);
        }

        [HttpPost("GetAllSCCList")]
        public async Task<IActionResult> GetAllSCCList([FromBody] GridParameter parameters)
        {
            var list = Manager.GetAllSCCList(parameters);
            return OkResult(new { parentDataSource = list });
        }


        [HttpGet("Get/{SCCMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> Get(int SCCMID, int ApprovalProcessID)
        {
            var master = await Manager.GetSCCMaster(SCCMID);
            SCCMID = master.IsNullOrDbNull() || master.SCCMID.IsZero() ? 0 : SCCMID;
            ApprovalProcessID = SCCMID.IsNotZero() ? ApprovalProcessID : 0;
            
            var child = await Manager.GetSCCChild(SCCMID);
            var attachments = Manager.GetAttachments(SCCMID);

            var mapList = Manager.GetSCCApprovalPanelDefault(SCCMID).Result;
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            var approvalFeedback = Manager.ReportForSCCApprovalFeedback(SCCMID);
            var proposedAttachments = Manager.GetProposedAttachments(SCCMID);
            return OkResult(new { Master = master, ChildList = child, SCCApprovalPanelList = mapList, Comments = comments, Attachments = attachments, ProposedAttachments = proposedAttachments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ApprovalFeedback = approvalFeedback });
        }

        [HttpGet("GetSCCAll/{SCCMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetSCCAll(int SCCMID, int ApprovalProcessID)
        {
            var master = await Manager.GetSCCMasterAll(SCCMID);
            SCCMID = master.IsNullOrDbNull() || master.SCCMID.IsZero() ? 0 : SCCMID;
            ApprovalProcessID = SCCMID.IsNotZero() ? ApprovalProcessID : 0;

            var child = await Manager.GetSCCChild(SCCMID);
            var attachments = Manager.GetAttachments(SCCMID);

            var mapList = Manager.GetSCCApprovalPanelDefault(SCCMID).Result;
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            var approvalFeedback = Manager.ReportForSCCApprovalFeedback(SCCMID);
            return OkResult(new { Master = master, ChildList = child, SCCApprovalPanelList = mapList, Comments = comments, Attachments = attachments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ApprovalFeedback = approvalFeedback });
        }

        [HttpGet("GetSCCFromAllList/{SCCMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetSCCFromAllList(int SCCMID, int ApprovalProcessID)
        {
            var master = await Manager.GetSCCMasterFromAllList(SCCMID);
            var child = await Manager.GetSCCChild(SCCMID);
            var attachments = Manager.GetAttachments(SCCMID);

            var mapList = Manager.GetSCCApprovalPanelDefault(SCCMID).Result;
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            var approvalFeedback = Manager.ReportForSCCApprovalFeedback(SCCMID);
            return OkResult(new { Master = master, ChildList = child, SCCApprovalPanelList = mapList, Comments = comments, Attachments = attachments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ApprovalFeedback = approvalFeedback });
        }

        [HttpGet("GetSCCForReAssessment/{SCCMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetSCCForReAssessment(int SCCMID, int ApprovalProcessID)
        {
            var master = await Manager.GetSCCMaster(SCCMID);
            SCCMID = master.IsNullOrDbNull() || master.SCCMID.IsZero() ? 0 : SCCMID;
            ApprovalProcessID = SCCMID.IsNotZero() ? ApprovalProcessID : 0;

            //var child = await Manager.GetSCCChild(SCCMID);
            var child = await Manager.GetSCCChildForAllItem((int)master.POMasterID,SCCMID);
            var attachments = Manager.GetAttachments(SCCMID);

            var mapList = Manager.GetSCCApprovalPanelDefault(SCCMID).Result;
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            return OkResult(new { Master = master, ChildList = child, SCCApprovalPanelList = mapList, Comments = comments, Attachments = attachments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers });
        }
        [HttpGet("GetSCCByID/{SCCMID:int}")]
        public async Task<IActionResult> GetSCCByID(int SCCMID)
        {
            var master = await Manager.GetSCCMaster(SCCMID);
            var child = await Manager.GetSCCChild(SCCMID);
            var attachments = new List<Attachment>();//Manager.GetAttachments(SCCMID);
            var mapList = Manager.GetGRNApprovalPanelDefault(SCCMID);
            return OkResult(new { Master = master, ChildList = child, Attachments = attachments, GRNApprovalPanelList = mapList });
        }
        
    }
}