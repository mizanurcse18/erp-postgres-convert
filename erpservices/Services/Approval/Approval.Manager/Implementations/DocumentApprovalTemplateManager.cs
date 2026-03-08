using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Approval.DAL.Entities;
using Approval.Manager.Dto;
using Approval.Manager.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Core.Util;

namespace Approval.Manager.Implementations
{
    public class DocumentApprovalTemplateManager : ManagerBase, IDocumentApprovalTemplateManager
    {

        private readonly IRepository<DocumentApprovalTemplate> DocumentApprovalTemplateRepo;
        private readonly IRepository<DocumentApprovalMaster> DocumentApprovalMasterRepoMain;
        public DocumentApprovalTemplateManager(IRepository<DocumentApprovalTemplate> documentApprovalMasterRepo, IRepository<DocumentApprovalMaster> documentApprovalMasterRepoMain)
        {
            DocumentApprovalTemplateRepo = documentApprovalMasterRepo;
            DocumentApprovalMasterRepoMain = documentApprovalMasterRepoMain;
        }


        public async Task<(bool, string)> SaveChanges(DocumentApprovalTemplateDto DocumentApprovalTemplate)
        {
            var existingDA = DocumentApprovalTemplateRepo.Entities.Where(x => x.DATID == DocumentApprovalTemplate.DATID).SingleOrDefault();
            
            var existInDocumentApprovalMasterTable = DocumentApprovalMasterRepoMain.Entities.Where(x => x.TemplateID == DocumentApprovalTemplate.DATID).SingleOrDefault();

            var masterModel = new DocumentApprovalTemplate();

            if (existInDocumentApprovalMasterTable.IsNotNull())
            {
                var existingKeywordsList = DocumentApprovalTemplateRepo.Entities
                    .Where(x => x.DATID == DocumentApprovalTemplate.DATID)
                    .Select(x => x.Keywords)
                    .FirstOrDefault();
                masterModel.Keywords = existingKeywordsList;
            }
            else
            {
                masterModel.Keywords = DocumentApprovalTemplate.Keywords;
            }

            masterModel.DATName = DocumentApprovalTemplate.DATName;
            masterModel.TemplateBody = DocumentApprovalTemplate.TemplateBody;
            masterModel.CategoryType = DocumentApprovalTemplate.CategoryType;
            
            using (var unitOfWork = new UnitOfWork())
            {
                if (DocumentApprovalTemplate.DATID.IsZero() && existingDA.IsNull())
                {
                    masterModel.SetAdded();
                    SetDocumentApprovalTemplateNewId(masterModel);
                    DocumentApprovalTemplate.DATID = (int)masterModel.DATID;

                }
                else
                {
                    masterModel.CreatedBy = existingDA.CreatedBy;
                    masterModel.CreatedDate = existingDA.CreatedDate;
                    masterModel.CreatedIP = existingDA.CreatedIP;
                    masterModel.RowVersion = existingDA.RowVersion;
                    masterModel.DATID = DocumentApprovalTemplate.DATID;
                    masterModel.SetModified();

                    
                }

                SetAuditFields(masterModel);


                DocumentApprovalTemplateRepo.Add(masterModel);

                unitOfWork.CommitChangesWithAudit();

               
            }
            await Task.CompletedTask;

            return (true, $"DocumentApprovalTemplate Submitted Successfully"); ;
        }

        private void SetDocumentApprovalTemplateNewId(DocumentApprovalTemplate master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("DocumentApprovalTemplate", AppContexts.User.CompanyID);
            master.DATID = code.MaxNumber;
        }



        private string GenerateDocumentApprovalTemplateReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/DocumentApprovalTemplate/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{ GenerateSystemCode("DocumentApprovalTemplateRefNo", AppContexts.User.CompanyID).SystemCode}";
            return format;
        }
        

        public GridModel GetDocumentApprovalTemplateList(GridParameter parameters)
        {
            
            string sql = $@"SELECT D.*
                            FROM DocumentApprovalTemplate D
                                    ";
            var result = DocumentApprovalTemplateRepo.LoadGridModel(parameters, sql);
            return result;
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetDocumentApprovalTemplateList()
        {
            string sql = $@"SELECT D.*,SV.SystemVariableCode CategoryTypeName
	                        ,CASE 
		                        WHEN DM.TemplateID IS NULL
			                        THEN CAST(1 AS BIT)
		                        ELSE CAST(0 AS BIT)
		                        END IsRemovable
                        FROM DocumentApprovalTemplate D
                        LEFT JOIN DocumentApprovalMaster DM ON DM.TemplateID=D.DATID
                        LEFT JOIN Security..SystemVariable SV ON D.CategoryType = SV.SystemVariableID
                        ORDER BY D.DATID DESC";
            var listDict = DocumentApprovalTemplateRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }


        public async Task<DocumentApprovalTemplateDto> GetDocumentApprovalTemplate(int DATID)
        {
            string sql = $@"SELECT D.*,SV.SystemVariableCode CategoryTypeName
                        from DocumentApprovalTemplate D
                        LEFT JOIN Security..SystemVariable SV ON D.CategoryType = SV.SystemVariableID
                        WHERE D.DATID={DATID}";
            var master = DocumentApprovalTemplateRepo.GetModelData<DocumentApprovalTemplateDto>(sql);
            return master;
        }
        public async Task<DocumentApprovalTemplateDto> GetTemplateWithReplacedData(int DATID)
        {
            string sql = $@"SELECT D.*,SV.SystemVariableCode CategoryTypeName
                        from DocumentApprovalTemplate D
                        LEFT JOIN Security..SystemVariable SV ON D.CategoryType = SV.SystemVariableID
                        WHERE D.DATID={DATID}";
            var master = DocumentApprovalTemplateRepo.GetModelData<DocumentApprovalTemplateDto>(sql);

            if (DATID > 0)
            {
                string sqlEmp = $@"SELECT VA.* FROM HRMS..ViewALLEmployee VA WHERE VA.EmployeeID={AppContexts.User.EmployeeID}";
                List<Dictionary<string, object>> collection = DocumentApprovalTemplateRepo.GetDataDictCollection(sqlEmp).ToList();
                string FullName = string.Empty, EmployeeCode = string.Empty, ConfirmDate = string.Empty, DesignationName = string.Empty, DepartmentName = string.Empty, DivisionName = string.Empty, Nationality = string.Empty,
                    PassportIssueDate = string.Empty, PassportExpiryDate = string.Empty, PassportNumber = string.Empty, GenderHeShe = string.Empty, GenderHisHer = string.Empty;
                int GenderID = 1;
                if (collection.Count > 0)
                {
                    FullName = collection[0]["FullName"].ToString();
                    EmployeeCode = collection[0]["EmployeeCode"].ToString();
                    //ConfirmDate = collection[0]["ConfirmDate"].ToString() != "" ? Convert.ToDateTime(collection[0]["ConfirmDate"].ToString()).ToString("MMM dd,yyyy") : "";
                    ConfirmDate = collection[0]["DateOfJoining"].ToString() != "" ? Convert.ToDateTime(collection[0]["DateOfJoining"].ToString()).ToString("MMM dd,yyyy") : "";
                    DesignationName = collection[0]["DesignationName"].ToString();
                    DepartmentName = collection[0]["DepartmentName"].ToString();
                    DivisionName = collection[0]["DivisionName"].ToString();
                    Nationality = collection[0]["Nationality"].ToString();
                    PassportIssueDate = collection[0]["PassportIssueDate"].ToString() != "" ?  Convert.ToDateTime(collection[0]["PassportIssueDate"].ToString()).ToString("MMM dd,yyyy") : "";
                    PassportExpiryDate = collection[0]["PassportExpiryDate"].ToString() != "" ?  Convert.ToDateTime(collection[0]["PassportExpiryDate"].ToString()).ToString("MMM dd,yyyy") : "";
                    PassportNumber = collection[0]["PassportNumber"].ToString();
                    GenderID = Convert.ToInt32(collection[0]["GenderID"]);
                    GenderHeShe = GenderID == 1 ? "He" : "She";
                    GenderHisHer = GenderID == 1 ? "his" : "her";
                }
                if (master.IsNotNull())
                {
                    master.TemplateBody = master.TemplateBody.Replace("{{FullName}}", FullName).Replace("{{EmployeeCode}}", EmployeeCode)
                        .Replace("{{ConfirmDate}}", ConfirmDate).Replace("{{He/She}}", GenderHeShe).Replace("{{his/her}}", GenderHisHer)
                        .Replace("{{DesignationName}}", DesignationName)
                        .Replace("{{DepartmentName}}", DepartmentName).Replace("{{DivisionName}}", DivisionName)
                        .Replace("{{Nationality}}", Nationality).Replace("{{PassportIssueDate}}", PassportIssueDate)
                        .Replace("{{PassportExpiryDate}}", PassportExpiryDate).Replace("{{PassportNo}}", PassportNumber);
                }
            }
            return master;
        }
        
        public async Task Delete(int id)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var ent = DocumentApprovalTemplateRepo.Entities.Where(x => x.DATID == id).FirstOrDefault();

                ent.SetDeleted();
                DocumentApprovalTemplateRepo.Add(ent);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }


    }



}
