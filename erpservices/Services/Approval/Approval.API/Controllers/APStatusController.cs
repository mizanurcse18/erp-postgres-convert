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
    public class APStatusController : BaseController
    {
        private readonly IAPStatusManager Manager;

        public APStatusController(IAPStatusManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetAPStatuss")]
        public async Task<IActionResult> GetAPStatuss()
        {
            var APStatuss = await Manager.GetAPStatusListDic();
            return OkResult(APStatuss);
        }

        [HttpGet("GetAPStatus/{APStatusID:int}")]
        public async Task<IActionResult> GetAPStatus(int APStatusID)
        {
            var APStatus = await Manager.GetAPStatus(APStatusID);
            return OkResult(APStatus);
        }

        [HttpPost("CreateAPStatus")]
        public IActionResult CreateAPStatus([FromBody] APStatusDto APStatus)
        {
            Manager.SaveChanges(APStatus);
            return OkResult(APStatus);
        }
        // POST: /User/Delete

        [HttpGet("Delete/{APStatusID:int}")]
        public async Task<IActionResult> Delete(int APStatusID)
        {
            await Manager.Delete(APStatusID);
            return OkResult(new { success = true });

        }

    }
}