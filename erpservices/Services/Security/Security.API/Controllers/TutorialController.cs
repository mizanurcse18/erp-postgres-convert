
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
    public class TutorialController : BaseController
    {
        private readonly ITutorialManager Manager;

        public TutorialController(ITutorialManager manager)
        {
            Manager = manager;
        }

        // GET: /Tutorial/GetAll
        //[HttpGet("GetAll")]
        //public async Task<IActionResult> GetAll()
        //{
        //    var financialYears = await Manager.GetTutorialListWithDetails();
        //    return OkResult(financialYears);
        //}

        // GET: /Tutorial/Get/{primaryID}

        [HttpGet("GetTutorial/{TutorialID:int}")]
        public async Task<IActionResult> GetTutorial(int TutorialID)
        {
            var tutorial = Manager.GetTutorial(TutorialID);
            return OkResult(new { master = tutorial[0], Attachments = tutorial[1]});
        }

        [HttpGet("GetTutorials")]
        public async Task<IActionResult> GetTutorials()
        {
            var tutorials = await Manager.GetTutorials();
            return OkResult(tutorials);
        }
        // POST: /Tutorial/CreateTutorial
        [HttpPost("SaveTutorial")]
        public async Task<IActionResult> SaveTutorial([FromBody] TutorialDto Tutorial)
        {
            await Manager.SaveChanges(Tutorial);
            return OkResult(true);
        }
        

        [HttpGet("DeleteTutorial/{TutorialID:int}")]
        public async Task<IActionResult> DeleteTutorial(int TutorialID)
        {
            await Manager.Delete(TutorialID);
            return OkResult(new { success = true });

        }


        [HttpGet("GetTutorialsForList")]
        public async Task<IActionResult> GetTutorialsForList()
        {
            var tutorials = await Manager.GetTutorialsForList();
            return OkResult(tutorials);
        }

        [HttpGet("GetRevenuesForList")]
        public async Task<IActionResult> GetRevenuesForList()
        {
            var tutorials = await Manager.GetRevenuesForList();
            return OkResult(tutorials);
        }
    }
}
