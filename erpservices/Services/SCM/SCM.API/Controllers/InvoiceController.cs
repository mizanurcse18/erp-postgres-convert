using API.Core;
using API.Core.Hubs;
using API.Core.Interface;
using Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SCM.Manager.Dto;
using SCM.Manager.Interfaces;
using System.Threading.Tasks;

namespace SCM.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class InvoiceController : BaseController
    {
        private readonly IInvoiceManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public InvoiceController(IInvoiceManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }
        [HttpPost("GetInvocieList")]
        public IActionResult GetInvocieList([FromBody] GridParameter parameters)
        {
            var model = Manager.GetListForGrid(parameters);
            return OkResult(new { parentDataSource = model });
        }
        [HttpPost("GetAdvanceInvoiceListForGrid")]
        public IActionResult GetAdvanceInvoiceListForGrid([FromBody] GridParameter parameters)
        {
            var model = Manager.GetAdvanceInvoiceListForGrid(parameters);
            return OkResult(new { parentDataSource = model });
        }
        [HttpPost("GetRegularInoviceListForGrid")]
        public IActionResult GetRegularInoviceListForGrid([FromBody] GridParameter parameters)
        {
            var model = Manager.GetRegularInvoiceListForGrid(parameters);
            return OkResult(new { parentDataSource = model });
        }

        [HttpPost("GetListForGrid")]
        public IActionResult GetListForGrid([FromBody] GridParameter parameters)
        {
            var model = Manager.GetCreatedInvoiceListForGrid(parameters);
            return OkResult(new { parentDataSource = model });
        }

        [HttpPost("GetApprovedInvoiceListForGrid")]
        public IActionResult GetApprovedInvoiceListForGrid([FromBody] GridParameter parameters)
        {
            var model = Manager.GetApprovedInvoiceListForGrid(parameters);
            return OkResult(new { parentDataSource = model });
        }

        //[HttpGet("GetDataForInvocie/{MRID:int}")]
        //public async Task<IActionResult> GetDataForInvocie(int MRID)
        //{
        //    var master = await Manager.GetMasterData(MRID);
        //    var child = await Manager.GetChildData(MRID);
        //    return OkResult(new { Master = master, ChildList = child });
        //}
        [HttpGet("GetDataForAdvanceInvocie/{POMasterID:int}")]
        public async Task<IActionResult> GetDataForAdvanceInvocie(int POMasterID)
        {
            var master = await Manager.GetMasterData(POMasterID);
            var child = await Manager.GetChildDataAdvance(POMasterID);
            return OkResult(new { Master = master, ChildList = child });
        }
        [HttpGet("GetDataForRegularInvocie/{POMasterID:int}")]
        public async Task<IActionResult> GetDataForRegularInvocie(int POMasterID)
        {
            var master = await Manager.GetMasterData(POMasterID);
            var materialRecieveMasterDetails = await Manager.MaterialReceiveMasterDetailsByPO(POMasterID);
            return OkResult(new { Master = master, ChildList = materialRecieveMasterDetails });
        }
        [HttpPost("SaveInvoice")]
        public IActionResult SaveInvoice([FromBody] InvoiceDto indto)
        {
            var response = Manager.SaveInvoice(indto).Result;
            _notificationHub.Clients.All.ReceiveNotification("INV");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpGet("Get/{InvoiceMasterID:int}/{ApprovalProcessID:int}/{IsAdvanceInvoice:bool}")]
        public async Task<IActionResult> Get(int InvoiceMasterID, int ApprovalProcessID, bool IsAdvanceInvoice)
        {
            var master = await Manager.GetInvoiceMasterDic(InvoiceMasterID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var approvalFeedback = Manager.InvoiceApprovalFeedback(InvoiceMasterID);
            var attachments = Manager.GetAttachments(InvoiceMasterID);
            var rejectedMembers = await Manager.GetRejectedMemeberList(ApprovalProcessID);
            var forwardingMembers = await Manager.GetForwardingMemberList(ApprovalProcessID);
            int isExistSccChild = Manager.GetExistSccChild(InvoiceMasterID);
            if (IsAdvanceInvoice)
            {
                var child = await Manager.GetInvoiceChildListOfDict(InvoiceMasterID);
                return OkResult(new { Master = master, ChildList = child, Comments = comments, Attachments = attachments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers });
            }
            else if(isExistSccChild > 0)
            {
                var child = await Manager.SCCMasterDetailsByInvoiceID(InvoiceMasterID);
                //var invoiceChild = Manager.GetInvoiceChildList(InvoiceMasterID).Result;
                return OkResult(new { Master = master, SccChildList = child, Comments = comments, Attachments = attachments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers });
            }
            else
            {
                var child = await Manager.MaterialReceiveMasterDetailsByInvoiceID(InvoiceMasterID);
                return OkResult(new { Master = master, ChildList = child, Comments = comments, Attachments = attachments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers });
            }
        }

        [HttpGet("GetDataForReassesstment/{InvoiceMasterID:int}/{IsAdvanceInvoice:bool}/{POMasterID:int}")]
        public async Task<IActionResult> GetDataForReassesstment(int InvoiceMasterID, bool IsAdvanceInvoice, int POMasterID)
        {
            var master = await Manager.GetInvoiceMasterDic(InvoiceMasterID);
            var attachments = Manager.GetAttachments(InvoiceMasterID);
            int isExistSccChild = Manager.GetExistSccChild(InvoiceMasterID);
            if (IsAdvanceInvoice)
            {
                var child = await Manager.GetInvoiceChildListOfDict(InvoiceMasterID);
                return OkResult(new { Master = master, ChildList = child, Attachments = attachments });
            }
            else if (isExistSccChild > 0)
            {
                var sccChild = await Manager.SCCMasterDetailsByInvoiceID(InvoiceMasterID);
                var invoiceChild = await Manager.GetSccInvoiceChildList(InvoiceMasterID);
                return OkResult(new { Master = master, SccChildList = sccChild, Attachments = attachments, InvoiceChild = invoiceChild });
            }
            else
            {
                var child = await Manager.MaterialReceiveMasterDetailsForReassessmentAndView(POMasterID, InvoiceMasterID);
                var invoiceChild = await Manager.GetInvoiceChildList(InvoiceMasterID);
                return OkResult(new { Master = master, ChildList = child, Attachments = attachments, InvoiceChild = invoiceChild });
            }
        }

        [HttpGet("GetInvoiceForTaxationVetting/{InvoiceMasterID:int}/{IsAdvanceInvoice:bool}/{POMasterID:int}")]
        public async Task<IActionResult> GetInvoiceForTaxationVetting(int InvoiceMasterID, bool IsAdvanceInvoice, int POMasterID)
        {
            var master = await Manager.GetInvoiceMasterDicForTaxationVetting(InvoiceMasterID);
            var invoiceAttachments = Manager.GetAttachments(InvoiceMasterID);
            int isExistSccChild = Manager.GetExistSccChild(InvoiceMasterID);
            if (IsAdvanceInvoice)
            {
                var child = await Manager.GetInvoiceChildListOfDict(InvoiceMasterID);
                return OkResult(new { Master = master, ChildList = child, InvoiceAttachments = invoiceAttachments });
            }
            else if (isExistSccChild > 0)
            {
                var child = await Manager.MaterialReceiveMasterDetailsForReassessmentAndView(POMasterID, InvoiceMasterID);
                var sccChild = await Manager.SCCMasterDetailsByInvoiceID(InvoiceMasterID);
                return OkResult(new { Master = master, ChildList = child, SccChildList = sccChild, InvoiceAttachments = invoiceAttachments });
            }
            else
            {
                var child = await Manager.MaterialReceiveMasterDetailsForReassessmentAndView(POMasterID, InvoiceMasterID);
                var invoiceChild = await Manager.GetInvoiceChildList(InvoiceMasterID);
                return OkResult(new { Master = master, ChildList = child, InvoiceAttachments = invoiceAttachments, InvoiceChild = invoiceChild });
            }
        }

        [HttpPost("GetRegularInoviceSccListForGrid")]
        public IActionResult GetRegularInoviceSccListForGrid([FromBody] GridParameter parameters)
        {
            var model = Manager.GetRegularInvoiceSccListForGrid(parameters);
            return OkResult(new { parentDataSource = model });
        }

        [HttpGet("GetDataForRegularInvocieScc/{POMasterID:int}")]
        public async Task<IActionResult> GetDataForRegularInvocieScc(int POMasterID)
        {
            var master = await Manager.GetMasterData(POMasterID);
            var materialRecieveMasterDetails = await Manager.SCCReceiveMasterDetailsByPO(POMasterID);
            return OkResult(new { Master = master, SccChildList = materialRecieveMasterDetails });
        }

    }
}