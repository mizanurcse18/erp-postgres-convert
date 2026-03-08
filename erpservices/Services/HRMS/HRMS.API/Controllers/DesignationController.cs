using API.Core;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class DesignationController : BaseController
    {
        private readonly IDesignationManager Manager;

        public DesignationController(IDesignationManager manager)
        {
            Manager = manager;
        }
        [HttpGet("GetDesignationList")]
        public async Task<ActionResult> GetDesignationList()
        {

            var designations = await Manager.GetDesignationList();
            return OkResult(designations);
        }

        [HttpPost("CreateDesignation")]
        public IActionResult CreateDesignation([FromBody] DesignationDto designation)
        {
            Manager.SaveChanges(designation);
            return OkResult(designation);
        }

        [HttpPost("UploadDesignation")]
        public async Task<IActionResult> UploadDesignation([FromForm] FileRequestDto request)
        {
            var result = await Manager.SaveChangesUploadDesignation(request);
            return OkResult(result);
        }

        [HttpGet("GetDesignation/{designationId:int}")]
        public async Task<IActionResult> GetDesignation(int designationId)
        {
            var designations = await Manager.GetDesignation(designationId);
            return OkResult(designations);
        }
        [HttpGet("DeletDesignation/{designationId:int}")]
        public IActionResult DeletDesignation(int designationId)
        {
            Manager.DeleteDesignation(designationId);
            return Ok(new { Success = true });
        }

    }
}
