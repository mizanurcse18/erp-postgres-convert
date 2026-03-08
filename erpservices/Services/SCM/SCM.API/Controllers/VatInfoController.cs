using API.Core;
using Core.AppContexts;
using Core.Extensions;
using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCM.Manager.Dto;
using SCM.Manager.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCM.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class VatInfoController : BaseController
    {
        private readonly IVatInfoManager Manager;

        public VatInfoController(IVatInfoManager manager)
        {
            Manager = manager;
        }
        
        [HttpGet("GetVatInfos")]
        public async Task<IActionResult> GetVatInfos()
        {
            var VatInfos = await Manager.GetVatInfoListDic();
            return OkResult(VatInfos);
        }

        [HttpGet("GetVatInfo/{VatInfoID:int}")]
        public async Task<IActionResult> GetVatInfo(int VatInfoID)
        {
            var VatInfo = await Manager.GetVatInfo(VatInfoID);
            return OkResult(VatInfo);
        }

        [HttpPost("CreateVatInfo")]
        public async Task<IActionResult> CreateVatInfo([FromBody] VatInfoDto VatInfo)
        {
            var newData = await Manager.SaveChanges(VatInfo);
            return OkResult(VatInfo);
        }
        // POST: /User/Delete

        [HttpGet("Delete/{VatInfoID:int}")]
        public async Task<IActionResult> Delete(int VatInfoID)
        {
            await Manager.Delete(VatInfoID);
            return OkResult(new { success = true });

        }

    }
}