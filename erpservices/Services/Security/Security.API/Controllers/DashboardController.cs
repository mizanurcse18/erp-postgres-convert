using API.Core;
using Core.AppContexts;
using Core.Extensions;
using Manager.Core.CommonDto;
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
    public class DashboardController : BaseController
    {
        private readonly IDashboardManager Manager;

        public DashboardController(IDashboardManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetApprovalDashboardData/{PersonID:int}")]
        public async Task<ActionResult> GetApprovalDashboardData(int PersonID)
        {

            var data =  Manager.GetApprovalDashboardData(PersonID);

            return OkResult(data);
        }
    }
}