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

namespace SCM.Manager.Implementations
{
    public class SupplierManager : ManagerBase, ISupplierManager
    {

        private readonly IRepository<Supplier> SupplierRepo;
        public SupplierManager(IRepository<Supplier> supplierRepo)
        {
            SupplierRepo = supplierRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetSupplierListDic()
        {
            string sql = $@"SELECT D.*, ST.TypeName SupplierTypeName, sv.SystemVariableCode SupplierCategoryName, 1 IsRemovable, GL.GLName
                        FROM Supplier D
                            LEFT JOIN SupplierType ST ON D.SupplierTypeID=ST.STID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.AccountsContext)}..GeneralLedger GL ON D.GLID=GL.GLID
                            left join {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable sv on D.SupplierCategoryID = sv.SystemVariableID
                        ORDER BY D.SupplierID DESC";
            var listDict = SupplierRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }
        public async Task<Dictionary<string, object>> GetSupplier(int SupplierID)
        {

            string sql = $@"SELECT D.*, ST.TypeName SupplierTypeName, sv.SystemVariableCode SupplierCategoryName, GL.GLName
                            FROM Supplier D 
                            LEFT JOIN SupplierType ST ON D.SupplierTypeID=ST.STID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.AccountsContext)}..GeneralLedger GL ON D.GLID=GL.GLID
                            left join {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable sv on D.SupplierCategoryID = sv.SystemVariableID
                            WHERE D.SupplierID={SupplierID}";

            var reg = SupplierRepo.GetData(sql);
            return await Task.FromResult(reg);
        }


        public async Task Delete(int SupplierID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var supplierEnt = SupplierRepo.Entities.Where(x => x.SupplierID == SupplierID).FirstOrDefault();

                supplierEnt.SetDeleted();
                SupplierRepo.Add(supplierEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        private List<Attachments> RemoveAttachments(SupplierDto supplier)
        {
            if (supplier.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachments>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='Supplier' AND ReferenceID={supplier.SupplierID}";
                var prevAttachment = SupplierRepo.GetDataDictCollection(attachmentSql);

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
                var removeList = attachemntList.Where(x => !supplier.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "Supplier";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + supplier.SupplierName);
                        File.Delete(str + "\\" + data.FileName);

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
        public List<Attachments> GetAttachments(int supplierid)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='Supplier' AND ReferenceID={supplierid}";
            var attachment = SupplierRepo.GetDataDictCollection(attachmentSql);
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

        public Task<SupplierDto> SaveChanges(SupplierDto supplierDto)
        {
            var removeList = RemoveAttachments(supplierDto);
            //check duplicates
            var isExistsName = SupplierRepo.Entities.FirstOrDefault(x => x.SupplierID != supplierDto.SupplierID && x.SupplierName.ToLower() == supplierDto.SupplierName.ToLower()).MapTo<Supplier>();
            if (isExistsName.IsNotNull())
            {
                supplierDto.SupplierNameError = "Supplier Name already exists by this Name.";
                return Task.FromResult(supplierDto);
            }

            using (var unitOfWork = new UnitOfWork())
            {
                var existSupplier = SupplierRepo.Entities.SingleOrDefault(x => x.SupplierID == supplierDto.SupplierID).MapTo<Supplier>();

                if (existSupplier.IsNull() || supplierDto.SupplierID.IsZero() )
                {
                    supplierDto.SetAdded();
                    SetNewSupplierCode(supplierDto);
                    SetNewUserID(supplierDto);
                }
                else
                {
                    supplierDto.SupplierCode = existSupplier.SupplierCode;
                    supplierDto.CreatedBy = existSupplier.CreatedBy;
                    supplierDto.CreatedDate = existSupplier.CreatedDate;
                    supplierDto.CreatedIP = existSupplier.CreatedIP;
                    supplierDto.RowVersion = existSupplier.RowVersion;
                    supplierDto.ExternalID = existSupplier.ExternalID;
                    supplierDto.SetModified();
                }

                var supplierEnt = supplierDto.MapTo<Supplier>();
                SetAuditFields(supplierEnt);

                if (supplierDto.Attachments.IsNotNull() && supplierDto.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(supplierDto.Attachments.Where(x => x.ID == 0).ToList(), supplierDto.SupplierName);

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)supplierDto.SupplierID, "Supplier", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)supplierDto.SupplierID, "Supplier", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }



                SupplierRepo.Add(supplierEnt);
                unitOfWork.CommitChangesWithAudit();
            }
            return Task.FromResult(supplierDto);
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
                        string filename = $"Supplier-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "Supplier\\" + name.Trim());

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

        private void SetNewUserID(SupplierDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("Supplier", AppContexts.User.CompanyID);
            obj.SupplierID = code.MaxNumber;
        }
        private void SetNewSupplierCode(SupplierDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("SupplierCode", AppContexts.User.CompanyID);
            obj.SupplierCode = code.SystemCode;
        }
    }
}
