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
using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HRMS.Manager.Implementations
{
    public class DivisionManager : ManagerBase, IDivisionManager
    {

        private readonly IRepository<Division> DivisionRepo;
        public DivisionManager(IRepository<Division> divisionRepo)
        {
            DivisionRepo = divisionRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetDivisionListDic()
        {
            string sql = $@"SELECT
                                div.division_id AS ""DivisionID"",
                                div.division_code AS ""DivisionCode"",
                                div.division_name AS ""DivisionName"",
                                CASE 
                                    WHEN emp.division_id IS NULL
                                        THEN TRUE
                                    ELSE FALSE
                                END AS ""IsRemovable""
                            FROM
                                division div
                            LEFT JOIN (
                                SELECT DISTINCT
                                    division_id
                                FROM
                                    employment
                            ) emp ON div.division_id = emp.division_id
                            ORDER BY
                                div.division_id DESC";
            var listDict = DivisionRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public async Task<DivisionDto> GetDivision(int DivisionID)
        {

            var dept = DivisionRepo.SingleOrDefault(x => x.DivisionID == DivisionID).MapTo<DivisionDto>();

            return await Task.FromResult(dept);
        }


        public async Task Delete(int DivisionID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var divisionEnt = DivisionRepo.Entities.Where(x => x.DivisionID == DivisionID).FirstOrDefault();

                divisionEnt.SetDeleted();
                DivisionRepo.Add(divisionEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public void SaveChanges(DivisionDto divisionDto)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existUser = DivisionRepo.Entities.SingleOrDefault(x => x.DivisionID == divisionDto.DivisionID).MapTo<Division>();

                if (existUser.IsNull() || divisionDto.DivisionID.IsZero() )
                {
                    divisionDto.SetAdded();
                    SetNewUserID(divisionDto);
                }
                else
                {
                    divisionDto.SetModified();
                }

                var divisionEnt = divisionDto.MapTo<Division>();
                SetAuditFields(divisionEnt);
                DivisionRepo.Add(divisionEnt);
                unitOfWork.CommitChangesWithAudit();
            }
        }

        private void SetNewUserID(DivisionDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("Division", AppContexts.User.CompanyID);
            obj.DivisionID = code.MaxNumber;
        }

        public async Task<List<Dictionary<string, object>>> GetExportDivisions(string whereCondition)
        {
            string where = whereCondition.IsNotNullOrEmpty() ? @$"WHERE {whereCondition}" : "";

            string sql = $@"SELECT
                                div.division_id AS ""DivisionID"",
                                div.division_name AS ""DivisionName"",
                                div.division_code AS ""DivisionCode""
                            FROM
                                division div
                            {where}";

            var data = DivisionRepo.GetDataDictCollection(sql);

            return await Task.FromResult(data.ToList());
        }

        public async Task <UploadGenericResponseDto<DivisionDto>> SaveChangesUploadDivision(IFormFile file)
        {
            UploadGenericResponseDto<DivisionDto> response = new UploadGenericResponseDto<DivisionDto>();
            List<DivisionDto> dList = new List<DivisionDto>();
            bool stat = true;
            string filePath = string.Empty;
            SaveFileDescription fileDescription = new SaveFileDescription();

            try
            {
                #region Validate File
                if (file == null || file.Length == 0)
                {
                    response.Message = "Please select a file to upload";
                    return response;
                }

                if (!file.FileName.EndsWith(".xls") && !file.FileName.EndsWith(".xlsx"))
                {
                    response.Message = "Please upload only Excel files (.xls or .xlsx)";
                    return response;
                }
                #endregion

                #region Save file in Disk & convert to ObjectList from Excel
                try
                {
                    fileDescription = await UploadUtil.SaveFileInDisk(file, "division");
                    filePath = fileDescription.FullPath;

                    var worksheet = UploadUtil.ConvertFileToExcel(filePath);
                    if (worksheet == null || worksheet.Dimension == null)
                    {
                        response.Message = "Excel file is empty or invalid";
                        File.Delete(filePath);
                        return response;
                    }

                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    if (rowCount <= 1) // Only header row
                    {
                        response.Message = "Excel file has no data rows";
                        File.Delete(filePath);
                        return response;
                    }

                    dList = new GenericParse<DivisionDto>().ParseExcelFileToObjectList(worksheet, rowCount, colCount);
                    worksheet.Dispose();

                    if (!dList.Any())
                    {
                        response.Message = "No valid division data found in Excel";
                        File.Delete(filePath);
                        return response;
                    }
                }
                catch (Exception ex)
                {
                    if (!string.IsNullOrEmpty(filePath))
                        File.Delete(filePath);
                    response.Message = "Error processing Excel file: " + ex.Message;
                    return response;
                }
                #endregion

                #region Validate Division Data
                // Validate required fields
                foreach (var division in dList)
                {
                    if (string.IsNullOrWhiteSpace(division.DivisionName))
                    {
                        division.DivisionNameError = "Division Name is required";
                        stat = false;
                    }
                    if (string.IsNullOrWhiteSpace(division.DivisionCode))
                    {
                        division.DivisionCodeError = "Division Code is required";
                        stat = false;
                    }
                }

                // Check duplicates in Excel
                var duplicateDivNameRecords = dList.GroupBy(d => d.DivisionName.Trim().ToLower())
                                                 .Where(d => d.Count() > 1)
                                                 .Select(d => d.Key)
                                                 .ToList();

                var duplicateDivCodeRecords = dList.GroupBy(d => d.DivisionCode.Trim().ToLower())
                                                 .Where(d => d.Count() > 1)
                                                 .Select(d => d.Key)
                                                 .ToList();

                if (duplicateDivNameRecords.Any())
                {
                    dList.Where(e => duplicateDivNameRecords.Contains(e.DivisionName.Trim().ToLower()))
                        .ToList()
                        .ForEach(d => d.DivisionNameError = "Duplicate Division Name");
                    stat = false;
                }

                if (duplicateDivCodeRecords.Any())
                {
                    dList.Where(e => duplicateDivCodeRecords.Contains(e.DivisionCode.Trim().ToLower()))
                        .ToList()
                        .ForEach(d => d.DivisionCodeError = "Duplicate Division Code");
                    stat = false;
                }
                #endregion

                #region Check Database Duplicates
                var existingDivisions = await DivisionRepo.GetAllListAsync();
                //var allDivNames = existingDivisions.Select(d => d.DivisionName.Trim().ToLower()).ToList();
                //var allDivCodes = existingDivisions.Select(d => d.DivisionCode.Trim().ToLower()).ToList();

                // Check new divisions against database
                var newDivisions = dList.Where(d => Convert.ToInt32(d.DivisionID) == 0);
                foreach (var div in newDivisions)
                {
                    if (existingDivisions.Exists(x=>x.DivisionName.ToLower().Equals(div.DivisionName.Trim().ToLower())))
                    {
                        div.DivisionNameError = "Division Name already exists in database";
                        stat = false;
                    }
                    if (existingDivisions.Exists(x => x.DivisionCode.ToLower().Equals(div.DivisionCode.Trim().ToLower())))
                    {
                        div.DivisionCodeError = "Division Code already exists in database";
                        stat = false;
                    }
                }

                // Validate existing division IDs
                //var existingDivisionIds = existingDivisions.Select(d => d.DivisionID).ToList();
                //foreach (var div in dList.Where(d => Convert.ToInt32(d.DivisionID) != 0))
                //{
                //    if (!existingDivisionIds.Contains(Convert.ToInt32(div.DivisionID)))
                //    {
                //        div.DivisionNameError = "Division not found with this ID";
                //        stat = false;
                //    }
                //}
                #endregion

                if (!stat)
                {
                    response.dList = dList;
                    response.Message = "Validation failed. Please check the errors and try again.";
                    File.Delete(filePath);
                    return response;
                }

                #region Save Divisions
                using (var unitOfWork = new UnitOfWork())
                {
                    try
                    {
                        List<Division> divisionsToSave = new List<Division>();

                        foreach (var d in dList)
                        {
                            // For new division uploads, DivisionID will always be 0
                            Division divisionEntity = new Division
                            {
                                DivisionName = d.DivisionName?.Trim(),
                                DivisionCode = d.DivisionCode?.Trim()
                            };

                            // Set it as a new record
                            divisionEntity.SetAdded();
                            d.SetAdded();
                            SetNewUserID(d); // This will generate a new ID
                            divisionEntity.DivisionID = d.DivisionID; // Use the generated ID

                            SetAuditFields(divisionEntity);
                            divisionsToSave.Add(divisionEntity);
                        }

                        // Save divisions
                        foreach (var division in divisionsToSave)
                        {
                            DivisionRepo.Add(division);
                        }

                        #region Save Attachment
                        Attachment attachment = new Attachment
                        {
                            FileName = fileDescription.SavedFileName,
                            OriginalName = fileDescription.FileOriginalName,
                            Type = fileDescription.FileExtention,
                            FilePath = fileDescription.FileRelativePath,
                            Size = fileDescription.FileSize / 1000
                        };

                        SetAttachmentNewId(attachment);
                        SaveSingleAttachment(
                            attachment.FUID,
                            attachment.FilePath,
                            attachment.FileName,
                            attachment.Type,
                            attachment.OriginalName,
                            0,
                            "DivisionUpload",
                            false,
                            attachment.Size,
                            0,
                            false,
                            attachment.Description ?? ""
                        );
                        #endregion

                        // Commit the transaction
                        unitOfWork.CommitChangesWithAudit();

                        // Update the response list with new IDs
                        response.dList = dList;
                        response.uploadStatus = true;
                        response.Message = $"Successfully saved {divisionsToSave.Count} divisions";
                        
                        return response;
                    }
                    catch (Exception ex)
                    {
                        response.Message = "Error saving divisions: " + ex.Message;
                        File.Delete(filePath);
                        return response;
                    }
                }
                #endregion

                return response;
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(filePath))
                    File.Delete(filePath);
                response.Message = "An unexpected error occurred: " + ex.Message;
                return response;
            }
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

       

    }
}
