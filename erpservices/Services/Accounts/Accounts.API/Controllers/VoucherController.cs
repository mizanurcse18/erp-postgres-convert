using Accounts.Manager.Dto;
using Accounts.Manager.Interfaces;
using API.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Accounts.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]

    public class VoucherController : BaseController
    {
        private readonly IVoucherManager Manager;

        public VoucherController(IVoucherManager manager)
        {
            Manager = manager;
        }

        [HttpPost("SaveVoucher")]
        public IActionResult SaveVoucher([FromBody] VoucherDto voucher)
        {
            var response =  Manager.SaveChanges(voucher).Result;
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }
    }
}
