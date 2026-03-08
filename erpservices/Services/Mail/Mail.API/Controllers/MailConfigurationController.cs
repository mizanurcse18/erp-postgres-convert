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
    public class MailConfigurationController : BaseController
    {
        private readonly IMailConfigurationManager Manager;

        public MailConfigurationController(IMailConfigurationManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetMailConfigurations")]
        public async Task<IActionResult> GetMailConfigurations()
        {
            var mailConfigurations = await Manager.GetMailConfigurationListDic();
            return OkResult(mailConfigurations);
        }

        [HttpGet("GetMailConfiguration/{MailConfigurationID:int}")]
        public async Task<IActionResult> GetMailConfiguration(int MailConfigurationID)
        {
            var mailConfiguration = await Manager.GetMailConfiguration(MailConfigurationID);
            return OkResult(mailConfiguration);
        }

        [HttpGet("GetActiveConfig")]
        public async Task<ActionResult> GetActiveConfig(int ConfigId)
        {
            //if (!HasViewPrivilege())
            //    return ValidationResult("You have no privilege to view any information.");

            bool isExists = await Manager.GetActiveConfig(ConfigId);

            return OkResult(isExists);
        }
        [HttpPost("CreateMailConfiguration")]
        public IActionResult CreateMailConfiguration([FromBody] MailConfigurationDto mailConfiguration)
        {
            Manager.SaveChanges(mailConfiguration);
            return OkResult(mailConfiguration);
        }
        // POST: /User/Delete

        [HttpGet("Delete/{MailConfigurationID:int}")]
        public async Task<IActionResult> Delete(int MailConfigurationID)
        {
            await Manager.Delete(MailConfigurationID);
            return OkResult(new { success = true });

        }

    }
}