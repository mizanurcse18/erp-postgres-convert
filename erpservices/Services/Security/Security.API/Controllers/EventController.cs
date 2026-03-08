
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using API.Core;
using Security.API.Models;
using Security.Manager;
using System.Globalization;
using System.Collections.Generic;
using System;
using DAL.Core.Extension;
using Security.Manager.Interfaces;

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class EventController : BaseController
    {
        private readonly IEventManager Manager;

        public EventController(IEventManager manager)
        {
            Manager = manager;
        }

        // GET: /Event/GetAll
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var Events = await Manager.GetEventList();
            return OkResult(Events);
        }          
    }
}
