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
    public class MaterialReceiveController : BaseController
    {
        private readonly IMaterialReceiveManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public MaterialReceiveController(IMaterialReceiveManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        [HttpPost("SaveMaterialReceive")]
        public IActionResult SaveMaterialReceive([FromBody] MaterialReceiveDto MR)
        {
            var response = Manager.SaveChanges(MR).Result;
            _notificationHub.Clients.All.ReceiveNotification("GRN");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }


        [HttpPost("GetAll")]
        public async Task<IActionResult> GetAll([FromBody] GridParameter parameters)
        {
            var list = Manager.GetMaterialReceiveList(parameters);
            return OkResult(new { parentDataSource = list });
        }
        [HttpPost("GetAllGRN")]
        public async Task<IActionResult> GetAllGRN([FromBody] GridParameter parameters)
        {
            var list = Manager.GetAllGRNList(parameters);
            return OkResult(new { parentDataSource = list });
        }


        [HttpGet("Get/{MRID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> Get(int MRID, int ApprovalProcessID)
        {
            var master = await Manager.GetMaterialReceiveMaster(MRID);
            MRID = master.IsNullOrDbNull() || master.MRID.IsZero() ? 0 : MRID;
            ApprovalProcessID = MRID.IsNotZero() ? ApprovalProcessID : 0;

            var child = await Manager.GetMaterialReceiveChild(MRID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var attachments = Manager.GetAttachments(MRID);
            var approvalFeedback = Manager.ReportForGRNApprovalFeedback(MRID);
            var rejectedMembers = await Manager.GetRejectedMemeberList(ApprovalProcessID);
            var forwardingMembers = await Manager.GetForwardingMemberList(ApprovalProcessID);
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.GRN, MRID);
            return OkResult(new { Master = master, ChildList = child, Comments = comments, Attachments = attachments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ForwardInfoComments = forwardInfoComments });

        }

        [HttpGet("GetGRNFromAllList/{MRID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetGRNFromAllList(int MRID, int ApprovalProcessID)
        {
            var master = await Manager.GetMaterialReceiveMasterFromAllList(MRID);
            var child = await Manager.GetMaterialReceiveChild(MRID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var attachments = Manager.GetAttachments(MRID);
            var approvalFeedback = Manager.ReportForGRNApprovalFeedback(MRID);
            var rejectedMembers = await Manager.GetRejectedMemeberList(ApprovalProcessID);
            var forwardingMembers = await Manager.GetForwardingMemberList(ApprovalProcessID);
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.GRN, MRID);
            return OkResult(new { Master = master, ChildList = child, Comments = comments, Attachments = attachments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ForwardInfoComments = forwardInfoComments });

        }


        [HttpPost("GetAllForService")]
        public async Task<IActionResult> GetAllForService([FromBody] GridParameter parameters)
        {
            var model = await Manager.GetMaterialReceiveListForService(parameters);
            return OkResult(new { parentDataSource = model });
        }
        //[HttpGet("GetAllForService/{filterData}")]
        //public async Task<IActionResult> GetAllForService(string filterData)
        //{
        //    var list = await Manager.GetMaterialReceiveListForService(filterData);
        //    return OkResult(list);
        //}
        [HttpGet("GetForService/{MRID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetForService(int MRID, int ApprovalProcessID)
        {
            var master = await Manager.GetMaterialReceiveMasterForService(MRID);
            var child = await Manager.GetMaterialReceiveChildForService(MRID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = await Manager.GetRejectedMemeberList(ApprovalProcessID);
            var forwardingMembers = await Manager.GetForwardingMemberList(ApprovalProcessID);
            var approvalFeedback = Manager.ReportForGRNApprovalFeedback(MRID);
            return OkResult(new { Master = master, ChildList = child, Comments = comments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers });
        }

        [HttpGet("GetMaterialReceiveForReAssessment/{MRID:int}/{QCMID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetMaterialReceiveForReAssessment(int MRID, int QCMID, int ApprovalProcessID)
        {
            var master = await Manager.GetMaterialReceiveMaster(MRID);
            var child = await Manager.GetMaterialReceiveChild(MRID);
            var mapList = Manager.GetGRNApprovalPanelDefault(MRID).Result;
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var attachments = Manager.GetAttachments(MRID);
            var rejectedMembers = await Manager.GetRejectedMemeberList(ApprovalProcessID);
            var forwardingMembers = await Manager.GetForwardingMemberList(ApprovalProcessID);
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.GRN, MRID);

            return OkResult(new { Master = master, ChildList = child, GRNApprovalPanelList = mapList, Comments = comments, Attachments = attachments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ForwardInfoComments = forwardInfoComments });
        }

        [HttpGet("GetGRNForInvoiceList/{MRID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetGRNForInvoiceList(int MRID, int ApprovalProcessID)
        {
            var master = await Manager.GetMaterialReceiveMaster(MRID);
            MRID = master.IsNullOrDbNull() || master.MRID.IsZero() ? 0 : MRID;
            ApprovalProcessID = MRID.IsNotZero() ? ApprovalProcessID : 0;

            var child = await Manager.GetMaterialReceiveChild(MRID);
            var comments = Manager.GetApprovalComment(master.ApprovalProcessID);
            var attachments = Manager.GetAttachments(MRID);
            var approvalFeedback = Manager.ReportForGRNApprovalFeedback(MRID);
            var rejectedMembers = await Manager.GetRejectedMemeberList(master.ApprovalProcessID);
            var forwardingMembers = await Manager.GetForwardingMemberList(master.ApprovalProcessID);
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.GRN, MRID);
            return OkResult(new { Master = master, ChildList = child, Comments = comments, Attachments = attachments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ForwardInfoComments = forwardInfoComments });
        }

    }
}