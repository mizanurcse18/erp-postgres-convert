using API.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.API.Models;
using System.Threading.Tasks;
using System.Security.Cryptography.Xml;
using System.Collections.Generic;
using Security.DAL.Entities;
using System.IO;
using System;
using Core.Extensions;
using Core.AppContexts;
using Core;
using HRMS.Manager.Interfaces;

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class CommonController : BaseController
    {
        private readonly IEmployeeSupervisorMapManager Manager;

        public CommonController(IEmployeeSupervisorMapManager manager)
        {
            Manager = manager;
        }

        [HttpGet("RemoveDiscontinuedEmployee/{EmployeeID:int}")]
        public async Task<IActionResult> RemoveDiscontinuedEmployee(int EmployeeID)
        {
            return OkResult("Deleted");
        }
    }
}
