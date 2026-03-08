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
    public class ScheduleController : BaseController
    {
        private readonly IScheduleManager Manager;

        public ScheduleController(IScheduleManager manager)
        {
            Manager = manager;
        }

        [HttpPost("ExecuteSchedule")]
        public IActionResult ExecuteSchedule([FromBody] ScheduleUtilityDto obj)
        {
            Manager.ExecuteSchedule(obj);
            return OkResult(obj);
        }
        
    }
}