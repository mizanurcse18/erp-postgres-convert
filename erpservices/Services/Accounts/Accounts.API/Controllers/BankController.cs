using API.Core;
using Accounts.Manager.Dto;
using Accounts.Manager.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class BankController : BaseController
    {
        private readonly IBankManager Manager;

        public BankController(IBankManager manager)
        {
            Manager = manager;
        }
        [HttpGet("GetAll")]
        public async Task<ActionResult> GetBankList()
        {

            var banks = await Manager.GetBankList();
            return OkResult(banks);
        }

        [HttpPost("Save")]
        public IActionResult CreateBank([FromBody] BankDto bank)
        {
            Manager.SaveChanges(bank);
            return OkResult(bank);
        }

        [HttpGet("Get/{BankID:int}")]
        public async Task<IActionResult> GetBank(int BankID)
        {
            var banks = await Manager.GetBank(BankID);
            return OkResult(banks);
        }
        [HttpGet("Delete/{BankID:int}")]
        public IActionResult DeletBank(int BankID)
        {
            bool status = Manager.DeleteBank(BankID);
            return Ok(new { Success = status });
        }

    }
}
