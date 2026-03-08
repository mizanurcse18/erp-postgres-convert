using API.Core;
using Core;
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
    public class LocationController : BaseController
    {
        private readonly ILocationManager Manager;

        public LocationController(ILocationManager manager)
        {
            Manager = manager;
        }
        [HttpPost("GetLocationList")]
        public async Task<ActionResult> GetLocationList([FromBody] GridParameter parameters)
        {
            var locations = Manager.GetLocationList(parameters);
            return OkResult(new { parentDataSource = locations });
            //return OkResult(locations);
        }

        [HttpGet("GetLocationListAll")]
        public async Task<ActionResult> GetLocationListAll()
        {
            var locations = Manager.GetLocationList();
            return OkResult(locations.Result);
        }

        [HttpPost("CreateLocation")]
        public IActionResult CreateLocation([FromBody] LocationDto location)
        {
            Manager.SaveChanges(location);
            return OkResult(location);
        }

        [HttpGet("GetLocation/{locationId:int}")]
        public async Task<IActionResult> GetLocation(int locationId)
        {
            var locations = await Manager.GetLocation(locationId);
            return OkResult(locations);
        }
        [HttpGet("DeleteLocation/{locationId:int}")]
        public IActionResult DeleteLocation(int locationId)
        {
            Manager.DeleteLocation(locationId);
            return Ok(new { Success = true });
        }

    }
}
