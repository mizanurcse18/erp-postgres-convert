using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SCM.DAL.Entities;
using SCM.Manager.Dto;
using SCM.Manager.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HRMS.Manager.Implementations
{
    public class WarehouseManager : ManagerBase, IWarehouseManager
    {

        private readonly IRepository<Warehouse> WarehouseRepo;
        public WarehouseManager(IRepository<Warehouse> warehouseRepo)
        {
            WarehouseRepo = warehouseRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetWarehouseListDic()
        {
            string sql = $@"SELECT I.*, SV1.SystemVariableCode AbleToName, SV2.SystemVariableCode WarehouseTypeName, 1 IsRemovable, GL.GLName GLName, GL1.GLName SalesReturnGLName
                            FROM Warehouse I
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.AccountsContext)}..GeneralLedger GL ON I.GLID=GL.GLID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.AccountsContext)}..GeneralLedger GL1 ON I.SalesReturnGLID=GL1.GLID
                            
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV1 ON I.AbleToID = SV1.SystemVariableID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV2 ON I.WarehouseTypeID = SV2.SystemVariableID
                            ORDER BY I.WarehouseID DESC";
            var listDict = WarehouseRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }
        public async Task<Dictionary<string, object>> GetWarehouse(int WarehouseID)
        {

            string sql = $@"SELECT I.*, SV1.SystemVariableCode AbleToName, SV2.SystemVariableCode WarehouseTypeName, GL.GLName GLName, GL1.GLName SalesReturnGLName
                            FROM Warehouse I
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.AccountsContext)}..GeneralLedger GL ON I.GLID=GL.GLID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.AccountsContext)}..GeneralLedger GL1 ON I.SalesReturnGLID=GL1.GLID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV1 ON I.AbleToID = SV1.SystemVariableID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV2 ON I.WarehouseTypeID = SV2.SystemVariableID
                            WHERE I.WarehouseID={WarehouseID}";

            var reg = WarehouseRepo.GetData(sql);
            return await Task.FromResult(reg);
        }


        public async Task Delete(int WarehouseID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var warehouseEnt = WarehouseRepo.Entities.Where(x => x.WarehouseID == WarehouseID).FirstOrDefault();

                warehouseEnt.SetDeleted();
                WarehouseRepo.Add(warehouseEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }

        private List<Attachments> RemoveAttachments(WarehouseDto wh)
        {
            if (wh.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachments>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='Warehouse' AND ReferenceID={wh.WarehouseID}";
                var prevAttachment = WarehouseRepo.GetDataDictCollection(attachmentSql);

                foreach (var data in prevAttachment)
                {
                    attachemntList.Add(new Attachments
                    {
                        FUID = (int)data["FUID"],
                        FilePath = data["FilePath"].ToString(),
                        OriginalName = data["OriginalName"].ToString(),
                        FileName = data["FileName"].ToString(),
                        Type = data["FileType"].ToString(),
                        Size = Convert.ToDecimal(data["SizeInKB"]),

                    });
                }
                var removeList = attachemntList.Where(x => !wh.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "Warehouse";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + wh.WarehouseName);
                        System.IO.File.Delete(str + "\\" + data.FileName);

                    }

                }
                return removeList;
            }
            return null;
        }
        private void SetAttachmentNewId(Attachments attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }
        public List<Attachments> GetAttachments(int wid)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='Warehouse' AND ReferenceID={wid}";
            var attachment = WarehouseRepo.GetDataDictCollection(attachmentSql);
            var attachemntList = new List<Attachments>();

            foreach (var data in attachment)
            {
                attachemntList.Add(new Attachments
                {
                    FUID = (int)data["FUID"],
                    AID = data["FUID"].ToString(),
                    FilePath = data["FilePath"].ToString(),
                    OriginalName = data["OriginalName"].ToString() + data["FileType"].ToString(),
                    FileName = data["FileName"].ToString(),
                    Type = data["FileType"].ToString(),
                    Size = Convert.ToDecimal(data["SizeInKB"]),
                    Description = data["Description"].ToString()
                });
            }
            return attachemntList;
        }
        public Task<WarehouseDto> SaveChanges(WarehouseDto warehouseDto)
        {
            var removeList = RemoveAttachments(warehouseDto);
            //check duplicates
            var isExistsName = WarehouseRepo.Entities.FirstOrDefault(x => x.WarehouseID != warehouseDto.WarehouseID && x.WarehouseName.ToLower() == warehouseDto.WarehouseName.ToLower()).MapTo<Warehouse>();
            if (isExistsName.IsNotNull())
            {
                warehouseDto.WarehouseNameError = "Warehouse Name already exists.";
                return Task.FromResult(warehouseDto);
            }


            using (var unitOfWork = new UnitOfWork())
            {
                var existWarehouse = WarehouseRepo.Entities.SingleOrDefault(x => x.WarehouseID == warehouseDto.WarehouseID).MapTo<Warehouse>();

                if (existWarehouse.IsNull() || warehouseDto.WarehouseID.IsZero() )
                {
                    warehouseDto.SetAdded();
                    SetNewUserID(warehouseDto);
                }
                else
                {

                    warehouseDto.CreatedBy = existWarehouse.CreatedBy;
                    warehouseDto.CreatedDate = existWarehouse.CreatedDate;
                    warehouseDto.CreatedIP = existWarehouse.CreatedIP;
                    warehouseDto.RowVersion = existWarehouse.RowVersion;
                    warehouseDto.SetModified();
                }

                var warehouseEnt = warehouseDto.MapTo<Warehouse>();
                SetAuditFields(warehouseEnt);

                if (warehouseDto.Attachments.IsNotNull() && warehouseDto.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(warehouseDto.Attachments.Where(x => x.ID == 0).ToList(), warehouseDto.WarehouseName);

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)warehouseDto.WarehouseID, "Warehouse", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)warehouseDto.WarehouseID, "Warehouse", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }

                WarehouseRepo.Add(warehouseEnt);
                unitOfWork.CommitChangesWithAudit();
            }
            return Task.FromResult(warehouseDto);
        }
        private List<Attachments> AddAttachments(List<Attachments> list, string name)
        {
            if (list.Count > 0)
            {
                var attachemntList = new List<Attachments>();
                int sl = 0;
                foreach (var attachment in list)
                {
                    if (attachment.AttachedFile.IsNotNull())
                    {
                        string filename = $"Warehouse-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "Warehouse\\" + name.Trim());

                        attachemntList.Add(new Attachments
                        {
                            FilePath = filePath,
                            OriginalName = Path.GetFileNameWithoutExtension(attachment.OriginalName),
                            FileName = filename,
                            Type = Path.GetExtension(attachment.OriginalName),
                            Size = attachment.Size,
                            Description = attachment.Description
                        });

                        sl++;
                    }

                }
                return attachemntList;
            }
            return null;
        }

        private void SetNewUserID(WarehouseDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("Warehouse", AppContexts.User.CompanyID);
            obj.WarehouseID = code.MaxNumber;
        }

    }
}
