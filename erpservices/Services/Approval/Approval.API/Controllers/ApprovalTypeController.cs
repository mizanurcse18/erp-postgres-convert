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
    public class ApprovalTypeController : BaseController
    {
        private readonly IApprovalTypeManager Manager;

        public ApprovalTypeController(IApprovalTypeManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetApprovalTypes")]
        public async Task<IActionResult> GetApprovalTypes()
        {
            var ApprovalTypes = await Manager.GetApprovalTypeListDic();
            return OkResult(ApprovalTypes);
        }

        [HttpGet("GetApprovalType/{ApprovalTypeID:int}")]
        public async Task<IActionResult> GetApprovalType(int ApprovalTypeID)
        {
            var ApprovalType = await Manager.GetApprovalType(ApprovalTypeID);
            return OkResult(ApprovalType);
        }

        [HttpPost("CreateApprovalType")]
        public IActionResult CreateApprovalType([FromBody] ApprovalTypeDto ApprovalType)
        {
            Manager.SaveChanges(ApprovalType);
            return OkResult(ApprovalType);
        }
        // POST: /User/Delete

        [HttpGet("Delete/{ApprovalTypeID:int}")]
        public async Task<IActionResult> Delete(int ApprovalTypeID)
        {
            await Manager.Delete(ApprovalTypeID);
            return OkResult(new { success = true });

        }

    }
}