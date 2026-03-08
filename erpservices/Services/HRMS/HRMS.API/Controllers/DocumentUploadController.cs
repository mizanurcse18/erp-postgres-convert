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
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class DocumentUploadController : BaseController
    {
        private readonly IDocumentUploadManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;
        public DocumentUploadController(IDocumentUploadManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }
        [HttpPost("GetListForGrid")]
        public IActionResult GetListForGrid([FromBody] GridParameter parameters)
        {
            var model = Manager.GetListForGrid(parameters);
            model.IsSubmittedFromPopup = parameters.IsSubmittedFromPopup;
            return OkResult(new { parentDataSource = model });
        }
       

        [HttpPost("SaveDocumentUpload")]
        public IActionResult SaveDocumentUpload([FromBody] DocumentUploadDto documentUpload)
        {
            var response = Manager.SaveChanges(documentUpload).Result;
            _notificationHub.Clients.All.ReceiveNotification("DocumentUpload");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }
        [HttpPost("GetDocumentUpload")]
        public async Task<IActionResult> GetDocumentUpload(DocumentUploadDto documentUploadDto)
        {
            var ead = await Manager.GetDocumentUpload(documentUploadDto.DUID, documentUploadDto.ApprovalProcessID);
            documentUploadDto.DUID = ead.IsNullOrDbNull() || ead.DUID.IsZero() ? 0 : documentUploadDto.DUID;
            documentUploadDto.ApprovalProcessID = documentUploadDto.DUID.IsNotZero() ? documentUploadDto.ApprovalProcessID : 0;
            var comments = Manager.GetApprovalComment(documentUploadDto.ApprovalProcessID);
            var attachments = Manager.GetAttachments(documentUploadDto.DUID);
            var rejectedMembers = Manager.GetRejectedMemeberList(documentUploadDto.ApprovalProcessID).Result;
            var forwardingMembers = await Manager.GetAllEmployeesForDocumentUpload();

            if(attachments.Count > 0)
            {
                FilePath filePath = new FilePath();
                foreach(var atch in attachments)
                {
                    filePath.link = documentUploadDto.BaseUrl +"hrms" + atch.FilePath;
                    ead.FilePaths.Add(filePath);
                }
            }

            //ead.FilePaths = ListfilePath.Cast<>;


            string plainText = JsonConvert.SerializeObject(ead);
            var encryptedString = Util.EncryptString(Util.OTPKey, plainText);
            //var forwardingMembers = Manager.GetForwardingMemberList(DUID, (int)Util.ApprovalType.DocumentUpload, (int)Util.ApprovalPanel.DocumentUpload).Result;

            return OkResult(new { Master = ead, Comments = comments, Attachments = attachments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, EncryptedData = encryptedString });
        }

        [HttpGet("GetDocumentUploadForReAssessment/{DUID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetDocumentUploadForReAssessment(int DUID,int ApprovalProcessID)
        {
            var master = await Manager.GetDocumentUpload(DUID, ApprovalProcessID);
            var attachments = Manager.GetAttachments(DUID);
            return OkResult(new { Master = master, Attachments = attachments });
        }

        [HttpGet("GetDocumentUploadReport/{DUID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetDocumentUploadReport(int DUID, int ApprovalProcessID)
        {
            var ead = await Manager.GetDocumentUpload(DUID, ApprovalProcessID);
            DUID = ead.IsNullOrDbNull() || ead.DUID.IsZero() ? 0 : DUID;
            ApprovalProcessID = DUID.IsNotZero() ? ApprovalProcessID : 0;
            //var master = Manager.ReportForDocumentUploadMaster(DUID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var attachments = Manager.ReportForDocumentUploadAttachments(DUID);
            var approvalFeedback = Manager.ReportForEADApprovalFeedback(DUID);
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.EmployeeeDocumentUpload, DUID);
            return OkResult(new { Master = ead, Comments = comments, Attachments = attachments, ApprovalFeedback = approvalFeedback, ForwardInfoComments = forwardInfoComments });
        }

        [HttpGet("RemoveDocumentUpload/{DUID:int}/{ApprovalProcessID:int}")]
        public IActionResult RemoveDocumentUpload(int DUID, int ApprovalProcessID)
        {
            Manager.RemoveDocumentUpload(DUID, ApprovalProcessID);
            _notificationHub.Clients.All.ReceiveNotification("DocumentUpload");
            return OkResult(DUID);
        }


        [HttpGet("LoadExistingPanelByDUID/{id:int}")]
        public async Task<IActionResult> LoadExistingPanelByDUID(int id)
        {
            var data = Manager.LoadExistingPanelByDUID(id);
            return OkResult(new { data });
        }

        [HttpPost("UpdateDocumentUploadStatus")]
        public IActionResult UpdateDocumentUploadStatus(DocumentUploadResponseDto documentUploadResponseDto)
        {
            Manager.UpdateDocumentUploadStatus(documentUploadResponseDto);
            return OkResult(new { status = true });
        }

        [HttpGet("GetHODDocumentUploadExcel")]
        public IActionResult GetHODDocumentUploadExcel()
        {
            var model = Manager.GetAllHODDocumentUploadList();
            //model.IsSubmittedFromPopup = parameters.IsSubmittedFromPopup;
            return OkResult(model);
        }

    }
}