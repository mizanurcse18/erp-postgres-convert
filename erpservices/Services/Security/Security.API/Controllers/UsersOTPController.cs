using API.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.API.Models;
using Security.Manager.Interfaces;
using System.Threading.Tasks;
using System.Security.Cryptography.Xml;
using System.Collections.Generic;
using Security.DAL.Entities;
using System.IO;
using System;
using Core.Extensions;
using Core.AppContexts;
using Security.Manager.Dto;
using Core;
using Microsoft.AspNetCore.SignalR;
using API.Core.Hubs;
using API.Core.Interface;

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class UsersOTPController : BaseController
    {
        private readonly IUsersOTPManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public UsersOTPController(IUsersOTPManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }


        [HttpPost("GenerateOTP")]
        public IActionResult GenerateOTP([FromBody] UsersOTPDto dto)
        {
            var response = Manager.GenerateOTP(dto).Result;
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }
        [HttpGet("VerifyOTP/{otp}")]
        public async Task<ActionResult> VerifyOTP(string otp)
        {
            (bool, bool) value = Manager.VerifyOTP(otp);
            return OkResult(new { IsVerified = value.Item1 });
        }
        [HttpGet("SaveSMSResponse/{resp}")]
        public async Task<ActionResult> SaveSMSResponse(string resp)
        {
            bool isVerified = Manager.SaveSMSResponse(resp);
            return OkResult(true);
        }
        //[HttpGet("GetOTPHistoryList/{type:int}")]
        //public async Task<ActionResult> GetOTPHistoryList(int type)
        //{
        //    var data = await Manager.GetOTPHistoryList(type);
        //    return OkResult(data);
        //}
        [HttpPost("GetOTPHistoryList")]
        public IActionResult GetOTPHistoryList([FromBody] GridParameter parameters)
        {
            var model = Manager.GetOTPHistoryList(parameters);
            return OkResult(new { parentDataSource = model });
        }


        [HttpPost("UploadPayslipGenerateOTP")]
        public IActionResult UploadPayslipGenerateOTP([FromBody] UsersOTPDto dto)
        {
            //var response = Manager.UploadPayslipGenerateOTP(dto).Result;
            var response = Manager.GenerateOTP(dto).Result;
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }
    }
}
