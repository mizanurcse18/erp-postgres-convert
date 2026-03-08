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
    public class RegionController : BaseController
    {
        private readonly IRegionManager Manager;

        public RegionController(IRegionManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetRegions")]
        public async Task<IActionResult> GetRegions()
        {
            var regions = await Manager.GetRegionListDic();
            return OkResult(regions);
        }

        [HttpGet("GetRegion/{RegionID:int}")]
        public async Task<IActionResult> GetRegion(int RegionID)
        {
            var region = await Manager.GetRegion(RegionID);
            return OkResult(region);
        }

        [HttpPost("CreateRegion")]
        public IActionResult CreateRegion([FromBody] RegionDto region)
        {
            Manager.SaveChanges(region);
            return OkResult(region);
        }
        // POST: /User/Delete

        [HttpGet("Delete/{RegionID:int}")]
        public async Task<IActionResult> Delete(int RegionID)
        {
            await Manager.Delete(RegionID);
            return OkResult(new { success = true });

        }

    }
}