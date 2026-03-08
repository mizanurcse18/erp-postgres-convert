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
    public class PettyCashDisbursementController : BaseController
    {
        private readonly IPettyCashDisbursementManager Manager;
        private readonly IPettyCashExpenseClaimManager ExpenseManager;
        private readonly IPettyCashAdvanceManager AdvanceManager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public PettyCashDisbursementController(IPettyCashDisbursementManager manager, IPettyCashExpenseClaimManager expenseManager, IPettyCashAdvanceManager advanceManager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            ExpenseManager = expenseManager;
            AdvanceManager = advanceManager;
            _notificationHub = notificationHub;
        }
        [HttpPost("GetAll")]
        public async Task<IActionResult> GetAll([FromBody] GridParameter parameters)
        {
            var list = Manager.GetAllPettyCashDisbursementClaimList(parameters);
            return OkResult(new { parentDataSource = list });
        }
        [HttpGet("DisburseClaim/{MasterID:int}/{ClaimTypeID:int}/{DisbursementRemarks}")]
        public IActionResult DisburseClaim(int MasterID, int ClaimTypeID, string DisbursementRemarks)
        {
            var response = Manager.DisburseClaim(MasterID, ClaimTypeID, DisbursementRemarks).Result;
            _notificationHub.Clients.All.ReceiveNotification("DisburseClaim");
            return OkResult(new { status = response.Item1, message = response.Item2 });

        }


        [HttpGet("Get/{MasterID:int}/{ApprovalProcessID:int}/{ClaimTypeID:int}")]
        public async Task<IActionResult> Get(int MasterID,int ApprovalProcessID, int ClaimTypeID)
        {
            if (ClaimTypeID == 1)
            {
                var expenseClaim = await ExpenseManager.GetPettyCashExpenseClaim(MasterID);
                var expenseClaimChild = await ExpenseManager.GetPettyCashExpenseClaimChild(MasterID);
                var comments = ExpenseManager.GetApprovalComment(ApprovalProcessID);
                var rejectedMembers = ExpenseManager.GetRejectedMemeberList(ApprovalProcessID).Result;
                var forwardingMembers = ExpenseManager.GetForwardingMemberList(ApprovalProcessID).Result;
                return OkResult(new { Master = expenseClaim, ChildList = expenseClaimChild, Comments = comments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers });

            }
            else
            {
                var pettycashAdvance = await AdvanceManager.GetPettyCashAdvance(MasterID);
                var pettycashAdvanceChild = await AdvanceManager.GetPettyCashAdvanceChild(MasterID);
                var comments = AdvanceManager.GetApprovalComment(ApprovalProcessID);
                var rejectedMembers = AdvanceManager.GetRejectedMemeberList(ApprovalProcessID).Result;
                var forwardingMembers = AdvanceManager.GetForwardingMemberList(MasterID, (int)Util.ApprovalType.PettyCashAdvanceClaim, (int)Util.ApprovalPanel.PettyCashAdvanceClaim).Result;
                return OkResult(new { Master = pettycashAdvance, ChildList = pettycashAdvanceChild, Comments = comments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers });

            }


        }
    }
}
