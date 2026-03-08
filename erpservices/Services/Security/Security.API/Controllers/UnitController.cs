using API.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Manager.Dto;
using Security.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class UnitController : BaseController
    {
        private readonly IUnitManager Manager;

        public UnitController(IUnitManager manager)
        {
            Manager = manager;
        }
        [HttpGet("GetUnitList")]
        public async Task<ActionResult> GetUnitList()
        {

            var units = await Manager.GetUnitList();
            return OkResult(units);
        }

        [HttpPost("CreateUnit")]
        public IActionResult CreateUnit([FromBody] UnitDto unit)
        {
            Manager.SaveChanges(unit);
            return OkResult(unit);
        }

        [HttpGet("GetUnit/{unitId:int}")]
        public async Task<IActionResult> GetUnit(int unitId)
        {
            var units = await Manager.GetUnit(unitId);
            return OkResult(units);
        }
        [HttpGet("DeleteUnit/{unitId:int}")]
        public IActionResult DeleteUnit(int unitId)
        {
            Manager.DeleteUnit(unitId);
            return Ok(new { Success = true });
        }

    }
}
