using Accounts.Manager.Dto;
using Core;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Accounts.Manager
{
    public interface IIOUOrExpensePaymentManager
    {
        Task<List<ExpenseClaimFilterdData>> GetFilteredExpenseClaims(DateTime FromDate, DateTime ToDate, int DivisionID, int DepartmentID, int EmployeeID,int PaymentMasterID, string ReferenceNo);
        Task<List<IOUClaimFilterdData>> GetFilteredIOUClaims(DateTime FromDate, DateTime ToDate, int DivisionID, int DepartmentID, int EmployeeID,int PaymentMasterID);
        Task<IOUOrExpensePaymentMasterDto> GetMaster(int PaymentMasterID);
        Task<List<ExpenseClaimFilterdData>> GetChildList(int PaymentMasterID);
        //List<IOUOrExpensePaymentMasterDto> GetMasterList(string filterData);
        GridModel GetMasterList(GridParameter parameters);
        GridModel GetMasterApprovedHistoryList(GridParameter parameters);
        Task<(bool, string)> SaveChanges(IOUOrExpensePaymentDto expense);
        IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID);
        IEnumerable<Dictionary<string, object>> GetIOUApprovalComment(int approvalProcessID);
        Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);

        IEnumerable<Dictionary<string, object>> ExpensePaymentApprovalFeedback(int paymentid);
        IEnumerable<Dictionary<string, object>> IOUPaymentApprovalFeedback(int paymentid);
        IEnumerable<Dictionary<string, object>> GetForwardingMemberComments(int APTypeID, int ReferenceID);

        Task<(bool, string)> SaveIOUChanges(IOUPaymentDto expense);

        Task<List<IOUClaimFilterdData>> GetChildIOUList(int PaymentMasterID);
        Task<(bool, string)> CreateIOUOrExpPaymentSettlement(IOUOrExpensePaymentDto dto);

    }
}
