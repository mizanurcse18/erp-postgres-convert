using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.Manager.Implementations
{
    public class DepartmentManager : ManagerBase, IDepartmentManager
    {

        private readonly IRepository<Department> DepartmentRepo;
        private readonly IRepository<Division> DivisionRepo;
        public DepartmentManager(IRepository<Department> departmentRepo, IRepository<Division> divisionRepo)
        {
            DepartmentRepo = departmentRepo;
            DivisionRepo = divisionRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetDepartmentListDic()
        {
            string sql = $@"SELECT Dept.*, Dv.DivisionName
	                        ,CASE 
		                        WHEN Emp.DepartmentID IS NULL
			                        THEN CAST(1 AS BIT)
		                        ELSE CAST(0 AS BIT)
		                        END IsRemovable
                        FROM Department Dept
                        LEFT JOIN Division Dv ON Dept.DivisionID = Dv.DivisionID
                        LEFT JOIN (
	                        SELECT DISTINCT DepartmentID
	                        FROM Employment
	                        ) Emp ON Dept.DepartmentID = Emp.DepartmentID
                        ORDER BY Dept.DepartmentID DESC";
            var listDict = DepartmentRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }
        public async Task<Dictionary<string, object>> GetDepartment(int DepartmentID)
        {

            string sql = $@"SELECT D.*, Dv.DivisionName FROM Department D 
                            LEFT JOIN Division Dv ON D.DivisionID = Dv.DivisionID WHERE D.DepartmentID={DepartmentID}";

            var reg = DepartmentRepo.GetData(sql);
            return await Task.FromResult(reg);
        }


        public async Task Delete(int DepartmentID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var departmentEnt = DepartmentRepo.Entities.Where(x => x.DepartmentID == DepartmentID).FirstOrDefault();

                departmentEnt.SetDeleted();
                DepartmentRepo.Add(departmentEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public Task<DepartmentDto> SaveChanges(DepartmentDto departmentDto)
        {
            //check duplicates
            var isExistsName = DepartmentRepo.Entities.FirstOrDefault(x => x.DepartmentID != departmentDto.DepartmentID && x.DepartmentName.ToLower() == departmentDto.DepartmentName.ToLower() && x.DivisionID == departmentDto.DivisionID.ToString()).MapTo<Department>();
            if (isExistsName.IsNotNull())
            {
                departmentDto.DepartmentNameError = "Department Name already exists by this division.";
                return Task.FromResult(departmentDto);
            }

            var isExistsCode = DepartmentRepo.Entities.FirstOrDefault(x => x.DepartmentID != departmentDto.DepartmentID && x.DepartmentCode.ToLower() == departmentDto.DepartmentCode.ToLower() && x.DivisionID == departmentDto.DivisionID.ToString()).MapTo<Department>();
            if (isExistsCode.IsNotNull())
            {
                departmentDto.DepartmentCodeError = "Department Code already exists by this division.";
                return Task.FromResult(departmentDto);
            }

            var isExistsDiv = DepartmentRepo.Entities.FirstOrDefault(x => x.DepartmentID != departmentDto.DepartmentID && x.DepartmentName.ToLower() == departmentDto.DepartmentName.ToLower() && x.DepartmentCode.ToLower() == departmentDto.DepartmentCode.ToLower() && x.DivisionID == departmentDto.DivisionID.ToString()).MapTo<Department>();
            if (isExistsDiv.IsNotNull())
            {
                departmentDto.DepartmentNameError = "Department already exists by this division.";
                return Task.FromResult(departmentDto);
            }

            using (var unitOfWork = new UnitOfWork())
            {
                var existUser = DepartmentRepo.Entities.SingleOrDefault(x => x.DepartmentID == departmentDto.DepartmentID).MapTo<Department>();

                if (existUser.IsNull() || departmentDto.DepartmentID.IsZero())
                {
                    departmentDto.SetAdded();
                    SetNewUserID(departmentDto);
                }
                else
                {
                    departmentDto.SetModified();
                }

                var departmentEnt = departmentDto.MapTo<Department>();
                SetAuditFields(departmentEnt);
                DepartmentRepo.Add(departmentEnt);
                unitOfWork.CommitChangesWithAudit();
            }
            return Task.FromResult(departmentDto);
        }

        public async Task<UploadGenericResponseDto<DepartmentDto>> SaveChangesUploadDepartment(IFormFile file)
        {
            UploadGenericResponseDto<DepartmentDto> response = new UploadGenericResponseDto<DepartmentDto>();
            List<DepartmentDto> dList = new List<DepartmentDto>();
            bool stat = true;

            SaveFileDescription fileDesc = new SaveFileDescription();
            string filePath = string.Empty;
            try
            {
                #region Save file in Disk & convert to ObjectList from Excel and Dispose file
                fileDesc = await UploadUtil.SaveFileInDisk(file, "department");
                filePath = fileDesc.FullPath;

                if (!fileDesc.FileExtention.Equals(".xls") && !fileDesc.FileExtention.Equals(".xlsx"))
                {
                    File.Delete(filePath);
                    return response;
                }

                var worksheet = UploadUtil.ConvertFileToExcel(filePath);
                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;

                if(rowCount < 1 || colCount < 1)
                {
                    File.Delete(filePath);
                    return response;
                }

                dList = new GenericParse<DepartmentDto>().ParseExcelFileToObjectList(worksheet, rowCount, colCount);
                worksheet.Dispose();
                #endregion
            }
            catch (Exception ex)
            {
                File.Delete(filePath);
                return response;
            }
            #region Check duplicate dept name and dept code in uploded excel 
            var duplicateDeptNameRecords = dList.GroupBy(d => d.DepartmentName).Where(d => d.Count() > 1).Select(d => d.Key).ToList();
            var duplicateDeptCodeRecords = dList.GroupBy(d => d.DepartmentCode).Where(d => d.Count() > 1).Select(d => d.Key).ToList();

            if (duplicateDeptNameRecords.Any())
            {
                dList.Where(e => duplicateDeptNameRecords.Contains(e.DepartmentName)).Select(d => d.DepartmentNameError = "Duplicate Department Name").ToList();
                stat = false;
            }
            if (duplicateDeptCodeRecords.Any())
            {
                dList.Where(e => duplicateDeptCodeRecords.Contains(e.DepartmentCode)).Select(d => d.DepartmentCodeError = "Duplicate Department Code").ToList();
                stat = false;
            }
            #endregion

            #region Get all Division and Department list from DB.
            var allDivision = DivisionRepo.GetAllListAsync().Result;
            var deptList = DepartmentRepo.GetAllListAsync().Result;
            #endregion

            #region Get all division name, department name and code
            var divisionsName = allDivision.Select(div => div.DivisionName).ToList();
            var allDeptName = deptList.Select(div => div.DepartmentName).ToList();
            var allDeptCode = deptList.Select(div => div.DepartmentCode).ToList();
            #endregion

            #region Get all division name exists in the file and check existance from DB list
            var distinctDivisionName = dList.GroupBy(d => d.DivisionName).Select(x => x.FirstOrDefault()).Select(x => x.DivisionName).ToList();

            var nonExistsDivision = distinctDivisionName.Select(d => d).Except(divisionsName).ToList();

            if (nonExistsDivision.Any())
            {
                stat = false;
                dList.Where(e => nonExistsDivision.Contains(e.DivisionName)).Select(d => d.DivisionNameError = "Division name not found.").ToList();
            }
            #endregion

            #region Get all department name exists in the file and check existance from DB list
            var distinctNewDepartmentName = dList.Where(d => Convert.ToInt32(d.DepartmentID) == 0).Select(d => d.DepartmentName).ToList().GroupBy(d => d).Select(x => x.FirstOrDefault()).ToList();

            if (distinctNewDepartmentName.Any() && !duplicateDeptNameRecords.Any())
            {
                var exitsDeptName = allDeptName.Where(s => distinctNewDepartmentName.Contains(s)).ToList();

                if (exitsDeptName.Any())
                {
                    stat = false;
                    dList.Where(d => exitsDeptName.Contains(d.DepartmentName) && Convert.ToInt32(d.DepartmentID) == 0).Select(d => d.DepartmentNameError = "Department Name already exists.").ToList();
                }
            }
            #endregion

            #region Get all department code exists in the file and check existance from DB list
            var distinctNewDepartmentCode = dList.Where(d => Convert.ToInt32(d.DepartmentID) == 0).Select(d => d.DepartmentCode).ToList().GroupBy(d => d).Select(x => x.FirstOrDefault()).ToList();
            if (distinctNewDepartmentCode.Any() && !duplicateDeptCodeRecords.Any())
            {
                var exitsDeptCode = allDeptCode.Where(s => distinctNewDepartmentCode.Contains(s)).ToList();
                if (exitsDeptCode.Any())
                {
                    stat = false;
                    dList.Where(d => exitsDeptCode.Contains(d.DepartmentCode) && Convert.ToInt32(d.DepartmentID) == 0).Select(d => d.DepartmentCodeError = "Department Code already exists.").ToList();
                }
            }
            #endregion

            // If any point it does not meet the validation
            if (!stat)
            {
                File.Delete(filePath);
                response.dList = dList;
                return response;
            }

            List<Department> depList = new List<Department>();
            using (var unitOfWork = new UnitOfWork())
            {
                foreach (var d in dList)
                {
                    var existDept = DepartmentRepo.Entities.SingleOrDefault(x => x.DepartmentID == d.DepartmentID).MapTo<Department>();

                    if(!d.DepartmentID.IsZero() && existDept == null)
                    {                        
                        File.Delete(filePath);
                        dList.Where(dept => Convert.ToInt32(dept.DepartmentID) == d.DepartmentID).Select(dept => dept.DepartmentNameError = "Department not found with this ID.").ToList();
                        response.dList = dList;
                        unitOfWork.Dispose();
                        return response;
                    }

                    if (existDept.IsNull() && d.DepartmentID.IsZero())
                    {
                        d.SetAdded();
                        SetNewUserID(d);
                    }
                    else
                    {
                        d.SetModified();
                        d.RowVersion = existDept.RowVersion;
                        d.CreatedBy = existDept.CreatedBy;
                        d.CreatedIP = existDept.CreatedIP;
                        d.CreatedDate = existDept.CreatedDate;
                    }
                    // Set Division ID according to the list retured from DB
                    d.DivisionID = allDivision.Where(div => div.DivisionName == d.DivisionName).FirstOrDefault().DivisionID;
                    
                    var departmentEnt = d.MapTo<Department>();
                    SetAuditFields(departmentEnt);
                    depList.Add(departmentEnt);
                }
                DepartmentRepo.AddRange(depList);

                #region Save file in the DB
                Attachment attachment = new Attachment();
                attachment.FileName = fileDesc.SavedFileName;
                attachment.OriginalName = fileDesc.FileOriginalName;
                attachment.Type = fileDesc.FileExtention;
                attachment.FilePath = fileDesc.FileRelativePath;
                attachment.Size = fileDesc.FileSize / 1000;
                SetAttachmentNewId(attachment);
                SaveSingleAttachment(attachment.FUID, attachment.FilePath, attachment.FileName, attachment.Type, attachment.OriginalName, 0, "DepartmentUpload", false, attachment.Size, 0, false, attachment.Description ?? "");
                #endregion
                unitOfWork.CommitChangesWithAudit();

                response.dList = dList;
                response.uploadStatus = true;
            }
            return await Task.FromResult(response);
        }

        public static string SaveSingleAttachment(int FUID, string FilePath, string FileName, string FileType, string OriginalName, int ReferenceID, string TableName, bool IsDeleted, decimal Size, int ParentFUID = 0, bool IsFolder = false, string description = "", bool IsUpdated = false)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"{AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SaveSingleAttachment @0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15", FUID, FilePath, FileName, FileType, OriginalName, ReferenceID, TableName, ParentFUID, IsFolder, AppContexts.User.CompanyID, AppContexts.User.UserID, AppContexts.User.IPAddress, IsDeleted, Size, description, IsUpdated);

            return result.ToString();
        }

        private void SetAttachmentNewId(Attachment attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }

        private void SetNewUserID(DepartmentDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("Department", AppContexts.User.CompanyID);
            obj.DepartmentID = code.MaxNumber;
        }

        public async Task<List<Dictionary<string, object>>> GetExportDepartments(string whereCondition)
        {
            string where = whereCondition.IsNotNullOrEmpty() ? @$"WHERE {whereCondition}" : "";
            string sql = $@"select dept.DepartmentID, dept.DepartmentName,dept.DepartmentCode,div.DivisionID,div.DivisionName,div.DivisionCode from Department Dept LEFT JOIN Division Div ON Div.DivisionID = Dept.DivisionID {where}";
            var data = DepartmentRepo.GetDataDictCollection(sql);

            return await Task.FromResult(data.ToList());
        }
    }
}
