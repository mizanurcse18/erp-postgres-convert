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
    public class VatTaxDeductionSourceController : BaseController
    {
        private readonly IVatTaxDeductionSourceManager Manager;

        public VatTaxDeductionSourceController(IVatTaxDeductionSourceManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetVatTaxDeductionSourceList")]
        public async Task<IActionResult> GetVatTaxDeductionSourceList()
        {
            var vatTaxDeductionSources = await Manager.GetVatTaxDeductionSourceList();
            return OkResult(vatTaxDeductionSources);
        }

        [HttpGet("Get/{FinancialYearID:int}")]
        public async Task<IActionResult> Get(int financialYearID)
        {
            var vatTaxDeductionSources = await Manager.GetVatTaxDeductionSource(financialYearID);

            return OkResult(vatTaxDeductionSources);
        }

        [HttpPost("CreateVatTaxDeductionSource")]
        public async Task<IActionResult> CreateVatTaxDeductionSource([FromBody] List<VatTaxDeductionSourceDto> vatTaxDeductionSource)
        {
            Manager.SaveChanges(vatTaxDeductionSource);
            return OkResult(vatTaxDeductionSource);
        }

        [HttpGet("RemoveVatTaxDeductionSource/{FinancialYearID:int}")]
        public async Task<IActionResult> RemoveVatTaxDeductionSource(int financialYearID)
        {
            Manager.RemoveVatTaxDeductionSource(financialYearID);

            return OkResult(financialYearID);
        }

    }
}
