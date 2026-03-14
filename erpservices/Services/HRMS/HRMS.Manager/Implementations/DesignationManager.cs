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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Implementations
{

    public class DesignationManager : ManagerBase, IDesignationManager
    {
        private readonly IRepository<Designation> DesignationRepo;
        public DesignationManager(IRepository<Designation> designationRepo)
        {
            DesignationRepo = designationRepo;
        }

        public async Task<List<DesignationDto>> GetDesignationList()
        {
            //var designation = await DesignationRepo.GetAllListAsync();
            //return designation.MapTo<List<DesignationDto>>();
            string sql = $@"SELECT
                                desg.designation_id AS ""DesignationID"",
                                desg.designation_code AS ""DesignationCode"",
                                desg.designation_name AS ""DesignationName"",
                                CASE 
                                    WHEN emp.designation_id IS NULL
                                        THEN TRUE
                                    ELSE FALSE
                                END AS ""IsRemovable""
                            FROM
                                designation desg
                            LEFT JOIN (
                                SELECT DISTINCT
                                    designation_id
                                FROM
                                    employment
                            ) emp ON desg.designation_id = emp.designation_id
                            ORDER BY
                                desg.designation_id DESC";

            return await Task.FromResult(DesignationRepo.GetDataModelCollection<DesignationDto>(sql));
        }

        public void SaveChanges(DesignationDto designationDto)
        {
            using var unitOfWork = new UnitOfWork();
            var existDesignation = DesignationRepo.Entities.SingleOrDefault(x => x.DesignationID == designationDto.DesignationID).MapTo<Designation>();

            if (existDesignation.IsNull() || existDesignation.DesignationID.IsZero() || existDesignation.IsAdded)
            {
                designationDto.SetAdded();
                SetNewDesignationID(designationDto);
            }
            else
            {
                designationDto.SetModified();
            }
            var userEnt = designationDto.MapTo<Designation>();
            userEnt.CompanyID = designationDto.CompanyID ?? AppContexts.User.CompanyID;


            DesignationRepo.Add(userEnt);
            unitOfWork.CommitChangesWithAudit();
        }

        public async Task<UploadGenericResponseDto<DesignationDto>> SaveChangesUploadDesignation(FileRequestDto request)
        {
            UploadGenericResponseDto<DesignationDto> response = new UploadGenericResponseDto<DesignationDto>();
            List<DesignationDto> dList = new List<DesignationDto>();
            bool stat = true;

            SaveFileDescription fileDesc = new SaveFileDescription();
            string filePath = string.Empty;
            try
            {
                #region Save file in Disk & convert to ObjectList from Excel and Dispose file
                fileDesc = await UploadUtil.SaveFileInDisk(request.File, "designation");
                filePath = fileDesc.FullPath;

                if (!fileDesc.FileExtention.Equals(".xls") && !fileDesc.FileExtention.Equals(".xlsx"))
                {
                    File.Delete(filePath);
                    return response;
                }

                var worksheet = UploadUtil.ConvertFileToExcel(filePath);
                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;

                if (rowCount < 1 || colCount < 1)
                {
                    File.Delete(filePath);
                    return response;
                }

                dList = new GenericParse<DesignationDto>().ParseExcelFileToObjectList(worksheet, rowCount, colCount);
                worksheet.Dispose();
                #endregion
            }
            catch (Exception ex)
            {
                File.Delete(filePath);
                return response;
            }

            #region Check duplicate designation name and code in uploded excel 
            var duplicateDesignationtNameRecords = dList.GroupBy(d => d.DesignationName).Where(d => d.Count() > 1).Select(d => d.Key).ToList();
            var duplicateDesignationCodeRecords = dList.GroupBy(d => d.DesignationCode).Where(d => d.Count() > 1).Select(d => d.Key).ToList();

            if (duplicateDesignationtNameRecords.Any())
            {
                dList.Where(e => duplicateDesignationtNameRecords.Contains(e.DesignationName)).Select(d => d.DesignationNameError = "Duplicate Designation Name").ToList();
            }
            if (duplicateDesignationCodeRecords.Any())
            {
                dList.Where(e => duplicateDesignationCodeRecords.Contains(e.DesignationCode)).Select(d => d.DesignationCodeError = "Duplicate Designation Code").ToList();
            }
            #endregion

            #region Get all Designation list from DB.
            var allDesignation = DesignationRepo.GetAllListAsync().Result;
            #endregion

            #region Get all division name, department name and code
            var allDesignationName = allDesignation.Select(div => div.DesignationName).ToList();
            var allDesignationCode = allDesignation.Select(div => div.DesignationCode).ToList();
            #endregion

            #region Get all designation name exists in the file and check existance from DB list
            var distinctNewDesignationName = dList.Where(d => Convert.ToInt32(d.DesignationID) == 0).Select(d => d.DesignationName).ToList().GroupBy(d => d).Select(x => x.FirstOrDefault()).ToList();

            if (distinctNewDesignationName.Any() && !duplicateDesignationtNameRecords.Any())
            {
                var exitsDesignationName = allDesignationName.Where(s => distinctNewDesignationName.Contains(s)).ToList();

                if (exitsDesignationName.Any())
                {
                    stat = false;
                    dList.Where(d => exitsDesignationName.Contains(d.DesignationName) && Convert.ToInt32(d.DesignationID) == 0).Select(d => d.DesignationNameError = "Designation Name already exists.").ToList();
                }
            }
            #endregion

            #region Get all designation code exists in the file and check existance from DB list
            var distinctNewDesignationCode = dList.Where(d => Convert.ToInt32(d.DesignationID) == 0).Select(d => d.DesignationCode).ToList().GroupBy(d => d).Select(x => x.FirstOrDefault()).ToList();
            if (distinctNewDesignationCode.Any() && !duplicateDesignationCodeRecords.Any())
            {
                var exitsDesignationCode = allDesignationCode.Where(s => distinctNewDesignationCode.Contains(s)).ToList();
                if (exitsDesignationCode.Any())
                {
                    stat = false;
                    dList.Where(d => exitsDesignationCode.Contains(d.DesignationCode) && Convert.ToInt32(d.DesignationID) == 0).Select(d => d.DesignationCodeError = "Designation Code already exists.").ToList();
                }
            }
            #endregion

            // If any point it does not meet the validation
            if (!stat && !request.IsContinue)
            {
                File.Delete(filePath);
                response.dList = dList;
                return response;
            }
            bool fileError = false;
            List<Designation> desList = new List<Designation>();
            using (var unitOfWork = new UnitOfWork())
            {
                foreach (var d in dList)
                {
                    fileError = false;
                    var existDesignation = DesignationRepo.Entities.SingleOrDefault(x => x.DesignationID == d.DesignationID).MapTo<Designation>();

                    if (!d.DesignationID.IsZero() && existDesignation == null && !request.IsContinue)
                    {
                        fileError = true;
                        dList.Where(des => Convert.ToInt32(des.DesignationID) == d.DesignationID).Select(dept => dept.DesignationNameError = "Designation not found with this ID.").ToList();
                    }

                    if (existDesignation.IsNull() && d.DesignationID.IsZero() && !fileError)
                    {
                        d.SetAdded();
                        SetNewDesignationID(d);
                    }
                    else if(!fileError && !d.DesignationID.IsZero())
                    {
                        d.SetModified();
                        d.RowVersion = existDesignation.RowVersion;
                        d.CreatedBy = existDesignation.CreatedBy;
                        d.CreatedIP = existDesignation.CreatedIP;
                        d.CreatedDate = existDesignation.CreatedDate;
                    }
                    if (string.IsNullOrEmpty(d.DesignationNameError) && string.IsNullOrEmpty(d.DesignationCodeError))
                    {
                        var designationtEnt = d.MapTo<Designation>();
                        SetAuditFields(designationtEnt);
                        desList.Add(designationtEnt);
                    }
                   
                }
                if ((fileError && !request.IsContinue) || desList.Count < 1)
                {
                    File.Delete(filePath);
                    response.dList = dList;
                    unitOfWork.Dispose();
                    return response;
                }

                DesignationRepo.AddRange(desList);

                #region Save file in the DB

                using (var excelPackage = new ExcelPackage())
                {
                    File.Delete(filePath);
                    var worksheet = excelPackage.Workbook.Worksheets.Add("upload-designation");
                    worksheet.Cells.LoadFromCollection(dList.Select(d=> new {d.DesignationID,d.DesignationName, d.DesignationCode, d.DesignationNameError,d.DesignationCodeError}), true);
                    var byteArray = excelPackage.GetAsByteArray();
                    UploadUtil.SaveAttachmentInDisk(byteArray, fileDesc.SavedFileName, "designation");
                }

                Attachment attachment = new Attachment();
                attachment.FileName = fileDesc.SavedFileName;
                attachment.OriginalName = fileDesc.FileOriginalName;
                attachment.Type = fileDesc.FileExtention;
                attachment.FilePath = fileDesc.FileRelativePath;
                attachment.Size = fileDesc.FileSize / 1000;
                SetAttachmentNewId(attachment);
                SaveSingleAttachment(attachment.FUID, attachment.FilePath, attachment.FileName, attachment.Type, attachment.OriginalName, 0, "DesignationUpload", false, attachment.Size, 0, false, attachment.Description ?? "");
                #endregion
                unitOfWork.CommitChangesWithAudit();

                response.dList = dList;
                response.uploadStatus = true;
            }
            return await Task.FromResult(response);
        }

        private void SetAttachmentNewId(Attachment attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }


        private void SetNewDesignationID(DesignationDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("Designation", AppContexts.User.CompanyID);
            obj.DesignationID = code.MaxNumber;
        }

        public async Task<DesignationDto> GetDesignation(int designationId)
        {
            var designation = DesignationRepo.Entities.SingleOrDefault(x => x.DesignationID == designationId).MapTo<DesignationDto>();
            return await Task.FromResult(designation);
        }

        public void DeleteDesignation(int designationId)
        {
            using var unitOfWork = new UnitOfWork();
            var designation = DesignationRepo.Entities.SingleOrDefault(x => x.DesignationID == designationId);
            designation.SetDeleted();
            DesignationRepo.Add(designation);

            unitOfWork.CommitChangesWithAudit();
        }

    }
}
