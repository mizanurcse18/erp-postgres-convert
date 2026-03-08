using API.Core;
using Core.AppContexts;
using Core.Extensions;
using Mail.Manager.Dto;
using Mail.Manager.Interfaces;
using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mail.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class MailGroupSetupController : BaseController
    {
        private readonly IMailGroupSetupManager Manager;

        public MailGroupSetupController(IMailGroupSetupManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetMailGroupSetups")]
        public async Task<IActionResult> GetMailGroupSetups()
        {
            var mailConfigurations = await Manager.GetMailGroupSetupListDic();
            return OkResult(mailConfigurations);
        }

        [HttpGet("GetMailGroupSetup/{MailGroupSetupID:int}")]
        public async Task<IActionResult> GetMailGroupSetup(int MailGroupSetupID)
        {
            var mailConfiguration = await Manager.GetMailGroupSetup(MailGroupSetupID);
            var MailSetups = await Manager.GetMailSetup(MailGroupSetupID);
            var mailConfigurationObj = new
            {
                mailConfiguration,
                MailSetups
            };
            return OkResult(mailConfigurationObj);
        }

        [HttpPost("CreateMailGroupSetup")]
        public IActionResult CreateMailGroupSetup([FromBody] MailGroupSetupDto mailConfiguration)
        {
            Manager.SaveChanges(mailConfiguration);
            return OkResult(mailConfiguration);
        }
        // POST: /User/Delete

        [HttpGet("Delete/{MailGroupSetupID:int}")]
        public async Task<IActionResult> Delete(int MailGroupSetupID)
        {
            await Manager.Delete(MailGroupSetupID);
            return OkResult(new { success = true });

        }

    }
}