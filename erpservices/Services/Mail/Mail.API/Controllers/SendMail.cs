using API.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mail.API.Controllers
{
    [ApiController, Route("[controller]")]
    public class SendMail : BaseController
    {
        [HttpPost("SendEMailToRecipients")]
        public async Task<IActionResult> SendEMailToRecipients([FromBody] EmailDtoCore emailData)
        {
            return Ok("ok");
        }
    }
}
