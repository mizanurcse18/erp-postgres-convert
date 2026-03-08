using API.Core;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
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
    public class UddoktaMerchantTagUntagController : BaseController
    {
        private readonly IUddoktaMerchantTagUntagManager Manager;
        

        public UddoktaMerchantTagUntagController(IUddoktaMerchantTagUntagManager manager)
        {
            Manager = manager;
        }
        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAll()
        {

            var mapList = await Manager.GetUddoktaMerchantList();
            return OkResult(mapList);
        }

        [HttpPost("Save")]
        public IActionResult SaveChanges([FromBody] UserWiseUddoktaOrMerchantMapping obj)
        {
            var response = Manager.SaveChanges(obj).Result;
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpGet("Get/{MapID:int}")]
        public async Task<IActionResult> GetUddoktaMerchant(int MapID)
        {
            var UddoktaMerchants = await Manager.GetUddoktaMerchant(MapID);
            return OkResult(UddoktaMerchants);
        }
        [HttpGet("Delete/{MapID:int}")]
        public IActionResult Delet(int MapID)
        {
            bool status = Manager.Delete(MapID);
            return Ok(new { Success = status });
        }

    }
}
