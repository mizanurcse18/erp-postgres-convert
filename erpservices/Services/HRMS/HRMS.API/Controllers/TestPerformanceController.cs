using API.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Core;
using HRMS.Manager.Interfaces;

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class TestPerformanceController : BaseController
    {
        private readonly IEmployeeManager Manager;

        public TestPerformanceController(IEmployeeManager manager)
        {
            Manager = manager;
        }

        [HttpPost("CheckPerformance")]
        public async Task<IActionResult> TestPerformance([FromBody] GridParameter parameters)
        {
            var results = new Dictionary<string, long>();
            
            // Test synchronous version
            var syncStartTime = DateTime.Now.Ticks;
            var syncModel = Manager.GetEmployeeListDic(parameters);
            var syncEndTime = DateTime.Now.Ticks;
            results.Add("Synchronous", (syncEndTime - syncStartTime) / TimeSpan.TicksPerMillisecond);

            // Test asynchronous version
            var asyncStartTime = DateTime.Now.Ticks;
            var asyncModel = await Manager.GetEmployeeListDicAsync(parameters);
            var asyncEndTime = DateTime.Now.Ticks;
            results.Add("Asynchronous", (asyncEndTime - asyncStartTime) / TimeSpan.TicksPerMillisecond);

            return OkResult(new 
            {
                PerformanceResults = results,
                SyncData = syncModel,
                AsyncData = asyncModel
            });
        }
    }
} 