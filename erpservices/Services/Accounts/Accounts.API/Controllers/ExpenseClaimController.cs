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
    public class ExpenseClaimController : BaseController
    {
        private readonly IExpenseClaimManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public ExpenseClaimController(IExpenseClaimManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        [HttpPost("SaveExpenseClaim")]
        public IActionResult SaveExpenseClaim([FromBody] ExpenseClaimDto expense)
        {
            var response = Manager.SaveChanges(expense).Result;
            _notificationHub.Clients.All.ReceiveNotification("ExpenseClaim");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpPost("GetAll")]
        public async Task<IActionResult> GetAll([FromBody] GridParameter parameters)
        {
            var list = parameters.EmployeeTypes == "Self" ? Manager.GetExpenseClaimList(parameters) : Manager.GetExpenseClaimListForEmp(parameters);
            return OkResult(new { parentDataSource = list });
        }
        [HttpPost("GetAllForEmployee")]
        public async Task<IActionResult> GetAllForEmployee([FromBody] GridParameter parameters)
        {
            var list = Manager.GetExpenseClaimListForEmp(parameters);
            return OkResult(new { parentDataSource = list });
        }
        [HttpGet("GetExpenseForReAssessment/{ECMasterID:int}")]
        public async Task<IActionResult> GetExpenseForReAssessment(int ECMasterID)
        {

            var expenseClaim = await Manager.GetExpenseClaim(ECMasterID);
            var expenseClaimChild = await Manager.GetExpenseClaimChild(ECMasterID);
            return OkResult(new { Master = expenseClaim, ChildList = expenseClaimChild });
        }

        [HttpGet("Get/{ECMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> Get(int ECMasterID, int ApprovalProcessID)
        {
            var expenseClaim = await Manager.GetExpenseClaim(ECMasterID);
            var expenseClaimChild = await Manager.GetExpenseClaimChild(ECMasterID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;

            //var divHeadBudget = Manager.GetDivHeadBudget(ECMasterID);
            var divHeadBudgetDetails = Manager.GetDivHeadBudgetDetails(ECMasterID);
            return OkResult(new
            {
                Master = expenseClaim,
                ChildList = expenseClaimChild,
                Comments = comments,
                RejectedMembers = rejectedMembers,
                ForwardingMembers = forwardingMembers,
                //BudgetHeadDetails = divHeadBudget,
                ClaimBudgetDetails = divHeadBudgetDetails
            });
        }

        [HttpGet("GetExpenseReport/{ECMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetExpenseReport(int ECMasterID, int ApprovalProcessID)
        {
            var expenseClaim = await Manager.GetExpenseClaim(ECMasterID);
            var expenseClaimChild = await Manager.GetExpenseClaimChild(ECMasterID);
            if (ApprovalProcessID == 0)
            {
                ApprovalProcessID = expenseClaim.ApprovalProcessID;
            }
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            var approvalFeedback = Manager.ReportApprovalFeedback(ECMasterID);
            return OkResult(new { Master = expenseClaim, ChildList = expenseClaimChild, Comments = comments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ApprovalFeedback = approvalFeedback });
        }

        [HttpGet("GetIOUAmount/{IOUMasterID:int}")]
        public async Task<IActionResult> GetIOUAmount(int IOUMasterID)
        {
            var amount = Manager.GetIOUClaimAmount(IOUMasterID).Result;
            return OkResult(amount);
        }

        [HttpPost("UpdateExpenseClaimAfterReset")]
        public IActionResult UpdateExpenseClaimAfterReset([FromBody] ExpenseClaimMasterDto ecm)
        {
            Manager.UpdateExpenseClaimAfterReset(ecm);
            return OkResult(true);
        }

        [HttpPost("GetAllExpenseClaims")]
        public async Task<IActionResult> GetAllExpenseClaims([FromBody] GridParameter parameters)
        {
            var list = Manager.GetAllExpenseClaims(parameters);
            return OkResult(new { parentDataSource = list });
        }

        [HttpGet("GetExportAllExpenseClaims")]
        public ActionResult GetExportAllExpenseClaims( string FromDate, string ToDate)
        {
            var list = Manager.GetExportAllExpenseClaims( FromDate, ToDate);

            return OkResult(list.Result);
        }

    }
}
