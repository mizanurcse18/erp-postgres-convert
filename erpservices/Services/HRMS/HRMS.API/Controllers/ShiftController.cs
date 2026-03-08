using API.Core;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class ShiftController : BaseController
    {
        private readonly IShiftManager Manager;

        public ShiftController(IShiftManager manager)
        {
            Manager = manager;
        }

        [HttpGet("Get/{ShiftID:int}")]
        public async Task<IActionResult> Get(int ShiftID)
        {
            var model = await Manager.GetShfitByShiftingMasterId(ShiftID);
            return OkResult(model);
        }

        [HttpPost("CreateShift")]
        public async Task<IActionResult> CreateShiftAsync([FromBody] ShiftDto shift)
        {
            var model = await Manager.SaveChanges(shift);
            return await Get(model.ShiftingMasterID);
        }

        [HttpGet("GetList")]
        public async Task<IActionResult> GetList()
        {
            var model = await Manager.GetShfitList();
            return OkResult(model);
        }
        [HttpGet("Remove/{ShiftID:int}")]
        public async Task<IActionResult> Remove(int ShiftID)
        {
            await Manager.RemoveShfitByShiftingMasterId(ShiftID);
            return OkResult(new { success = true });
        }
    }
}
