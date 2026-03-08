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
    public class JobGradeController : BaseController
    {
        private readonly IJobGradeManager Manager;

        public JobGradeController(IJobGradeManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetJobGrades")]
        public async Task<IActionResult> GetJobGrades()
        {
            var divisions = await Manager.GetJobGradeListDic();
            return OkResult(divisions);
        }

        [HttpGet("GetJobGrade/{JobGradeID:int}")]
        public async Task<IActionResult> GetJobGrade(int JobGradeID)
        {
            var division = await Manager.GetJobGrade(JobGradeID);
            return OkResult(division);
        }

        [HttpPost("CreateJobGrade")]
        public IActionResult CreateJobGrade([FromBody] JobGradeDto division)
        {
            Manager.SaveChanges(division);
            return OkResult(division);
        }
        // POST: /User/Delete

        [HttpGet("Delete/{JobGradeID:int}")]
        public async Task<IActionResult> Delete(int JobGradeID)
        {
            await Manager.Delete(JobGradeID);
            return OkResult(new { success = true });

        }

    }
}