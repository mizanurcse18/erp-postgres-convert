using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using Accounts.Manager.Interfaces;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Manager.Implementations
{
    public class VoucherManager : ManagerBase, IVoucherManager
    {
        private readonly IRepository<VoucherMaster> VoucherMasterRepo;
        private readonly IRepository<VoucherChild> VoucherChildRepo;
        public VoucherManager(IRepository<VoucherMaster> _VoucherMasterRepo, IRepository<VoucherChild> _VoucherChildRepo)
        {
            VoucherMasterRepo = _VoucherMasterRepo;
            VoucherChildRepo = _VoucherChildRepo;
        }

        private void SetVoucherMasterNewId(VoucherMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("VoucherMaster", AppContexts.User.CompanyID);
            master.VoucherMasterID = code.MaxNumber;
        }

        private void SetVoucherChildNewId(VoucherChild child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("VoucherChild", AppContexts.User.CompanyID);
            child.VoucherChildID = code.MaxNumber;
        }

        private List<VoucherChild> GenerateVoucherChildItem(VoucherDto VD)
        {
            var existingVoucherItem = VoucherChildRepo.GetAllList(x => x.VoucherMasterID == VD.VoucherMasterId);
            var voucherChildModel = new List<VoucherChild>();
            if (VD.VoucherDetails.IsNotNull())
            {
                VD.VoucherDetails.ForEach(x =>
                {
                    voucherChildModel.Add(new VoucherChild
                    {
                        VoucherMasterID = x.VoucherMasterID,
                        VoucherChildID  = x.VoucherChildID,
                        TxnTypeID = x.TxnTypeID,
                        COAID = x.COAID,
                        CostCenterID = x.CostCenterID,
                        BudgetHeadID = x.BudgetHeadID,
                        Narration = x.Narration,
                        ModeOfPaymentID = x.ModeOfPaymentID,
                        CBID = x.CBID,
                        CBCID = x.CBCID,
                        LeafNo = x.LeafNo,
                        DebitAmount = x.DebitAmount,
                        CreditAmount = x.CreditAmount,
                        IsActive = x.IsActive,
                    });

                });

                voucherChildModel.ForEach(x =>
                {
                    if (existingVoucherItem.Count > 0 && x.VoucherChildID > 0)
                    {
                        var existingModelData = existingVoucherItem.FirstOrDefault(y => y.VoucherChildID == x.VoucherChildID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.VoucherMasterID = VD.VoucherMasterId;
                        x.SetAdded();
                        SetVoucherChildNewId(x);
                    }
                });

                var willDeleted = existingVoucherItem.Where(x => !voucherChildModel.Select(y => y.VoucherChildID).Contains(x.VoucherChildID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    voucherChildModel.Add(x);
                });
            }

            return voucherChildModel;
        }

        private void AddAttachments(VoucherDto voucher)
        {
            if (voucher.Attachments.IsNotNull() && voucher.Attachments.Count > 0)
            {
                var attachemntsList = voucher.Attachments.Where(x => x.ID == 0).ToList();
                int sl = 0;
                foreach (var attachment in attachemntsList)
                {
                    if (attachment.AttachedFile.IsNotNull())
                    {
                        // To Add Physical Files

                        string filename = $"Voucher-{DateTime.Now:ddMMyyHHmmss}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "Voucher\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

                        // To Add Into DB
                        SetAttachmentNewId(attachment);
                        SaveSingleAttachment(attachment.FUID, filePath, filename, Path.GetExtension(attachment.OriginalName), Path.GetFileNameWithoutExtension(attachment.OriginalName), (int)voucher.VoucherMasterId, "VoucherMaster", false, attachment.Size, 0, false, attachment.Description ?? "");

                        sl++;
                    }
                }
            }
        }

        private void SetAttachmentNewId(Attachments attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }

        private void RemoveAttachments(VoucherDto voucher)
        {
            if (voucher.VoucherMasterId > 0)
            {
                var attachemntList = new List<Attachments>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='VoucherMaster' AND ReferenceID={voucher.VoucherMasterId}";
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
                var removeFiles = attachemntList.Where(x => !voucher.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeFiles.Count > 0)
                {
                    foreach (var data in removeFiles)
                    {
                        // To Remove Physical Files

                        string attachmentFolder = "upload\\attachments";
                        string folderName = "Voucher";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        //File.Delete(str + "\\" + data.FileName);
                        System.IO.File.Delete(str + "\\" + data.FileName);
                        // To Remove From DB

                        SaveSingleAttachment(data.FUID, data.FilePath, data.FileName, Path.GetExtension(data.OriginalName), Path.GetFileNameWithoutExtension(data.OriginalName), (int)voucher.VoucherMasterId, "VoucherMaster", true, data.Size, 0, false, data.Description ?? "");
                    }

                }
            }
        }

        public async Task<(bool, string)> SaveChanges(VoucherDto voucher)
        {
            var existingVoucher = VoucherMasterRepo.Entities.Where(x=> x.VoucherMasterID == voucher.VoucherMasterId).FirstOrDefault();

            if (voucher.VoucherMasterId > 0 && (existingVoucher.CreatedBy != AppContexts.User.UserID))
            {
                return (false, "You don't have permission to save this Voucher.");
            }

            // Need to use auto mapper
            var masterModel = new VoucherMaster
            {
                ReferenceNo = voucher.ReferenceNo,
                IsExcelUpload = voucher.IsExcelUpload,
                VoucherTypeID = voucher.VoucherTypeId,
                Remarks = voucher.Remarks,
                VoucherDate = voucher.VoucherDate,
                IsActive  =  true,
                ApprovalStatusID = 0
            };

            using (var unitOfWork = new UnitOfWork())
            {
                if (voucher.VoucherMasterId.IsZero() && existingVoucher.IsNull())
                {
                    masterModel.SetAdded();
                    SetVoucherMasterNewId(masterModel);
                    voucher.VoucherMasterId = (int)masterModel.VoucherMasterID;
                }
                else
                {
                    masterModel.CreatedBy = existingVoucher.CreatedBy;
                    masterModel.CreatedDate = existingVoucher.CreatedDate;
                    masterModel.CreatedIP = existingVoucher.CreatedIP;
                    masterModel.RowVersion = existingVoucher.RowVersion;
                    masterModel.VoucherMasterID = existingVoucher.VoucherMasterID;
                    masterModel.SetModified();
                }
                var voucherChildmodel = GenerateVoucherChildItem(voucher);
                RemoveAttachments(voucher);
                AddAttachments(voucher);

                SetAuditFields(masterModel);
                VoucherMasterRepo.Add(masterModel);
                VoucherChildRepo.AddRange(voucherChildmodel);

                unitOfWork.CommitChangesWithAudit();
            }
            await Task.CompletedTask;
            return (true, $"Voucher Created Successfully"); ;
        }


    }
}
