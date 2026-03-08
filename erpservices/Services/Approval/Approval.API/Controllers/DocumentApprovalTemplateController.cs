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

namespace Approval.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class DocumentApprovalTemplateController : BaseController
    {
        private readonly IDocumentApprovalTemplateManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public DocumentApprovalTemplateController(IDocumentApprovalTemplateManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] DocumentApprovalTemplateDto DocumentApprovalTemplate)
        {
            var response = Manager.SaveChanges(DocumentApprovalTemplate).Result;

            return OkResult(new { status = response.Item1, message = response.Item2 });
        }


        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var data = await Manager.GetDocumentApprovalTemplateList();
            return OkResult(data);
        }
        [HttpGet("Get/{DATID:int}")]
        public async Task<IActionResult> Get(int DATID)
        {

            var res = await Manager.GetDocumentApprovalTemplate(DATID);
            return OkResult(res);
        }
        [HttpGet("GetTemplateWithReplacedData/{DATID:int}")]
        public async Task<IActionResult> GetTemplateWithReplacedData(int DATID)
        {

            var res = await Manager.GetTemplateWithReplacedData(DATID);
            return OkResult(res);
        }

        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await Manager.Delete(id);
            return OkResult(new { success = true });

        }

    }
}