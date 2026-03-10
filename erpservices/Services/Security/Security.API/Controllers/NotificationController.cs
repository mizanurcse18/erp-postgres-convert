using API.Core;
using API.Core.Hubs;
using API.Core.Interface;
using API.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Security.Manager.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class NotificationController : BaseController
    {
        private readonly INotificationManager Manager;
        public NotificationController(INotificationManager manager)
        {
            Manager = manager;
        }

        
        [HttpGet("GetNotificationCountAndList")]
        public async Task<IActionResult> GetNotificationCountAndList()
        {
            var notificationList = new List<Dictionary<string, object>>();
            //await Manager.GetNotificationList(10);
            int notificationCount = 0;
            if (notificationList.Count > 0)
            {
                notificationCount = (int)notificationList.FirstOrDefault()["TotalRows"];
            }
            // run some logic...
            var notification = new
            {
                notificationList,
                notificationCount
            };
            return OkResult(notification);
        }

        [HttpGet("GetNotificationList")]
        public async Task<IActionResult> GetNotificationList()
        {
            var notificationList = await Manager.GetNotificationList();
            // run some logic...            
            return OkResult(notificationList);
        }
        [HttpGet("GetNotificationCountAndListForNFA")]
        public async Task<IActionResult> GetNotificationCountAndListForNFA()
        {
            var notificationList = Manager.GetNotificationListForNFA(10).Result;
            var notificationCount = Manager.GetNotificationListForNFA().Result.Count();
            // run some logic...
            var notification = new
            {
                notificationList,
                notificationCount 
            };
            return OkResult(notification);
        }

        [HttpGet("GetNotificationListForNFA")]
        public async Task<IActionResult> GetNotificationListForNFA()
        {
            var notificationList = Manager.GetNotificationListForNFA().Result;
            // run some logic...            
            return OkResult(notificationList);
        }
        [HttpGet("GetNotificationListForNotification")]
        public async Task<IActionResult> GetNotificationListForNotification()
        {
            var notificationList = await Manager.GetNotificationListForNotification();
            
            var notificationCount = await Manager.GetNotificationList();
            // run some logic...
            var notification = new
            {
                notificationList,
                notificationCount
            };
            return OkResult(notification);
        }
        [HttpGet("GetAPTypeList")]
        public async Task<IActionResult> GetAPTypeList()
        {
            var types = new List<Dictionary<string, object>>();//await Manager.GetAPTypeList();
            return OkResult(types);
        }
        [HttpGet("GetNotificationByAPType/{id:int}")]
        public async Task<IActionResult> GetNotificationByAPType(int id)
        {
            var types = await Manager.GetNotificationByAPType(id);
            return OkResult(types);
        }
        


    }
}
