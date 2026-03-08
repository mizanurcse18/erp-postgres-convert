using System;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

using Core;
using API.Core;
using Core.Extensions;
using Core.AppContexts;

using Security.Manager.Dto;
using Security.Manager.Interfaces;
using Security.API.Models;
using Security.Manager;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class ReportController : BaseController
    {
        private readonly ISecurityGroupManager Manager;

        public ReportController(ISecurityGroupManager manager)
        {
            Manager = manager;
        }

        [HttpGet("ExportReport")]
        public async Task<IActionResult> ExportReport(Int32 reportId, String exportType)
        {
            string reportFolder = "upload\\reports";
            string fileName = "HR\\EmployeeReport.rdl";
            IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
            string reportPath = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + reportFolder + "\\" + fileName);
            var expType = (Util.ExportType)Util.GetEnumValue(typeof(Util.ExportType), exportType.IsNullOrEmpty() ? "PDF" : exportType);
            //var report = new RdlReport(reportPath, "");
            //return File();
            return OkResult("success");
        }
    }
}
