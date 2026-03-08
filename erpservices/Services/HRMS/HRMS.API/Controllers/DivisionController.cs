using API.Core;
using Core.AppContexts;
using Core.Extensions;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class DivisionController : BaseController
    {
        private readonly IDivisionManager Manager;

        public DivisionController(IDivisionManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetDivisions")]
        public async Task<IActionResult> GetDivisions()
        {
            var divisions = await Manager.GetDivisionListDic();
            return OkResult(divisions);
        }

        [HttpGet("GetDivision/{DivisionID:int}")]
        public async Task<IActionResult> GetDivision(int DivisionID)
        {
            var division = await Manager.GetDivision(DivisionID);
            return OkResult(division);
        }

        [HttpPost("CreateDivision")]
        public IActionResult CreateDivision([FromBody] DivisionDto division)
        {
            Manager.SaveChanges(division);
            return OkResult(division);
        }

        [HttpPost("UploadDivision")]
        public async Task<IActionResult> UploadDivision([FromForm] IFormFile file)
        {
            var result = await Manager.SaveChangesUploadDivision(file);
            return OkResult(result);
        }

        // POST: /User/Delete

        [HttpGet("Delete/{DivisionID:int}")]
        public async Task<IActionResult> Delete(int DivisionID)
        {
            await Manager.Delete(DivisionID);
            return OkResult(new { success = true });

        }

        [HttpGet("GetExportDivisions")]
        public ActionResult GetExportDivisions(string WhereCondition)
        {
            var divList = Manager.GetExportDivisions(WhereCondition);

            return OkResult(divList.Result);
        }

    }
}