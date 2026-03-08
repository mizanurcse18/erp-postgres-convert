using API.Core;
using Accounts.Manager.Dto;
using Accounts.Manager.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Accounts.DAL.Entities;

namespace Accounts.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class COAController : BaseController
    {
        private readonly ICOAManager Manager;

        public COAController(ICOAManager manager)
        {
            Manager = manager;
        }
        [HttpGet("GetCOAListDictionary")]
        public async Task<ActionResult> GetCOAListDictionary()
        {

            var COAs = await Manager.GetCOAListDictionary();
            return OkResult(COAs);
        }
        [HttpGet("GetCOAList")]
        public async Task<ActionResult> GetCOAList()
        {

            var COAs = await Manager.GetCOAList();
            return OkResult(COAs);
        }
        [HttpGet("GetCOAReportList")]
        public async Task<ActionResult> GetCOAReportList()
        {

            var COAs = await Manager.GetCOAReportList();
            return OkResult(COAs);
        }

        [HttpPost("SaveChanges")]
        public async Task<ActionResult> Save(ChartOfAccountsDto model)
        {
            var COAModel = Manager.SaveChanges(model);
            return await GetCOAList();
        }

    }
}
