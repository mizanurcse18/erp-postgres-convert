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
    public class HolidayController : BaseController
    {
        private readonly IHolidayManager Manager;

        public HolidayController(IHolidayManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetHolidayList")]
        public async Task<IActionResult> GetHolidayList()
        {
            var holidays = await Manager.GetHolidayList();
            return OkResult(holidays);
        }

        [HttpGet("Get/{FinancialYearID:int}")]
        public async Task<IActionResult> Get(int financialYearID)
        {
            var holidays = await Manager.GetHoliday(financialYearID);

            return OkResult(holidays);
        }

        [HttpPost("CreateHoliday")]
        public async Task<IActionResult> CreateHoliday([FromBody] List<HolidayDto> holiday)
        {
            Manager.SaveChanges(holiday);
            return OkResult(holiday);
        }

        [HttpGet("RemoveHoliday/{FinancialYearID:int}")]
        public async Task<IActionResult> RemoveHoliday(int financialYearID)
        {
            Manager.RemoveHoliday(financialYearID);

            return OkResult(financialYearID);
        }

    }
}
