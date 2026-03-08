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
    public class WageCodeConfigurationController : BaseController
    {
        private readonly IWageCodeConfigurationManager Manager;

        public WageCodeConfigurationController(IWageCodeConfigurationManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetWageCodeConfigurations()
        {
            var WageCodeConfigurations = await Manager.GetWageCodeConfigurationListDic();
            return OkResult(WageCodeConfigurations);
        }

        [HttpGet("Get/{WageCodeConfigurationID:int}")]
        public async Task<IActionResult> GetWageCodeConfiguration(int WageCodeConfigurationID)
        {
            var WageCodeConfiguration = await Manager.GetWageCodeConfiguration(WageCodeConfigurationID);
            return OkResult(WageCodeConfiguration);
        }

        [HttpPost("CreateWageCodeConfiguration")]
        public IActionResult CreateWageCodeConfiguration([FromBody] WageCodeConfigurationDto WageCodeConfiguration)
        {
            Manager.SaveChanges(WageCodeConfiguration);
            return OkResult(WageCodeConfiguration);
        }
        // POST: /User/Delete

        [HttpGet("Delete/{WageCodeConfigurationID:int}")]
        public async Task<IActionResult> Delete(int WageCodeConfigurationID)
        {
            await Manager.Delete(WageCodeConfigurationID);
            return OkResult(new { success = true });

        }

    }
}