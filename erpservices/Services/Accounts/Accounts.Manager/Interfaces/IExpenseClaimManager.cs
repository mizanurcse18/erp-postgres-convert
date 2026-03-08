using Accounts.Manager.Dto;
using Core;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Accounts.Manager
{
    public interface IExpenseClaimManager
    {
        GridModel GetExpenseClaimList(GridParameter parameters);
        GridModel GetExpenseClaimListForEmp(GridParameter parameters);
        Task<ExpenseClaimMasterDto> GetExpenseClaim(int ECMasterID);
        Task<List<ExpenseClaimChildDto>> GetExpenseClaimChild(int ECMasterID);
        Task<(bool, string)> SaveChanges(ExpenseClaimDto expense);
        Task RemoveExpenseClaim(int ECMasterID);
        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);
        Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);
        Task<decimal> GetIOUClaimAmount(int ECMasterID);
        IEnumerable<Dictionary<string, object>> ReportApprovalFeedback(int ReferenceID);
        Task UpdateExpenseClaimAfterReset(ExpenseClaimMasterDto ecm);

        //IEnumerable<Dictionary<string, object>> GetDivHeadBudget(int ECMasterID);

        IEnumerable<Dictionary<string, object>> GetDivHeadBudgetDetails(int ECMasterID);

        GridModel GetAllExpenseClaims(GridParameter parameters);
       
        Task<List<Dictionary<string, object>>> GetExportAllExpenseClaims( string FromDate, string ToDate);

    }
}
