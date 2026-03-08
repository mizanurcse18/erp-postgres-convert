using API.Core;
using Approval.Manager.Dto;
using Core.AppContexts;
using Core.Extensions;
using HRMS.Manager.Interfaces;
using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class ApprovalPanelController : BaseController
    {
        private readonly IApprovalPanelManager Manager;

        public ApprovalPanelController(IApprovalPanelManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetApprovalPanels")]
        public async Task<IActionResult> GetApprovalPanels()
        {
            var ApprovalPanels = await Manager.GetApprovalPanelListDic();
            return OkResult(ApprovalPanels);
        }

        [HttpGet("GetApprovalPanel/{ApprovalPanelID:int}")]
        public async Task<IActionResult> GetApprovalPanel(int ApprovalPanelID)
        {
            var ApprovalPanel = await Manager.GetApprovalPanel(ApprovalPanelID);
            return OkResult(ApprovalPanel);
        }

        [HttpPost("CreateApprovalPanel")]
        public IActionResult CreateApprovalPanel([FromBody] ApprovalPanelDto ApprovalPanel)
        {
            Manager.SaveChanges(ApprovalPanel);
            return OkResult(ApprovalPanel);
        }
        // POST: /User/Delete

        [HttpGet("Delete/{ApprovalPanelID:int}")]
        public async Task<IActionResult> Delete(int ApprovalPanelID)
        {
            await Manager.Delete(ApprovalPanelID);
            return OkResult(new { success = true });

        }

    }
}