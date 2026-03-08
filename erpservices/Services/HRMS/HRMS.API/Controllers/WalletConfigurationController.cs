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
    public class WalletConfigurationController : BaseController
    {
        private readonly IWalletConfigurationManager Manager;

        public WalletConfigurationController(IWalletConfigurationManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetWalletConfigurations()
        {
            var WalletConfigurations = await Manager.GetWalletConfigurationListDic();
            return OkResult(WalletConfigurations);
        }

        [HttpGet("Get")]
        public async Task<IActionResult> GetWalletConfiguration(decimal cashoutrate)
        {
            var WalletConfiguration = await Manager.GetWalletConfiguration(cashoutrate);
            return OkResult(WalletConfiguration);
        }

        [HttpPost("CreateWalletConfiguration")]
        public IActionResult CreateWalletConfiguration([FromBody] WalletConfigurationDto WalletConfiguration)
        {
            Manager.SaveChanges(WalletConfiguration);
            return OkResult(WalletConfiguration);
        }
        // POST: /User/Delete

        [HttpGet("Delete")]
        public async Task<IActionResult> Delete(decimal cashoutrate)
        {
            await Manager.Delete(cashoutrate);
            return OkResult(new { success = true });

        }

    }
}