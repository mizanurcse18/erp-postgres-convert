
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
    public class SystemConfiguration : BaseController
    {
        private readonly ISystemConfigurationManager Manager;

        public SystemConfiguration(ISystemConfigurationManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var systemConfiguration = await Manager.GetSystemConfiguration();
            return OkResult(systemConfiguration);
        }

        [HttpPost("SaveSystemConfiguration")]
        public async Task<IActionResult> SaveSystemConfiguration([FromBody] SystemConfigurationDto systemConfigurationDto)
        {
            await Manager.SaveChanges(systemConfigurationDto);
            return await GetAll();
        }


    }
}
