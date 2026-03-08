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
using System.Web;

namespace Accounts.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class IOUOrExpensePaymentController : BaseController
    {
        private readonly IIOUOrExpensePaymentManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public IOUOrExpensePaymentController(IIOUOrExpensePaymentManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        [HttpGet("GetExpenseClaimsForPaymentSattlement/{FromDate}/{ToDate}/{DivisionID:int}/{DepartmentID:int}/{EmployeeID:int}/{PaymentMasterID:int}/{ClaimReferenceNo}")]
        public IActionResult GetExpenseClaimsForPaymentSattlement(string FromDate, string ToDate, int DivisionID, int DepartmentID, int EmployeeID,int PaymentMasterID, string ClaimReferenceNo)
        {
            var decodedReferenceNo = HttpUtility.UrlDecode(ClaimReferenceNo);
            DateTime fromDate = FromDate.Trim() == "default" ? DateTime.MinValue : DateTime.ParseExact(FromDate, "dd-MM-yyyy", null);
            DateTime toDate = ToDate.Trim() == "default" ? DateTime.MaxValue : DateTime.ParseExact(ToDate, "dd-MM-yyyy", null);
            var expenses = Manager.GetFilteredExpenseClaims(fromDate, toDate, DivisionID, DepartmentID, EmployeeID, PaymentMasterID, decodedReferenceNo).Result;
            return OkResult(new { ExpenseClaims = expenses });
        }

        [HttpPost("SaveExpenseClaimPaymentSattlement")]
        public IActionResult SaveExpenseClaimPaymentSattlement([FromBody] IOUOrExpensePaymentDto expense)
        {
            var response = Manager.SaveChanges(expense).Result;
            _notificationHub.Clients.All.ReceiveNotification("IOUExpenseSattlement");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpPost("GetAll")]
        public async Task<IActionResult> GetAll([FromBody] GridParameter parameters)
        {
            var list = Manager.GetMasterList(parameters);
            return OkResult(new { parentDataSource = list });
        }

        [HttpPost("GetAllApprovedHistory")]
        public async Task<IActionResult> GetAllApprovedHistory([FromBody] GridParameter parameters)
        {
            var list = Manager.GetMasterApprovedHistoryList(parameters);
            return OkResult(new { parentDataSource = list });
        }
        //[HttpGet("GetAll/{filterData}")]
        //public async Task<IActionResult> GetAll(string filterData)
        //{
        //    var nfas = Manager.GetMasterList(filterData);
        //    return OkResult(nfas);
        //}

        [HttpGet("Get/{PaymentMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> Get(int PaymentMasterID, int ApprovalProcessID)
        {
            var expenseClaim = await Manager.GetMaster(PaymentMasterID);
            var expenseClaimChild = await Manager.GetChildList(PaymentMasterID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var approvalFeedback = Manager.ExpensePaymentApprovalFeedback(PaymentMasterID);
            
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.ExpensePayment, PaymentMasterID);

            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;

            return OkResult(new { Master = expenseClaim, ChildList = expenseClaimChild, Comments = comments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ForwardInfoComments = forwardInfoComments });
        }

        [HttpGet("GetIOUPayment/{PaymentMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetIOUPayment(int PaymentMasterID, int ApprovalProcessID)
        {
            var iouClaim = await Manager.GetMaster(PaymentMasterID);
            var iouClaimChild = await Manager.GetChildIOUList(PaymentMasterID);
            var comments = Manager.GetIOUApprovalComment(ApprovalProcessID);
            var approvalFeedback = Manager.IOUPaymentApprovalFeedback(PaymentMasterID);

            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.ExpensePayment, PaymentMasterID);

            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            return OkResult(new { Master = iouClaim, ChildList = iouClaimChild, Comments = comments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ForwardInfoComments = forwardInfoComments });
        }

        [HttpGet("GetIOUClaimsForPaymentSattlement/{FromDate}/{ToDate}/{DivisionID:int}/{DepartmentID:int}/{EmployeeID:int}/{PaymentMasterID:int}")]
        public IActionResult GetIOUClaimsForPaymentSattlement(string FromDate, string ToDate, int DivisionID, int DepartmentID, int EmployeeID,int PaymentMasterID)
        {
            DateTime fromDate = FromDate.Trim() == "default" ? DateTime.MinValue : DateTime.ParseExact(FromDate, "dd-MM-yyyy", null);
            DateTime toDate = ToDate.Trim() == "default" ? DateTime.MaxValue : DateTime.ParseExact(ToDate, "dd-MM-yyyy", null);
            var ious = Manager.GetFilteredIOUClaims(fromDate, toDate, DivisionID, DepartmentID, EmployeeID, PaymentMasterID).Result;
            return OkResult(new { IOUClaims = ious });
        }


        [HttpPost("SaveIOUClaimPaymentSattlement")]
        public IActionResult SaveIOUClaimPaymentSattlement([FromBody] IOUPaymentDto iou)
        {
            var response = Manager.SaveIOUChanges(iou).Result;
            _notificationHub.Clients.All.ReceiveNotification("IOUExpenseSattlement");
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }

        [HttpGet("GetIOUSattlement/{PaymentMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetIOUSattlement(int PaymentMasterID, int ApprovalProcessID)
        {
            var iouClaim = await Manager.GetMaster(PaymentMasterID);
            var iouClaimChild = await Manager.GetChildIOUList(PaymentMasterID);
            var comments = Manager.GetIOUApprovalComment(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            return OkResult(new { Master = iouClaim, ChildList = iouClaimChild, Comments = comments, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers });
        }


        [HttpGet("GetIOUSettlementForReAssessment/{PaymentMasterID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> GetIOUSettlementForReAssessment(int PaymentMasterID,int ApprovalProcessID)
        {
            var expenseClaim = await Manager.GetMaster(PaymentMasterID);
            var expenseClaimChild = await Manager.GetChildIOUList(PaymentMasterID);
            return OkResult(new { Master = expenseClaim, ChildList = expenseClaimChild });
        }

        [HttpGet("GetIOUOrExpSettlementForReAssessment/{PaymentMasterID:int}")]
        public async Task<IActionResult> GetIOUOrExpSettlementForReAssessment(int PaymentMasterID)
        {
            
            var expenseClaim = await Manager.GetMaster(PaymentMasterID);
            var expenseClaimChild = await Manager.GetChildList(PaymentMasterID);
            return OkResult(new { Master = expenseClaim, ChildList = expenseClaimChild });

        }

        //Approved Expense Payment to Settlement
        [HttpPost("CreateIOUOrExpPaymentSettlement")]
        public IActionResult CreateIOUOrExpPaymentSettlement([FromBody] IOUOrExpensePaymentDto dto)
        {
            
            var response = Manager.CreateIOUOrExpPaymentSettlement(dto).Result;
            _notificationHub.Clients.All.ReceiveNotification("IOUOrExpensePayment");
            return OkResult(new { status = response.Item1, message = response.Item2 });

        }

    }
}
