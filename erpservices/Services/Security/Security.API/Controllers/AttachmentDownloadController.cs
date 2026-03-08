using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Core;
using Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Manager;
using Security.Manager.Dto;
using Security.Manager.Interfaces;

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class AttachmentDownloadController : BaseController
    {
        private readonly IAttachmentDownloadManager Manager;
        public AttachmentDownloadController(IAttachmentDownloadManager manager)
        {
            Manager = manager;
        }

        // POST:
        [HttpPost("GetAttachmentForDownload")]
        public IActionResult GetAttachmentForDownload([FromBody] DownloadAttachments param)
        {
            byte[] byteArray = Manager.GetAttachmentForDownload(param);


            return OkResult(byteArray);
        }

       
    }
}