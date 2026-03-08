using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.EntityBase;
using DAL.Core.Extension;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using Manager.Core;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.Manager
{
    public class CompanyLeavePolicyManager : ManagerBase, ICompanyLeavePolicyManager
    {
        private readonly IRepository<CompanyLeavePolicy> CompanyLeavePolicyRepo;
        //readonly IModelAdapter Adapter;
        public CompanyLeavePolicyManager(IRepository<CompanyLeavePolicy> clPolicyRepo
            )
        {
            CompanyLeavePolicyRepo = clPolicyRepo;
        }

        public async Task<List<CompanyLeavePolicyDto>> GetCompanyLeavePolicyTables()
        {
            var clPolicies = await CompanyLeavePolicyRepo.GetAllListAsync(x => x.CompanyID == AppContexts.User.CompanyID.ToString());
            return clPolicies.MapTo<List<CompanyLeavePolicyDto>>();
        }

        public async Task<List<CompanyLeavePolicyDto>> GetCompanyLeavePolicy(int finYearID, int empStatusID)
        {
            //var clPolicies = await CompanyLeavePolicyRepo.GetAllListAsync(x => x.FinancialYearID == finYearID && x.EmployeeStatusID == empStatusID);
            //return clPolicies.MapTo<List<CompanyLeavePolicyDto>>();

            //     string sql = $@"SELECT CLP.CLPolicyID, CLP.FinancialYearID, CLP.EmployeeStatusID, CLP.LeaveCategoryID, CLP.LeaveInDays, CLP.Remarks,CLP.CompanyID , SV.SystemVariableCode AS LeaveCategoryName,FY.Year AS Year, SVE.SystemVariableCode AS EmployeeStatusName, 1 AS IsExists
            //                     FROM CompanyLeavePolicy CLP
            //                     LEFT JOIN Security..SystemVariable SV ON CLP.LeaveCategoryID = SV.SystemVariableID
            //LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID = CLP.FinancialYearID
            //                     LEFT JOIN Security..SystemVariable SVE ON CLP.EmployeeStatusID = SVE.SystemVariableID
            //                     WHERE SV.EntityTypeName IN('LeaveCategory', 'EmployeeJobStatus') AND CLP.FinancialYearID={finYearID} AND CLP.EmployeeStatusID={empStatusID}";

            string sql = $@"SELECT ISNULL(CLP.CLPolicyID,ROW_NUMBER()OVER(ORDER BY SVE.SystemVariableID)) CLPolicyID
	                            ,FY.FinancialYearID
	                            ,SVE.SystemVariableID EmployeeStatusID
	                            ,SV.SystemVariableID LeaveCategoryID
	                            ,CLP.LeaveInDays
	                            ,CLP.Remarks
	                            ,ISNULL(CLP.CompanyID,'{AppContexts.User.CompanyID}') CompanyID
	                            ,SV.SystemVariableCode AS LeaveCategoryName
	                            ,FY.Year AS Year
	                            ,SVE.SystemVariableCode AS EmployeeStatusName
	                            ,1 AS IsExists
                            FROM Security..SystemVariable SV
                            LEFT JOIN CompanyLeavePolicy CLP ON CLP.LeaveCategoryID = SV.SystemVariableID
	                            AND CLP.FinancialYearID = {finYearID}
	                            AND CLP.EmployeeStatusID = {empStatusID}
                            LEFT JOIN Security..FinancialYear FY ON  FY.FinancialYearID = {finYearID}
                            LEFT JOIN Security..SystemVariable SVE ON SVE.SystemVariableID = {empStatusID}
                            WHERE SV.EntityTypeName IN ('LeaveCategory')";
            var policyList = new List<CompanyLeavePolicyDto>();
            policyList = CompanyLeavePolicyRepo.GetDataModelCollection<CompanyLeavePolicyDto>(sql);


            await Task.CompletedTask;
            return policyList;
        }


        public async Task<CompanyLeavePolicyDto> SaveChanges(CompanyLeavePolicyDto master, List<CompanyLeavePolicyDto> childs = null)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                if (childs.IsNull()) childs = new List<CompanyLeavePolicyDto>();
                
                foreach (var child in childs)
                {
                    var existsPolicy = CompanyLeavePolicyRepo.Entities.SingleOrDefault(x => x.FinancialYearID == child.FinancialYearID && x.EmployeeStatusID == child.EmployeeStatusID && x.LeaveCategoryID == child.LeaveCategoryID).MapTo<CompanyLeavePolicy>();
                    if (existsPolicy.IsNull())
                    {
                        child.SetAdded();
                        child.FinancialYearID = master.FinancialYearID;
                        child.EmployeeStatusID = master.EmployeeStatusID;
                        SetNewId(child);
                    }
                    else
                    {
                        child.SetModified();

                        child.RowVersion = existsPolicy.RowVersion;
                        child.CreatedBy = existsPolicy.CreatedBy;
                        child.CreatedDate = existsPolicy.CreatedDate;
                        child.CreatedIP = existsPolicy.CreatedIP;
                    }

                }
                var childsEnt = childs.MapTo<List<CompanyLeavePolicy>>();

                //Set Audti Fields Data
                SetAuditFields(childsEnt);

                CompanyLeavePolicyRepo.AddRange(childsEnt);
                unitOfWork.CommitChangesWithAudit();

            }
            await Task.CompletedTask;

            return master;
        }

        public async Task RemoveCompanyLeavePolicy(int FinancialYearID, int EmployeeStatusID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var childEnt = CompanyLeavePolicyRepo.Entities.Where(x => x.FinancialYearID == FinancialYearID && x.EmployeeStatusID == EmployeeStatusID).ToList();
                childEnt.ChangeState(ModelState.Deleted);

                CompanyLeavePolicyRepo.AddRange(childEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }

        private void SetNewId(CompanyLeavePolicyDto clPolicyTable)
        {
            if (!clPolicyTable.IsAdded) return;
            var code = GenerateSystemCode("CompanyLeavePolicy", AppContexts.User.CompanyID);
            clPolicyTable.CLPolicyID = code.MaxNumber;
        }

        public async Task<List<CompanyLeavePolicyDto>> GetCompanyLeavePolicyListWithDetails()
        {
            var policyList = new List<CompanyLeavePolicyDto>();
            var sql = $@"   SELECT 
	                            CLP.CLPolicyID, 
	                            CLP.FinancialYearID, 
	                            CLP.EmployeeStatusID,
	                            CLP.LeaveCategoryID, 
	                            CLP.LeaveInDays, 
	                            CLP.Remarks,
	                            CLP.CompanyID , 
	                            SV.SystemVariableCode AS LeaveCategoryName,
	                            FY.Year AS Year, 
	                            SVE.SystemVariableCode AS EmployeeStatusName
	                            ,CASE 
		                            WHEN DA.FinancialYearID IS NULL
			                            THEN CAST(1 AS BIT)
		                            ELSE CAST(0 AS BIT)
		                            END IsRemovable
                            FROM CompanyLeavePolicy CLP
                            LEFT JOIN Security..SystemVariable SV ON CLP.LeaveCategoryID = SV.SystemVariableID
                            LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID = CLP.FinancialYearID
                            LEFT JOIN Security..SystemVariable SVE ON CLP.EmployeeStatusID = SVE.SystemVariableID
                            LEFT JOIN ( 		
		                            SELECT DISTINCT
			                            FinancialYearID,LeaveCategoryID,EmployeeStatusID 
		                            FROM 
			                            EmployeeLeaveAccount ELA	
			                            INNER JOIN ViewALLEmployee VAE ON VAE.EmployeeID = ELA.EmployeeID
		                            ) 
		                            DA ON DA.FinancialYearID =CLP.FinancialYearID AND DA.LeaveCategoryID = CLP.LeaveCategoryID AND DA.EmployeeStatusID = CLP.EmployeeStatusID
                            WHERE SV.EntityTypeName IN('LeaveCategory', 'EmployeeJobStatus') AND CLP.CompanyID = '{AppContexts.User.CompanyID}'";

            //GROUP BY FY.Year,SVE.SystemVariableCode, CLP.FinancialYearID, CLP.EmployeeStatusID";

            //var listDict = CompanyLeavePolicyRepo.GetDataDictCollection(sql);

            policyList = CompanyLeavePolicyRepo.GetDataModelCollection<CompanyLeavePolicyDto>(sql);
            var customList = policyList.GroupBy(c => new { c.Year, c.EmployeeStatusName, c.FinancialYearID, c.EmployeeStatusID })
                .Select(chld => new CompanyLeavePolicyDto()
                {
                    Year = chld.Key.Year,
                    EmployeeStatusName = chld.Key.EmployeeStatusName,
                    FinancialYearID = chld.Key.FinancialYearID,
                    EmployeeStatusID = chld.Key.EmployeeStatusID,
                    IsRemovable = chld.ToList()[0].IsRemovable,
                    LeavePolicyList = chld.ToList()
                }).ToList();

            await Task.CompletedTask;
            return customList;
        }

        public async Task<List<CompanyLeavePolicyDto>> GetGenerateChildList(CompanyLeavePolicyDto clPolicy)
        {
            var policyList = new List<CompanyLeavePolicyDto>();

            //var existsPolicy = CompanyLeavePolicyRepo.Entities.SingleOrDefault(x => x.FinancialYearID == clPolicy.FinancialYearID && x.EmployeeStatusID == clPolicy.EmployeeStatusID).MapTo<CompanyLeavePolicy>();
            string sql = "";
            if(clPolicy.IsExists)
            {                
                sql = $@"SELECT 
	                        ISNULL(CLP.CLPolicyID,ROW_NUMBER() OVER (
	                        ORDER BY SV.SystemVariableID
                            )) CLPolicyID
	                        ,ISNULL(CLP.FinancialYearID,{clPolicy.FinancialYearID}) FinancialYearID
	                        ,ISNULL(CLP.EmployeeStatusID,{clPolicy.EmployeeStatusID}) EmployeeStatusID
	                        ,SV.SystemVariableID LeaveCategoryID
	                        ,ISNULL(CLP.LeaveInDays,0) LeaveInDays
	                        ,CLP.Remarks
	                        ,CLP.CompanyID
	                        ,SV.SystemVariableCode AS LeaveCategoryName
	                        ,FY.Year AS Year
	                        ,SVE.SystemVariableCode AS EmployeeStatusName
	                        ,1 AS IsExists
                        FROM
	                        Security..SystemVariable SV
	                        LEFT JOIN CompanyLeavePolicy CLP ON CLP.LeaveCategoryID = SV.SystemVariableID 
	                        AND CLP.FinancialYearID = {clPolicy.FinancialYearID} AND CLP.EmployeeStatusID = {clPolicy.EmployeeStatusID}
	                        LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID = {clPolicy.FinancialYearID} --CLP.FinancialYearID
	                        LEFT JOIN Security..SystemVariable SVE ON SVE.SystemVariableID = {clPolicy.EmployeeStatusID}
	                        WHERE SV.EntityTypeID = {(int)Util.SystemVariableEntityType.LeaveCategory}";
            }
            else
            {
                sql = $@"SELECT  ROW_NUMBER() OVER (
	                        ORDER BY SystemVariableID
                           )  CLPolicyID, 0 AS FinancialYearID, 0 AS EmployeeStatusID, SV.SystemVariableID AS LeaveCategoryID, 0 AS LeaveInDays, '' AS Remarks,SV.CompanyID, SV.SystemVariableCode AS LeaveCategoryName
                           FROM Security..SystemVariable SV WHERE SV.EntityTypeName = 'LeaveCategory'";
            }
            policyList = CompanyLeavePolicyRepo.GetDataModelCollection<CompanyLeavePolicyDto>(sql);


            await Task.CompletedTask;
            return policyList;
        }

        public async Task<bool> GetExistingPolicy(CompanyLeavePolicyDto clPolicy)
        {
            bool isExists = CompanyLeavePolicyRepo.Entities.Count(x => x.FinancialYearID == clPolicy.FinancialYearID && x.EmployeeStatusID == clPolicy.EmployeeStatusID) > 0;
            await Task.CompletedTask;
            return isExists;
        }

    }
}
