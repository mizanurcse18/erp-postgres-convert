using System;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

using Core;
using API.Core;
using Core.Extensions;
using Core.AppContexts;

using System.Collections.Generic;
using API.Core.Hubs;
using API.Core.Interface;
using Microsoft.AspNetCore.SignalR;
using Accounts.Manager;
using Accounts.Manager.Dto;

namespace Accounts.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class BudgetController : BaseController
    {
        private readonly IBudgetManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public BudgetController(IBudgetManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        //[HttpPost("SaveExpenseClaim")]
        //public IActionResult SaveExpenseClaim([FromBody] ExpenseClaimDto expense)
        //{
        //    var response = Manager.SaveChanges(expense).Result;
        //    _notificationHub.Clients.All.ReceiveNotification("ExpenseClaim");
        //    return OkResult(new { status = response.Item1, message = response.Item2 });
        //}

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var budgetDepartments = Manager.GetAllDeptBudgetList().Result;
            return OkResult(budgetDepartments);
        }
        [HttpGet("GetIOUExpenseBudget/{DepartmentID:int}")]
        public async Task<IActionResult> GetIOUExpenseBudget(int DepartmentID)
        {
            var budgetDepartments = Manager.GetIOUExpenseBudget(DepartmentID).Result;
            return OkResult(budgetDepartments);
        }
        [HttpPost("SaveBudget")]
        public  IActionResult SaveBudget(List<BudgetMasterDto> list)
        {
            Manager.SaveBudget(list);
            return OkResult(true);
        }


    }
}
