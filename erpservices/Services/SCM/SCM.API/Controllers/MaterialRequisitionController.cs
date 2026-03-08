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
    public class MaterialRequisitionController : BaseController
    {
        private readonly IMaterialRequisitionManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public MaterialRequisitionController(IMaterialRequisitionManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        [HttpPost("SaveMaterialRequisition")]
        public IActionResult SaveMaterialRequisition([FromBody] MaterialRequisitionDto MR)
        {
            var response = Manager.SaveChanges(MR).Result;
            _notificationHub.Clients.All.ReceiveNotification("MR");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpGet("GetsSupplierFromMRQuotation/{MRMasterID:int}")]
        public async Task<IActionResult> GetsSupplierFromMRQuotation(int MRMasterID)
        {
            var list = await Manager.GetsSupplierFromMRQuotation(MRMasterID);
            return OkResult(list);
        }
        [HttpGet("GetAllApproved")]
        public async Task<IActionResult> GetAllApproved()
        {
            var list = await Manager.GetAllApproved();
            return OkResult(list);
        }

        [HttpGet("GetMRByID/{MRMasterID:int}")]
        public async Task<IActionResult> GetMRByID(int MRMasterID)
        {
            var master = await Manager.GetMaterialRequisitionMaster(MRMasterID);
            MRMasterID = master.IsNullOrDbNull() || master.MRMasterID.IsZero() ? 0 : MRMasterID;
            var child = await Manager.GetMaterialRequisitionChildForPO(MRMasterID);
            return OkResult(new { Master = master, ChildList = child, });
        }
        [HttpGet("GetAll/{filterData}")]
        public async Task<IActionResult> GetAll(string filterData)
        {
            var list = await Manager.GetMaterialRequisitionList(filterData);
            return OkResult(list);
        }

        [HttpPost("GetListForGrid")]
        public async Task<IActionResult> GetListForGrid([FromBody] GridParameter parameters)
        {
            var model = await Manager.GetMRListForGrid(parameters);
            model.IsSubmittedFromPopup = parameters.IsSubmittedFromPopup;
            return OkResult(new { parentDataSource = model });
        }

        [HttpPost("GetApproveMRListForGrid")]
        public async Task<IActionResult> GetApproveMRListForGrid([FromBody] GridParameter parameters)
        {
            var model = await Manager.GetApproveMRListForGrid(parameters);
            model.IsSubmittedFromPopup = parameters.IsSubmittedFromPopup;
            return OkResult(new { parentDataSource = model });
        }

        [HttpGet("Get/{MRMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> Get(int MRMasterID, int ApprovalProcessID)
        {
            var master = await Manager.GetMaterialRequisitionMaster(MRMasterID);
            MRMasterID = master.IsNullOrDbNull() || master.MRMasterID.IsZero() ? 0 : MRMasterID;
            ApprovalProcessID = MRMasterID.IsNotZero() ? ApprovalProcessID : 0;

            var child = await Manager.GetMaterialRequisitionChild(MRMasterID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var attachments = Manager.GetAttachments(MRMasterID);
            var approvalFeedback = Manager.ReportForMRApprovalFeedback(MRMasterID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            var assesments = Manager.GetAssesments(MRMasterID);
            var isAssessmentMember = Manager.GetIsAssessmentMember();
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.MR, MRMasterID);
            return OkResult(new { Master = master, ChildList = child, Comments = comments, Attachments = attachments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, Assesment = assesments, ForwardInfoComments = forwardInfoComments,IsAssessmentMember = isAssessmentMember });
        }
        [HttpGet("GetMRFromPRHistory/{MRMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetMRFromPRHistory(int MRMasterID, int ApprovalProcessID)
        {
            return await GetMR( MRMasterID,  ApprovalProcessID);
        }

        private async Task<IActionResult> GetMR( int MRMasterID,  int ApprovalProcessID)
        {
            var master = await Manager.GetMaterialRequisitionMasterByID(MRMasterID);
            
            var child = await Manager.GetMaterialRequisitionChild(MRMasterID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var attachments = Manager.GetAttachments(MRMasterID);
            var approvalFeedback = Manager.ReportForMRApprovalFeedback(MRMasterID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            var assesments = Manager.GetAssesments(MRMasterID);
            var isAssessmentMember = Manager.GetIsAssessmentMember();
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.MR, MRMasterID);
            return OkResult(new { Master = master, ChildList = child, Comments = comments, Attachments = attachments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, Assesment = assesments, ForwardInfoComments = forwardInfoComments, IsAssessmentMember = isAssessmentMember });
        }

        [HttpGet("GetSCMMembersForPanel/{DivisionID:int}")]
        public async Task<IActionResult> GetSCMMembersForPanel(int DivisionID)
        {
            var mapList = Manager.GetSCMMembersForPanel(DivisionID).Result;
            return OkResult(new { SCMMembersList = mapList });
        }
        [HttpGet("GetDefaultMRApprovalPanel")]
        public async Task<IActionResult> GetDefaultMRApprovalPanel()
        {
            var mapList = Manager.GetDefaultMRApprovalPanel();
            return OkResult(new { DefaultMRList = mapList });
        }
        
        [HttpGet("GetMaterialRequisitionForReAssessment/{MRMasterID:int}")]
        public async Task<IActionResult> GetMaterialRequisitionForReAssessment(int MRMasterID)
        {
            var master = await Manager.GetMaterialRequisitionMaster(MRMasterID);
            var child = await Manager.GetMaterialRequisitionChild(MRMasterID);
            var attachments = Manager.GetAttachments(MRMasterID);
            var mapList = Manager.GetMRApprovalPanelDefault(MRMasterID).Result;
            return OkResult(new { Master = master, ChildList = child, Attachments = attachments, MRApprovalPanelList = mapList });
        }
        
    }
}