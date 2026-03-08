using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using Accounts.Manager.Interfaces;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Accounts.Manager.Implementations
{

    public class COAManager : ManagerBase, ICOAManager
    {
        private readonly IRepository<ChartOfAccounts> ChartOfAccountsRepo;
        public COAManager(IRepository<ChartOfAccounts> chartOfAccountsRepo)
        {
            ChartOfAccountsRepo = chartOfAccountsRepo;

        }

        public async Task<List<Dictionary<string, object>>> GetCOAListDictionary()
        {
            string sql = $@"
							WITH CountCTE AS (
                                SELECT ParentID, COUNT(*) AS ChildCount
                                FROM viewCOA
                                GROUP BY ParentID
                            ),
                            RecursiveCTE AS (
	                            SELECT 
		                            *
	                            FROM viewCOA COA
	                            WHERE ParentID = 0
    
                                UNION ALL
    
                                SELECT TV.*
                                FROM viewCOA TV
                                JOIN RecursiveCTE RC ON TV.ParentID = RC.COAID
                            )
                            SELECT R.*, ISNULL(C.ChildCount, 0) AS FinalChildCount
                            FROM RecursiveCTE R
                            LEFT JOIN CountCTE C ON R.COAID = C.ParentID
            ";

            return ChartOfAccountsRepo.GetDataDictCollection(sql).ToList();
        }
        public async Task<List<Dictionary<string, object>>> GetCOAList()
        {
            string sql = $@"
							WITH CountCTE AS (
                                SELECT ParentID, COUNT(*) AS ChildCount
                                FROM viewCOA
                                GROUP BY ParentID
                            ),
                            RecursiveCTE AS (
	                            SELECT 
		                            *
	                            FROM viewCOA COA
	                            WHERE ParentID = 0
    
                                UNION ALL
    
                                SELECT TV.*
                                FROM viewCOA TV
                                JOIN RecursiveCTE RC ON TV.ParentID = RC.COAID
                            )
                            SELECT R.*, ISNULL(C.ChildCount, 0) AS FinalChildCount
                            FROM RecursiveCTE R
                            LEFT JOIN CountCTE C ON R.COAID = C.ParentID
            ";

            return ChartOfAccountsRepo.GetDataDictCollection(sql).ToList();
        }
        public async Task<List<Dictionary<string, object>>> GetCOAReportList()
        {
            string sql = $@"
							WITH RecursiveCTE AS (
                            SELECT 
                                COAID AS ID,
                                ParentID,
                                CAST(AccountCode AS NVARCHAR(MAX)) + '-' + AccountName AS Type,
                                CAST('' AS NVARCHAR(MAX)) AS PrimaryGLCode,
                                CAST('' AS NVARCHAR(MAX)) AS PrimaryGLName,
                                CAST('' AS NVARCHAR(MAX)) AS ControlGLCode,
                                CAST('' AS NVARCHAR(MAX)) AS ControlGLName,
                                CAST('' AS NVARCHAR(MAX)) AS SubControlGLCode,
                                CAST('' AS NVARCHAR(MAX)) AS SubControlGLName,
                                CAST('' AS NVARCHAR(MAX)) AS LedgerCode,
                                CAST('' AS NVARCHAR(MAX)) AS LedgerName,
                                IsActive,
                                StartDate,
		                        CreatedBy,
		                        CreatedDate,
                                1 AS Level,
								CategoryID
                            FROM ChartOfAccounts
                            WHERE CategoryID = 1

                            UNION ALL

                            SELECT 
                                T.COAID AS ID,
                                T.ParentID,
                                RC.Type AS Type,
                                CASE WHEN RC.Level = 1 THEN T.AccountCode ELSE RC.PrimaryGLCode END AS PrimaryGLCode,
                                CASE WHEN RC.Level = 1 THEN T.AccountName ELSE RC.PrimaryGLName END AS PrimaryGLName,
                                CASE WHEN RC.Level = 2 THEN T.AccountCode ELSE RC.ControlGLCode END AS ControlGLCode,
                                CASE WHEN RC.Level = 2 THEN T.AccountName ELSE RC.ControlGLName END AS ControlGLName,
                                CASE WHEN RC.Level = 3 THEN T.AccountCode ELSE RC.SubControlGLCode END AS SubControlGLCode,
                                CASE WHEN RC.Level = 3 THEN T.AccountName ELSE RC.SubControlGLName END AS SubControlGLName,
                                CASE WHEN RC.Level = 4 THEN T.AccountCode ELSE RC.LedgerCode END AS LedgerCode,
                                CASE WHEN RC.Level = 4 THEN T.AccountName ELSE RC.LedgerName END AS LedgerName,
                                T.IsActive,
                                T.StartDate,
		                        T.CreatedBy,
		                        T.CreatedDate,
                                RC.Level + 1 AS Level,
                                T.CategoryID
                            FROM ChartOfAccounts T
                            JOIN RecursiveCTE RC ON T.ParentID = RC.ID
                        )
                        SELECT DISTINCT 
                            Type,
                            PrimaryGLCode,
                            PrimaryGLName,
                            ControlGLCode,
                            ControlGLName,
                            SubControlGLCode,
                            SubControlGLName,
                            LedgerCode,
                            LedgerName,
	                        CASE WHEN IsActive = 1 THEN 'Active' ELSE 'InActive' END Status,
	                        StartDate,
	                        CreatedBy,
	                        Emp.EmployeeCode,
	                        FullName,
	                        Level,
                            CategoryID
                        FROM RecursiveCTE
                        INNER JOIN HRMS..viewEmployeeUserMapData Emp on Emp.UserID = CreatedBy
                        ORDER BY Type;

            ";

            return ChartOfAccountsRepo.GetDataDictCollection(sql).ToList();
        }

        public async Task<ChartOfAccountsDto> SaveChanges(ChartOfAccountsDto model)
        {
            using var unitOfWork = new UnitOfWork();
            try
            {
                var listCOA = ChartOfAccountsRepo.GetAllList();
               // var existingModel = ChartOfAccountsRepo.Get(model.COAID);
                var existingModel = listCOA.Find(c=> c.COAID ==model.COAID);
                bool isAccExists = false;

                if (existingModel.IsNull() || model.COAID.IsZero())
                {
                    var modelWithAccCode = listCOA.FirstOrDefault(c=> c.AccountCode == model.AccountCode);
                    if (modelWithAccCode != null)
                    { 
                        int childCount = model.FinalChildCount;
                        childCount++;

                        isAccExists = true;
                        string accCode = GenerateCOAAccountNumber(model.AccountCode, model.CategoryShortCode, childCount);
                        while (isAccExists)
                        {
                            modelWithAccCode = listCOA.FirstOrDefault(c => c.AccountCode == accCode);
                            if(modelWithAccCode == null)
                            {
                                isAccExists = false;
                                model.AccountCode = accCode;
                            }
                            else
                            {
                                childCount++;
                                accCode = GenerateCOAAccountNumber(accCode, model.CategoryShortCode, childCount);
                            }
                        }
                    }

                    model.SetAdded();
                    SetCOAID(model);
                }
                else
                {
                    model.CreatedBy = existingModel.CreatedBy;
                    model.CreatedDate = existingModel.CreatedDate;
                    model.CreatedIP = existingModel.CreatedIP;
                    model.RowVersion = existingModel.RowVersion;
                    model.ParentID = existingModel.ParentID;
                    model.CategoryID = existingModel.CategoryID;
                    model.SetModified();
                }

                var saveModel = model.MapTo<ChartOfAccounts>();
                saveModel.CompanyID = AppContexts.User.CompanyID;
                SetAuditFields(saveModel);
                ChartOfAccountsRepo.Add(saveModel);
                unitOfWork.CommitChangesWithAudit();
                return saveModel.MapTo<ChartOfAccountsDto>();
            }
            catch (Exception ex)
            {
                return new ChartOfAccountsDto();
            }

        }

        public string GenerateCOAAccountNumber(string accountCode, string categoryShortCode, int childCount)
        {
            try
            {
                string accSequence = string.Empty;
                string childSeq = (childCount + 1).ToString();
                string T = string.Empty,
                        P = string.Empty,
                        C = string.Empty,
                        S = string.Empty,
                        L = string.Empty;

                if (categoryShortCode.Equals("[T]"))
                {
                    childSeq = childSeq.PadLeft(2, '0');
                }
                else
                {
                    childSeq = childSeq.PadLeft(3, '0');
                }

                if (!categoryShortCode.Equals("[T]"))
                {
                    string[] parts = accountCode.Split('-');
                    if (parts.Count() == 5)
                    {
                        T = parts[0];
                        P = parts[1];
                        C = parts[2];
                        S = parts[3];
                        L = parts[4];
                    }
                }

                if (categoryShortCode.Equals("[T]"))
                {
                    accSequence = accountCode + "-" + childSeq + "-000-000-000";
                }
                else if (categoryShortCode.Equals("[P]"))
                {
                    accSequence = T + "-" + P + '-' + childSeq + '-' + S + '-' + L;
                }
                else if (categoryShortCode.Equals("[C]"))
                {
                    accSequence = T + '-' + P + '-' + C + '-' + childSeq + '-' + L;
                }
                else if (categoryShortCode.Equals("[S]"))
                {
                    accSequence = T + '-' + P + '-' + C + '-' + S + '-' + childSeq;
                }

                return accSequence;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        private void SetCOAID(ChartOfAccountsDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("ChartOfAccounts", AppContexts.User.CompanyID);
            obj.COAID = code.MaxNumber;
        }
    }
}


