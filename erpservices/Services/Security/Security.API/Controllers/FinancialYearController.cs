
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

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class FinancialYearController : BaseController
    {
        private readonly IFinancialYearManager Manager;

        public FinancialYearController(IFinancialYearManager manager)
        {
            Manager = manager;
        }

        // GET: /FinancialYear/GetAll
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var financialYears = await Manager.GetFinancialYearListWithDetails();
            return OkResult(financialYears);
        }

        // GET: /FinancialYear/Get/{primaryID}
        [HttpGet("Get/{FinancialYearID:int}")]
        public async Task<IActionResult> Get(int FinancialYearID)
        {
            var finYear = await Manager.GetFinancialYear(FinancialYearID);
            var periods = Manager.GetPeriodByID(FinancialYearID).Result;

            return OkResult(new { MasterModel = finYear, ChildModels = periods });
        }
        [HttpGet("GetByYear/{year:int}")]
        public async Task<IActionResult> GetByYear(int year)
        {
            var periods = new List<PeriodDto>();

            FinancialYearDto finYear = await Manager.GetFinancialYearByYear(year);
            if(finYear.IsNotNull())
            {
                periods = await Manager.GetPeriodByID(finYear.FinancialYearID);
            }

            return OkResult(new { MasterModel = finYear, ChildModels = periods });
        }
        

        [HttpPost("GetGenerateChildList")]
        public async Task<IActionResult> GetGenerateChildList([FromBody] FinancialYearDto finYear)
        {

            var periodList = new List<PeriodDto>();

            periodList = await Manager.GetGenerateChildList(finYear);

            return OkResult(new { MasterModel = finYear, ChildModels = periodList });
        }

        // POST: /FinancialYear/CreateFinancialYear
        [HttpPost("SaveFinancialYear")]
        public async Task<IActionResult> SaveFinancialYear([FromBody] FinancialYearSaveModel FinancialYear)
        {
            await Manager.SaveChanges(FinancialYear.MasterModel, FinancialYear.ChildModels);
            return await Get(FinancialYear.MasterModel.FinancialYearID);
        }

        // PUT: /FinancialYear/UpdateFinancialYear
        [HttpPut("UpdateFinancialYear")]
        public async Task<IActionResult> UpdateFinancialYear([FromBody] FinancialYearSaveModel FinancialYearUpdate)
        {
            await Manager.SaveChanges(FinancialYearUpdate.MasterModel, FinancialYearUpdate.ChildModels);
            return await Get(FinancialYearUpdate.MasterModel.FinancialYearID);
        }

        // Delete: /FinancialYear/DeleteFinancialYear
        [HttpGet("RemoveFinancialYear/{FinancialYearID:int}")]
        public async Task<IActionResult> RemoveFinancialYear(int FinancialYearID)
        {
            await Manager.RemoveFinancialYear(FinancialYearID);
            return OkResult(new { Success = true });
        }

        [HttpGet("GetExistingFinancialYear")]
        public async Task<ActionResult> GetExistingFinancialYear(int year)
        {

            bool isExists = await Manager.GetExistingFinancialYear(year);

            return OkResult(isExists);
        }

    }
}
