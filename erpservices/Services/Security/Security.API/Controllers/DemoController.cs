using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Core;
using Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Manager;
using Security.Manager.Interfaces;

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class DemoController : BaseController
    {
        private readonly IDemoManager Manager;
        public DemoController(IDemoManager manager)
        {
            Manager = manager;
        }

        // POST: /Demo/GetUsers
        [HttpPost("SortFilterCustomGrid")]
        public IActionResult SortFilterCustomGrid([FromBody]GridParameter parameters)
        {
            var model = Manager.GetPersonList(parameters);
            int totalCount = Manager.GetTotalPerson();
            var customList = new
            {
                model,
                totalCount
            }; 
            return OkResult(customList);
        }

    }
}