using API.Core;
using Core.AppContexts;
using Core.Extensions;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class BranchInfoController : BaseController
    {
        private readonly IBranchInfoManager Manager;

        public BranchInfoController(IBranchInfoManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetBranchInfos")]
        public async Task<IActionResult> GetBranchInfos()
        {
            var branchInfos = await Manager.GetBranchInfoListDic();
            return OkResult(branchInfos);
        }

        [HttpGet("GetBranchInfo/{BranchID:int}")]
        public async Task<IActionResult> GetBranchInfo(int BranchID)
        {
            var branchInfo = await Manager.GetBranchInfo(BranchID);
            return OkResult(branchInfo);
        }

        [HttpPost("CreateBranchInfo")]
        public IActionResult CreateBranchInfo([FromBody] BranchInfoDto branchInfo)
        {
            Manager.SaveChanges(branchInfo);
            return OkResult(branchInfo);
        }
        // POST: /User/Delete

        [HttpGet("Delete/{BranchID:int}")]
        public async Task<IActionResult> Delete(int BranchID)
        {
            await Manager.Delete(BranchID);
            return OkResult(new { success = true });

        }

    }
}