using API.Core;
using Core.AppContexts;
using Core.Extensions;
using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCM.Manager.Dto;
using SCM.Manager.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCM.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class VendorAssessmentMemberController : BaseController
    {
        private readonly IVendorAssessmentMemberManager Manager;

        public VendorAssessmentMemberController(IVendorAssessmentMemberManager manager)
        {
            Manager = manager;
        }
        

        [HttpPost("SaveVendorAssessmentMember")]
        public async Task<IActionResult> SaveVendorAssessmentMember([FromBody] VendorAssessmentMemberDto VendorAssessmentMember)
        {
            var newData = await Manager.SaveChanges(VendorAssessmentMember);
            return OkResult(VendorAssessmentMember);
        }
        // POST: /User/Delete


    }
}