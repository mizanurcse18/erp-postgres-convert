using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.EntityBase;
using DAL.Core.Extension;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static Core.Util;

namespace Accounts.Manager
{
    public class PettyCashDisbursementManager : ManagerBase, IPettyCashDisbursementManager
    {
        private readonly IRepository<PettyCashExpenseMaster> PettyCashExpDisbursementRepo;
        private readonly IRepository<PettyCashAdvanceMaster> PettyCashAdvDisbursementRepo;
        private readonly IRepository<CustodianWallet> CustodianWalletRepo;
        private readonly IRepository<PettyCashTransactionHistory> PettyCashTransactionHistoryRepo;
        //readonly IModelAdapter Adapter;
        public PettyCashDisbursementManager(IRepository<PettyCashExpenseMaster> pettyCashExpDisbursementRepo, IRepository<PettyCashAdvanceMaster> pettyCashAdvDisbursementRepo, IRepository<CustodianWallet> custodianWalletRepo, IRepository<PettyCashTransactionHistory> pettyCashTransactionHistoryRepo)
        {
            PettyCashExpDisbursementRepo = pettyCashExpDisbursementRepo;
            PettyCashAdvDisbursementRepo = pettyCashAdvDisbursementRepo;
            CustodianWalletRepo = custodianWalletRepo;
            PettyCashTransactionHistoryRepo = pettyCashTransactionHistoryRepo;
        }
        public GridModel GetAllPettyCashDisbursementClaimList(GridParameter parameters)
        {
            string sql = $@"SELECT DISTINCT A.* FROM (
		                                    SELECT 
		                                     M.PCEMID MasterID
		                                    ,M.ReferenceNo
		                                    ,M.SubmitDate ClaimDate
		                                    ,M.EmployeeID
		                                    ,M.CWID
		                                    ,M.ApprovalStatusID
		                                    ,M.GrandTotal
		                                    ,1 ClaimTypeID
		                                    ,'Expense' ClaimType
		                                    ,M.ClaimStatusID
		                                    ,M.IsDisbursement
		                                    ,M.IsSettled
		                                    ,VA.FullName+VA.EmployeeCode+VA.DepartmentName+VA.DivisionName EmployeeWithDepartment
		                                    ,M.CreatedDate
											,VA.DepartmentName
											,VA.FullName AS EmployeeName, VA.DivisionName
											,VA.ImagePath
											,VA.EmployeeCode
                                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                                            ,0 IsResubmit
											,0 ReSubmitTotalAmount
											,0 PayableAmount
											,0 ReceiveableAmount
		                                    FROM PettyCashExpenseMaster M
		                                    LEFT JOIN HRMS..ViewALLEmployee VAE ON M.EmployeeID=VAE.EmployeeID
		                                    LEFT JOIN Security..Users U ON U.UserID = M.CreatedBy
		                                    LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
		                                    LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = M.ApprovalStatusID
		                                    LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = M.PCEMID AND AP.APTypeID = {(int)Util.ApprovalType.PettyCashExpenseClaim}
                                            LEFT JOIN (
                                                        SELECT 
                                                              APEmployeeFeedbackID,ApprovalProcessID,IsEditable ,IsSCM,IsMultiProxy  
                                                        FROM 
                                                            Approval.dbo.functionJoinListAEF({AppContexts.User.EmployeeID})
                                            )AEF ON AEF.ApprovalProcessID = AP.ApprovalProcessID
		                                    WHERE M.ApprovalStatusID=23 AND ISNULL(M.IsDisbursement,0)=0 AND ISNULL(M.IsSettled,0)=0

		                                    UNION ALL

		                                    SELECT 
		                                     M.PCAMID MasterID
		                                    ,M.ReferenceNo
		                                    ,M.RequestDate ClaimDate
		                                    ,M.EmployeeID
		                                    ,M.CWID
		                                    ,M.ApprovalStatusID
		                                    ,M.GrandTotal
		                                    ,2 ClaimTypeID
		                                    ,CASE WHEN ISNULL(M.IsResubmit,0)=1 THEN 'Resubmit Advance' ELSE 'Advance' END AS ClaimType
		                                    ,M.ClaimStatusID
		                                    ,M.IsDisbursement
		                                    ,M.IsSettled
		                                    ,VA.FullName+VA.EmployeeCode+VA.DepartmentName+VA.DivisionName EmployeeWithDepartment
		                                    ,M.CreatedDate
											,VA.DepartmentName
											,VA.FullName AS EmployeeName, VA.DivisionName
											,VA.ImagePath
											,VA.EmployeeCode
                                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                                            ,M.IsResubmit
											,M.ReSubmitTotalAmount
											,M.PayableAmount
											,M.ReceiveableAmount
		                                    FROM PettyCashAdvanceMaster M
		                                    LEFT JOIN HRMS..ViewALLEmployee VAE ON M.EmployeeID=VAE.EmployeeID
		                                    LEFT JOIN Security..Users U ON U.UserID = M.CreatedBy
		                                    LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
		                                    LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = M.ApprovalStatusID
                                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = M.PCAMID AND AP.APTypeID = {(int)Util.ApprovalType.PettyCashAdvanceClaim}
                                            LEFT JOIN (
                                                        SELECT 
                                                              APEmployeeFeedbackID,ApprovalProcessID,IsEditable ,IsSCM,IsMultiProxy  
                                                        FROM 
                                                            Approval.dbo.functionJoinListAEF({AppContexts.User.EmployeeID})
                                            )AEF ON AEF.ApprovalProcessID = AP.ApprovalProcessID
		                                    WHERE M.ApprovalStatusID=23 AND (ISNULL(M.IsDisbursement,0)=0 OR (M.ResubmitApprovalStatusID=23 AND ISNULL(M.IsResubmitDisbursement,0)=0)) AND ISNULL(M.IsSettled,0)=0
		                                    ) A
                                            LEFT JOIN CustodianWallet CW ON CW.CWID=A.CWID
		                                    
                                            WHERE CW.EmployeeID={AppContexts.User.EmployeeID}";
            var result = PettyCashAdvDisbursementRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }


        public async Task<(bool, string)> DisburseClaim(int MasterID, int ClaimTypeID, string DisbursementRemarks)
        {
            //Can Disburse
            if (ClaimTypeID == 1)
            {
                string sqlCanDisburse = $@"SELECT PE.*,CW.EmployeeID FROM
											PettyCashExpenseMaster PE
											JOIN CustodianWallet CW ON PE.CWID=CW.CWID
											WHERE PE.PCEMID={MasterID} AND CW.EmployeeID={AppContexts.User.EmployeeID} 
											AND PE.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved} AND isnull(PE.IsDisbursement,0)=0 AND isnull(PE.IsSettled,0)=0";
                var canDisburse = PettyCashExpDisbursementRepo.GetModelData<PettyCashExpenseMasterDto>(sqlCanDisburse);
                if (canDisburse.IsNull() || canDisburse.PCEMID == 0)
                {
                    return (false, $"Sorry, You are not permitted this disbursement!");
                }

                var claimDisburse = PettyCashExpDisbursementRepo.Get((long)MasterID);
                if (claimDisburse.IsNullOrDbNull())
                    return (false, "Expense Claim not found.");
                else if (claimDisburse.ApprovalStatusID == (int)Util.ApprovalStatus.Approved && (claimDisburse.IsDisbursement == false || claimDisburse.IsDisbursement == null))
                {
                    claimDisburse.IsDisbursement = true;
                    claimDisburse.DisbursementDate = DateTime.Now;
                    claimDisburse.DisbursementBy = AppContexts.User.UserID;
                    claimDisburse.DisbursementRemarks = DisbursementRemarks;
                    claimDisburse.IsSettled = true;
                    claimDisburse.SettlementDate = DateTime.Now;
                    claimDisburse.SettledBy = AppContexts.User.UserID;
                    claimDisburse.ClaimStatusID = 225; //Complete Process
                    claimDisburse.SetModified();

                    using (var unitOfWork = new UnitOfWork())
                    {
                        PettyCashExpDisbursementRepo.Add(claimDisburse);
                        unitOfWork.CommitChangesWithAudit();

                    }

                }
                UpdateCustodianBalanceModel(claimDisburse.CWID, false, claimDisburse.GrandTotal, 0, 0);
                InsertTransactionTableModel(claimDisburse.ReferenceNo, ClaimTypeID, "Expense", MasterID, claimDisburse.GrandTotal, 0, 0, claimDisburse.CWID);

            }
            if (ClaimTypeID == 2)
            {
                string sqlCanDisburse = $@"SELECT PA.*,CW.EmployeeID FROM
											PettyCashAdvanceMaster PA
											JOIN CustodianWallet CW ON PA.CWID=CW.CWID
											WHERE PA.PCAMID={MasterID} AND CW.EmployeeID={AppContexts.User.EmployeeID} 
											AND PA.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved} AND (isnull(PA.IsDisbursement,0)=0 OR (PA.ResubmitApprovalStatusID=23 AND isnull(PA.IsResubmitDisbursement,0)=0)) AND isnull(PA.IsSettled,0)=0";
                var canDisburse = PettyCashAdvDisbursementRepo.GetModelData<PettyCashAdvanceMasterDto>(sqlCanDisburse);

                if (canDisburse.IsNull() || canDisburse.PCAMID == 0)
                {
                    return (false, $"Sorry, You are not permitted this disbursement!");
                }

                var claimDisburse = PettyCashAdvDisbursementRepo.Get((long)MasterID);
                if (claimDisburse.IsNullOrDbNull())
                    return (false, "Advance Claim not found.");
                else if (claimDisburse.ResubmitApprovalStatusID == null && claimDisburse.ApprovalStatusID == (int)Util.ApprovalStatus.Approved && (claimDisburse.IsDisbursement == false || claimDisburse.IsDisbursement == null))
                {

                    claimDisburse.IsDisbursement = true;
                    claimDisburse.DisbursementDate = DateTime.Now;
                    claimDisburse.DisbursementBy = AppContexts.User.UserID;
                    claimDisburse.DisbursementRemarks = DisbursementRemarks;
                    claimDisburse.ClaimStatusID = 224; //Waiting for Resubmit
                    claimDisburse.PayableAmount = 0;
                    claimDisburse.ReceiveableAmount = 0;
                    claimDisburse.SetModified();

                    using (var unitOfWork = new UnitOfWork())
                    {
                        PettyCashAdvDisbursementRepo.Add(claimDisburse);
                        unitOfWork.CommitChangesWithAudit();

                    }
                    UpdateCustodianBalanceModel(claimDisburse.CWID, false, canDisburse.GrandTotal, 0, 0);
                    InsertTransactionTableModel(claimDisburse.ReferenceNo, ClaimTypeID, "Advance", MasterID, claimDisburse.GrandTotal, 0, 0, claimDisburse.CWID);
                }
                else
                {

                    claimDisburse.IsResubmitDisbursement = true;
                    claimDisburse.ResubmitDisbursementDate = DateTime.Now;
                    claimDisburse.ResubmitDisbursementBy = AppContexts.User.UserID;
                    claimDisburse.ResubmitDisbursementRemarks = DisbursementRemarks;
                    claimDisburse.IsSettled = true;
                    claimDisburse.SettlementDate = DateTime.Now;
                    claimDisburse.SettledBy = AppContexts.User.UserID;
                    claimDisburse.ClaimStatusID = 225; //Complete process
                    claimDisburse.PayableAmount = claimDisburse.GrandTotal < claimDisburse.ReSubmitTotalAmount ? (claimDisburse.ReSubmitTotalAmount - claimDisburse.GrandTotal) : 0;
                    claimDisburse.ReceiveableAmount = claimDisburse.GrandTotal > claimDisburse.ReSubmitTotalAmount ? (claimDisburse.GrandTotal - claimDisburse.ReSubmitTotalAmount) : 0;
                    claimDisburse.SetModified();

                    using (var unitOfWork = new UnitOfWork())
                    {
                        PettyCashAdvDisbursementRepo.Add(claimDisburse);
                        unitOfWork.CommitChangesWithAudit();

                    }
                    UpdateCustodianBalanceModel(claimDisburse.CWID, true, canDisburse.GrandTotal, claimDisburse.PayableAmount ?? 0, claimDisburse.ReceiveableAmount ?? 0);
                    InsertTransactionTableModel(claimDisburse.ReferenceNo, 3, "Resubmit", MasterID, claimDisburse.GrandTotal, claimDisburse.PayableAmount ?? 0, claimDisburse.ReceiveableAmount ?? 0, claimDisburse.CWID);
                }
            }



            //InsertTransactionTableModel(canDisburse);



            return (true, "Claim Disbursed.");

            //await Task.CompletedTask;
        }

        private void InsertTransactionTableModel(string referenceNo, int claimTypeID, string typeName, long masterID, decimal grandTotal, decimal payable, decimal receiveable, long CWID)
        {
            var transModel = new PettyCashTransactionHistory
            {
                ReferenceNo = referenceNo,
                TypeID = claimTypeID,
                TypeName = typeName,
                MasterID = (int)masterID,
                PayableAmount = grandTotal + payable,
                ReceivableAmount = receiveable,
                CustodianID = (int)CWID,
            };
            using (var unitOfWork = new UnitOfWork())
            {
                transModel.SetAdded();
                SetAuditFields(transModel);
                PettyCashTransactionHistoryRepo.Add(transModel);
                unitOfWork.CommitChangesWithAudit();
            }
        }

        public void UpdateCustodianBalanceModel(long CWID, bool Resubmit, decimal grandTotal, decimal payable, decimal receiveable)
        {
            var existingCW = CustodianWalletRepo.Entities.Where(x => x.CWID == CWID).SingleOrDefault();

            existingCW.CurrentBalance = Resubmit == false ? (existingCW.CurrentBalance + receiveable) - (grandTotal + payable) : existingCW.CurrentBalance + receiveable - payable;
            existingCW.SetModified();

            using (var unitOfWork = new UnitOfWork())
            {
                CustodianWalletRepo.Add(existingCW);
                unitOfWork.CommitChangesWithAudit();

            }
        }
    }
}
