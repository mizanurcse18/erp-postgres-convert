using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Hosting;
using SCM.DAL.Entities;
using SCM.Manager.Dto;
using SCM.Manager.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace HRMS.Manager.Implementations
{
    public class ItemManager : ManagerBase, IItemManager
    {

        private readonly IRepository<Item> ItemRepo;
        public ItemManager(IRepository<Item> itemRepo)
        {
            ItemRepo = itemRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetItemListDic()
        {
            string sql = QueryForList();
            var listDict = ItemRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        private static string QueryForList()
        {
            return $@"SELECT I.*, ISG.ItemSubGroupName, SV1.SystemVariableCode AssetTypeName, SV2.SystemVariableCode InventoryTypeName, GL.GLName GLName,
                            U.UnitCode UnitName, 1 IsRemovable, 'Approved' ApprovalStatus
                            FROM Item I
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.AccountsContext)}..GeneralLedger GL ON I.GLID=GL.GLID
                            LEFT JOIN ItemSubGroup ISG ON I.ItemSubGroupID = ISG.ItemSubGroupID 
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Unit U ON I.UnitID = U.UnitID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV1 ON I.AssetTypeID = SV1.SystemVariableID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV2 ON I.InventoryTypeID = SV2.SystemVariableID
                            ORDER BY I.ItemID DESC";
        }

        public async Task<Dictionary<string, object>> GetItem(int ItemID)
        {

            string sql = $@"SELECT I.*, ISG.ItemSubGroupName, SV1.SystemVariableCode AssetTypeName, SV2.SystemVariableCode InventoryTypeName, GL.GLName GLName,
                            U.UnitCode UnitName
                            FROM Item I
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.AccountsContext)}..GeneralLedger GL ON I.GLID=GL.GLID
                            LEFT JOIN ItemSubGroup ISG ON I.ItemSubGroupID = ISG.ItemSubGroupID 
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..Unit U ON I.UnitID = U.UnitID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV1 ON I.AssetTypeID = SV1.SystemVariableID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV2 ON I.InventoryTypeID = SV2.SystemVariableID
                            WHERE I.ItemID={ItemID}";

            var reg = ItemRepo.GetData(sql);
            reg.Add("Attachments", GetAttachments(ItemID));
            return await Task.FromResult(reg);
        }


        public async Task Delete(int ItemID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var itemEnt = ItemRepo.Entities.Where(x => x.ItemID == ItemID).FirstOrDefault();

                itemEnt.SetDeleted();
                ItemRepo.Add(itemEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public Task<ItemDto> SaveChanges(ItemDto itemDto)
        {
            //check duplicates
            var isExistsName = ItemRepo.Entities.FirstOrDefault(x => x.ItemID != itemDto.ItemID && x.ItemName.ToLower() == itemDto.ItemName.ToLower()).MapTo<Item>();
            if (isExistsName.IsNotNull())
            {
                itemDto.ItemNameError = "Item Name already exists.";
                return Task.FromResult(itemDto);
            }


            using (var unitOfWork = new UnitOfWork())
            {
                var existItem = ItemRepo.Entities.SingleOrDefault(x => x.ItemID == itemDto.ItemID).MapTo<Item>();
                
                if (existItem.IsNull() || itemDto.ItemID.IsZero() )
                {
                    itemDto.SetAdded();
                    SetItemCodeSuffix(itemDto);
                    itemDto.ItemCode = itemDto.ItemCodePrefix + itemDto.ItemCodeSuffix;
                    SetNewUserID(itemDto);
                }
                else
                {

                    itemDto.CreatedBy = existItem.CreatedBy;
                    itemDto.CreatedDate = existItem.CreatedDate;
                    itemDto.CreatedIP = existItem.CreatedIP;
                    itemDto.RowVersion = existItem.RowVersion;
                    itemDto.ExternalID = existItem.ExternalID;
                    itemDto.ItemCode = itemDto.ItemCodePrefix + itemDto.ItemCodeSuffix;
                    itemDto.SetModified();
                }

                RemoveAttachments(itemDto);
                AddAttachments(itemDto);

                var itemEnt = itemDto.MapTo<Item>();
                SetAuditFields(itemEnt);
                ItemRepo.Add(itemEnt);
                unitOfWork.CommitChangesWithAudit();
            }
            return Task.FromResult(itemDto);
        }

        private void SetNewUserID(ItemDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("Item", AppContexts.User.CompanyID);
            obj.ItemID = code.MaxNumber;
        }
        private void SetItemCodeSuffix(ItemDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("ItemSuffix", AppContexts.User.CompanyID);
            obj.ItemCodeSuffix = code.MaxNumber.ToString();
        }
        private void SetAttachmentNewId(Attachments attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }

        private void AddAttachments(ItemDto item)
        {
            if (item.Attachments.IsNotNull() && item.Attachments.Count > 0)
            {
                var attachemntsList = item.Attachments.Where(x => x.ID == 0).ToList();
                int sl = 0;
                foreach (var attachment in attachemntsList)
                {
                    if (attachment.AttachedFile.IsNotNull())
                    {
                        // To Add Physical Files

                        string filename = $"IndividualItemAttachment-{DateTime.Now:ddMMyyHHmmss}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "IndividualItemAttachment\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

                        // To Add Into DB
                        SetAttachmentNewId(attachment);
                        SaveSingleAttachment(attachment.FUID, filePath, filename, Path.GetExtension(attachment.OriginalName), Path.GetFileNameWithoutExtension(attachment.OriginalName), (int)item.ItemID, "Item", false, attachment.Size, 0, false, attachment.Description ?? "");

                        sl++;
                    }
                }
            }
        }

        private void RemoveAttachments(ItemDto item)
        {
            if(item.ItemID > 0)
            {
                //if (item.IsDeleted)
                //{
                //    foreach (var data in item.Attachments)
                //    {
                //        // To Remove Physical Files

                //        string attachmentFolder = "upload\\attachments";
                //        string folderName = "IndividualItemAttachment";
                //        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                //        string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                //        File.Delete(str + "\\" + data.FileName);

                //        // To Remove From DB

                //        SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)item.ItemID, "Item", true, data.Size, 0, false, data.Description ?? "");

                //    }
                //}
                //else
                //{
                //    var attachemntList = new List<Attachments>();
                //    string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='Item' AND ReferenceID={item.ItemID}";
                //    var prevAttachment = GetListOfDictionaryWithSql(attachmentSql).Result;

                //    foreach (var data in prevAttachment)
                //    {
                //        attachemntList.Add(new Attachments
                //        {
                //            FUID = (int)data["FUID"],
                //            FilePath = data["FilePath"].ToString(),
                //            OriginalName = data["OriginalName"].ToString(),
                //            FileName = data["FileName"].ToString(),
                //            Type = data["FileType"].ToString(),
                //            Size = Convert.ToDecimal(data["SizeInKB"]),

                //        });
                //    }
                //    var removeFiles = attachemntList.Where(x => !item.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                //    if (removeFiles.Count > 0)
                //    {
                //        foreach (var data in removeFiles)
                //        {
                //            // To Remove Physical Files

                //            string attachmentFolder = "upload\\attachments";
                //            string folderName = "IndividualItemAttachment";
                //            IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                //            string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                //            //File.Delete(str + "\\" + data.FileName);
                //            System.IO.File.Delete(str + "\\" + data.FileName);
                //            // To Remove From DB

                //            SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)item.ItemID, "Item", true, data.Size, 0, false, data.Description ?? "");

                //        }

                //    }
                //}

                var attachemntList = new List<Attachments>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='Item' AND ReferenceID={item.ItemID}";
                var prevAttachment = GetListOfDictionaryWithSql(attachmentSql).Result;

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
                var removeFiles = attachemntList.Where(x => !item.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeFiles.Count > 0)
                {
                    foreach (var data in removeFiles)
                    {
                        // To Remove Physical Files

                        string attachmentFolder = "upload\\attachments";
                        string folderName = "IndividualItemAttachment";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        //File.Delete(str + "\\" + data.FileName);
                        System.IO.File.Delete(str + "\\" + data.FileName);
                        // To Remove From DB

                        SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)item.ItemID, "Item", true, data.Size, 0, false, data.Description ?? "");

                    }

                }
            }
           

        }

        private List<Attachments> GetAttachments(int ItemID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload WHERE TableName='Item' AND ReferenceID={ItemID}";
            var attachment = GetListOfDictionaryWithSql(attachmentSql).Result;
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

        public GridModel GetListForGrid(GridParameter parameters)
        {
            var result = ItemRepo.LoadGridModel(parameters, QueryForList().Replace("ORDER BY I.ItemID DESC",""));
            return result;
        }
    }
}
