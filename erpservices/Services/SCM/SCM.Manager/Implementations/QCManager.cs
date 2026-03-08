using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.CommonDto;
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
using static Core.Util;

namespace SCM.Manager.Implementations
{
    public class QCManager : ManagerBase, IQCManager
    {

        private readonly IRepository<QCMaster> QCRepo;
        private readonly IRepository<QCChild> QCChildRepo;
        private readonly IRepository<Item> ItemRepo;
        private readonly IRepository<RTVMaster> RTVMasterRepo;
        private readonly IRepository<RTVChild> RTVChildRepo;
        public QCManager(IRepository<QCMaster> qcMasterRepo, IRepository<QCChild> qcChildRepo, IRepository<Item> itemRepo, IRepository<InventoryTransaction> inventoryTransactionRepo
            , IRepository<RTVMaster> rTVMasterRepo
            , IRepository<RTVChild> rTVChildRepo)
        {
            QCRepo = qcMasterRepo;
            QCChildRepo = qcChildRepo;
            ItemRepo = itemRepo;
            RTVMasterRepo = rTVMasterRepo;
            RTVChildRepo = rTVChildRepo;
        }

        private List<Attachment> AddAttachments(List<Attachment> list)
        {
            if (list.Count > 0)
            {
                var attachemntList = new List<Attachment>();
                int sl = 0;
                foreach (var attachment in list)
                {
                    if (attachment.AttachedFile.IsNotNull())
                    {
                        string filename = $"QC-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "QC\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

                        attachemntList.Add(new Attachment
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
        private List<Attachment> RemoveAttachments(QCDto QC)
        {
            if (QC.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachment>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='QCMaster' AND ReferenceID={QC.QCMID}";
                var prevAttachment = QCRepo.GetDataDictCollection(attachmentSql);

                foreach (var data in prevAttachment)
                {
                    attachemntList.Add(new Attachment
                    {
                        FUID = (int)data["FUID"],
                        FilePath = data["FilePath"].ToString(),
                        OriginalName = data["OriginalName"].ToString(),
                        FileName = data["FileName"].ToString(),
                        Type = data["FileType"].ToString(),
                        Size = Convert.ToDecimal(data["SizeInKB"]),

                    });
                }
                var removeList = attachemntList.Where(x => !QC.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "QC";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        System.IO.File.Delete(str + "\\" + data.FileName);

                    }

                }
                return removeList;
            }
            return null;
        }
        private void SetAttachmentNewId(Attachment attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }

        public async Task<(bool, string)> SaveChanges(QCDto QC)
        {
            if (QC.ChalanDate > DateTime.Now || QC.ReceiptDate > DateTime.Now)
            {
                return (false, "You can't select future date.");
            }

            var existingMR = QCRepo.Entities.Where(x => x.QCMID == QC.QCMID).SingleOrDefault();
            var existingRTV = RTVMasterRepo.Entities.Where(x => x.RTVMID == QC.RTVMID).SingleOrDefault();
            if (QC.QCMID > 0 && (existingMR.IsNullOrDbNull() || existingMR.CreatedBy != AppContexts.User.UserID))
            {
                return (false, "You don't have permission to save this QC");
            }


            foreach (var item in QC.Attachments)
            {
                string ext = "";
                bool fileValid = false;
                string fileValidError = "";
                if (item.FUID > 0)
                {
                    ext = item.Type.Remove(0, 1);
                }
                else
                {
                    string result = item.AttachedFile.Split(',')[1];

                    var bytes = System.Convert.FromBase64String(result);
                    fileValid = UploadUtil.IsFileValidForDocument(bytes, item.AttachedFile);


                    string err = CheckValidFileExtensionsForAttachment(ext, item.OriginalName);
                    if (fileValid == false)
                    {
                        fileValidError = "Uploaded file extension is not allowed.";
                    }
                    if (!fileValidError.IsNullOrEmpty())
                    {
                        err = fileValidError;
                    }
                    if (!err.IsNullOrEmpty())
                    {
                        return (false, err);
                    }
                }
            }


            int rtvLen = RTVMasterRepo.Entities.Count();

            var removeList = RemoveAttachments(QC);


            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (QC.QCMID > 0 && QC.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                                
                            FROM QCMaster MR
                            LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MR.QCMID AND AP.APTypeID = {(int)Util.ApprovalType.QC}
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
			                            FROM 
				                            Approval..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                            )AEF ON AEF.ApprovalProcessID = AP.ApprovalProcessID
                            
                            LEFT JOIN 
									(
									SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM  Approval..ApprovalEmployeeFeedback
									UNION ALL 
									SELECT DISTINCT ApprovalProcessID,EmployeeID,EmployeeID FROM Approval..ApprovalForwardInfo 
									)							
							F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                            LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID =  {(int)Util.ApprovalType.QC} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID =  {(int)Util.ApprovalType.QC} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MR.QCMID                                   
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {QC.ApprovalProcessID}";
                var canReassesstment = QCRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, $"Sorry, You can not edit QC once it processed from approval panel");
                    }
                }
                else
                {
                    return (false, $"Sorry, You can not edit QC once it processed from approval panel");
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)QC.QCMID, QC.ApprovalProcessID, (int)Util.ApprovalType.QC);
            }

            string ApprovalProcessID = "0";
            var masterModel = new QCMaster
            {
                WarehouseID = QC.WarehouseID,
                SupplierID = QC.SupplierID,
                PRMasterID = QC.PRMasterID,
                POMasterID = QC.POMasterID,
                TotalSuppliedQty = (decimal)QC.QCItemDetails.Select(x => x.SuppliedQty).DefaultIfEmpty(0).Sum(),
                TotalAcceptedQty = (decimal)QC.QCItemDetails.Select(x => x.AcceptedQty).DefaultIfEmpty(0).Sum(),
                TotalRejectedQty = (Decimal)QC.QCItemDetails.Select(x => x.SuppliedQty).DefaultIfEmpty(0).Sum() - (decimal)QC.QCItemDetails.Select(x => x.AcceptedQty).DefaultIfEmpty(0).Sum(),
                ReferenceKeyword = QC.ReferenceKeyword,
                ChalanNo = QC.ChalanNo,
                ChalanDate = QC.ChalanDate,
                ReceiptDate = QC.ReceiptDate,
                ApprovalStatusID = (int)ApprovalStatus.Pending,
                IsDraft = QC.IsDraft
            };

            var rtvMasterModel = new RTVMaster();
            if (QC.QCItemDetails.Where(x => x.RejectedQty > 0).Any())
            {
                rtvMasterModel = new RTVMaster
                {

                    SupplyDate = DateTime.Now,
                    ReturnDate = DateTime.Now,
                    WarehouseID = QC.WarehouseID,
                    SupplierID = QC.SupplierID,
                    PRMasterID = QC.PRMasterID,
                    POMasterID = QC.POMasterID,
                    TotalSuppliedQty = (decimal)QC.QCItemDetails.Select(x => x.SuppliedQty).DefaultIfEmpty(0).Sum(),
                    TotalReturnQty = (Decimal)QC.QCItemDetails.Select(x => x.SuppliedQty).DefaultIfEmpty(0).Sum() - (decimal)QC.QCItemDetails.Select(x => x.AcceptedQty).DefaultIfEmpty(0).Sum(),

                    BudgetPlanRemarks = QC.BudgetPlanRemarks,

                    SupplierDCDate = DateTime.Now,
                    IsDraft = QC.IsDraft
                };
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (QC.QCMID.IsZero() && existingMR.IsNull())
                {
                    masterModel.ReferenceNo = GenerateQCReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");
                    masterModel.SetAdded();
                    SetQCNewId(masterModel);
                    QC.QCMID = (int)masterModel.QCMID;

                    if (QC.QCItemDetails.Where(x => x.RejectedQty > 0).Any())
                    {
                        rtvMasterModel.ReferenceNo = GenerateRTVReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");
                        rtvMasterModel.QCMID = QC.QCMID;
                        rtvMasterModel.SupplierDCNo = (rtvLen + 1).ToString();
                        rtvMasterModel.SetAdded();
                        SetRTVNewId(rtvMasterModel);
                        QC.RTVMID = rtvMasterModel.RTVMID;
                    }
                }
                else
                {
                    masterModel.CreatedBy = existingMR.CreatedBy;
                    masterModel.CreatedDate = existingMR.CreatedDate;
                    masterModel.CreatedIP = existingMR.CreatedIP;
                    masterModel.RowVersion = existingMR.RowVersion;
                    masterModel.QCMID = QC.QCMID;
                    masterModel.ReferenceNo = existingMR.ReferenceNo;
                    masterModel.BudgetPlanRemarks = existingMR.BudgetPlanRemarks;
                    masterModel.ApprovalStatusID = existingMR.ApprovalStatusID;
                    masterModel.SetModified();

                    if (QC.QCItemDetails.Where(x => x.RejectedQty > 0).Any())
                    {
                        if (existingRTV.IsNull())
                        {
                            rtvMasterModel.ReferenceNo = GenerateRTVReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");
                            rtvMasterModel.QCMID = QC.QCMID;
                            rtvMasterModel.SupplierDCNo = (rtvLen + 1).ToString();
                            rtvMasterModel.SetAdded();
                            SetRTVNewId(rtvMasterModel);
                        }
                        else
                        {
                            rtvMasterModel.CreatedBy = existingRTV.CreatedBy;
                            rtvMasterModel.CreatedDate = existingRTV.CreatedDate;
                            rtvMasterModel.CreatedIP = existingRTV.CreatedIP;
                            rtvMasterModel.RowVersion = existingRTV.RowVersion;
                            rtvMasterModel.RTVMID = existingRTV.RTVMID;
                            rtvMasterModel.QCMID = existingRTV.QCMID;
                            rtvMasterModel.ReferenceNo = existingRTV.ReferenceNo;
                            rtvMasterModel.BudgetPlanRemarks = existingRTV.BudgetPlanRemarks;
                            rtvMasterModel.ApprovalStatusID = existingRTV.ApprovalStatusID;
                            rtvMasterModel.SetModified();
                        }

                    }
                    else
                    {
                        if (existingRTV.IsNotNull())
                        {
                            existingRTV.SetDeleted();
                        }

                    }
                }
                var childModel = GenerateQCChild(QC);
                var rtvChildModel = GenerateRTVChild(QC);



                SetAuditFields(masterModel);
                SetAuditFields(childModel);

                SetAuditFields(rtvMasterModel);
                SetAuditFields(rtvChildModel);

                if (QC.Attachments.IsNotNull() && QC.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(QC.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)QC.QCMID, "QCMaster", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)QC.QCMID, "QCMaster", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }

                if (masterModel.IsAdded)
                {
                    string approvalTitle = $"{Util.QCApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, QC Reference No:{masterModel.ReferenceNo}";


                    if (QC.QCApprovalPanelList.IsNotNull() && QC.QCApprovalPanelList.Count > 0)
                    {
                        foreach (var item in QC.QCApprovalPanelList)
                        {
                            SaveManualApprovalPanel(item.EmployeeID, (int)Util.ApprovalPanel.QCBelowTheLimit, item.SequenceNo, item.ProxyEmployeeID.Value, item.IsProxyEmployeeEnabled, item.NFAApprovalSequenceType.Value, item.IsEditable, item.IsSCM, item.IsMultiProxy, (int)Util.ApprovalType.QC, (int)masterModel.QCMID, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext));
                        }
                    }

                }
                else
                {
                    if (approvalProcessFeedBack.Count > 0)
                    {
                        UpdateApprovalProcessFeedback((int)approvalProcessFeedBack["ApprovalProcessID"],
                            (int)approvalProcessFeedBack["APEmployeeFeedbackID"], (int)Util.ApprovalFeedback.Approved,
                            $@"Reviewed And Resubmited by {AppContexts.User.FullName} - {AppContexts.User.EmployeeCode}",
                            (int)approvalProcessFeedBack["APTypeID"],
                            (int)approvalProcessFeedBack["ReferenceID"], 0);
                        ApprovalProcessID = approvalProcessFeedBack["ApprovalProcessID"].ToString();
                    }
                }

                QCRepo.Add(masterModel);
                QCChildRepo.AddRange(childModel);

                RTVMasterRepo.Add(rtvMasterModel);
                if (existingRTV.IsNotNull())
                {
                    RTVMasterRepo.Add(existingRTV);
                }
                RTVChildRepo.AddRange(rtvChildModel);

                unitOfWork.CommitChangesWithAudit();

                if (masterModel.IsAdded)
                {
                    string approvalTitle = $"{Util.QCApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, QC Reference No:{masterModel.ReferenceNo}";
                    var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
                    var obj = CreateManualApprovalProcess((int)masterModel.QCMID, Util.AutoQCAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.QC, (int)Util.ApprovalPanel.QCBelowTheLimit, context, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext));
                    ApprovalProcessID = obj.ApprovalProcessID;
                }

                if (ApprovalProcessID.ToInt() > 0)
                    // await Extension.Post<string>($"/SendMail/SendEMailToRecipients", "Test API Call");
                    await SendMailFromManagerBase(ApprovalProcessID, false, masterModel.QCMID, (int)Util.MailGroupSetup.QCInitiatedMail, (int)Util.ApprovalType.QC);

            }
            await Task.CompletedTask;

            return (true, $"QC Submitted Successfully"); ;
        }

        private void SetQCNewId(QCMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("QCMaster", AppContexts.User.CompanyID);
            master.QCMID = code.MaxNumber;
        }

        private void SetRTVNewId(RTVMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("RTVMaster", AppContexts.User.CompanyID);
            master.RTVMID = code.MaxNumber;
        }

        private void SetInventoryTransactionNewId(InventoryTransaction tran)
        {
            //if (!tran.IsAdded) return;
            var code = GenerateSystemCode("InventoryTransaction", AppContexts.User.CompanyID);
            tran.ITID = code.MaxNumber;
        }

        private List<QCChild> GenerateQCChild(QCDto QC)
        {
            var existingQCChild = QCChildRepo.Entities.Where(x => x.QCMID == QC.QCMID).ToList();
            var childModel = new List<QCChild>();
            if (QC.QCItemDetails.IsNotNull())
            {
                QC.QCItemDetails.ForEach(x =>
                {
                    if (x.AcceptedQty > 0)
                    {
                        childModel.Add(new QCChild
                        {
                            QCCID = x.QCCID,
                            QCMID = QC.QCMID,
                            POCID = x.POCID,
                            ItemID = x.ItemID,
                            SuppliedQty = x.SuppliedQty,
                            AcceptedQty = x.AcceptedQty,
                            RejectedQty = x.SuppliedQty - x.AcceptedQty,
                            QCCNote = x.QCCNote
                        });
                    }

                });

                childModel.ForEach(x =>
                {
                    if (existingQCChild.Count > 0 && x.QCCID > 0)
                    {
                        var existingModelData = existingQCChild.FirstOrDefault(y => y.QCCID == x.QCCID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.QCMID = QC.QCMID;
                        x.SetAdded();
                        SetQCChildNewId(x);
                    }
                });

                var willDeleted = existingQCChild.Where(x => !childModel.Select(y => y.QCCID).Contains(x.QCCID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    childModel.Add(x);
                });
            }

            return childModel;
        }

        private List<RTVChild> GenerateRTVChild(QCDto QC)
        {
            var childList = new List<RTVChild>();

            foreach (var x in QC.QCItemDetails.Where(x => x.RejectedQty > 0))
            {
                childList.Add(new RTVChild
                {
                    RTVCID = 0,
                    RTVMID = QC.RTVMID,
                    ItemID = x.ItemID,
                    POCID = x.POCID,
                    SuppliedQty = x.SuppliedQty,
                    ReturnQty = x.RejectedQty,
                    RTVCNote = x.QCCNote
                });

            }
            childList.ForEach(x => { x.SetAdded(); SetRTVChildNewId(x); });

            var removeList = RTVChildRepo.Entities.Where(x => x.RTVMID == QC.RTVMID).ToList();
            removeList.ForEach(x =>
            {
                x.SetDeleted();
                childList.Add(x);
            });
            return childList;

            //var existingQCChild = RTVChildRepo.Entities.Where(x => x.RTVMID == QC.RTVMID).ToList();
            //var childModel = new List<RTVChild>();
            //if (QC.QCItemDetails.IsNotNull())
            //{
            //    QC.QCItemDetails.ForEach(x =>
            //    {
            //        if (x.RejectedQty > 0)
            //        {
            //            childModel.Add(new RTVChild
            //            {
            //                RTVCID = 0,
            //                RTVMID = QC.RTVMID,
            //                ItemID = x.ItemID,
            //                POCID = x.POCID,
            //                SuppliedQty = x.SuppliedQty,
            //                ReturnQty = x.RejectedQty,
            //                RTVCNote = x.QCCNote

            //            });
            //        }

            //    });

            //    childModel.ForEach(x =>
            //    {
            //        if (existingQCChild.Count > 0 && x.RTVCID > 0)
            //        {
            //            var existingModelData = existingQCChild.FirstOrDefault(y => y.RTVCID == x.RTVCID);
            //            x.CreatedBy = existingModelData.CreatedBy;
            //            x.CreatedDate = existingModelData.CreatedDate;
            //            x.CreatedIP = existingModelData.CreatedIP;
            //            x.RowVersion = existingModelData.RowVersion;
            //            x.SetModified();
            //        }
            //        else
            //        {
            //            x.RTVMID = QC.RTVMID;
            //            x.SetAdded();
            //            SetRTVChildNewId(x);
            //        }
            //    });

            //    var willDeleted = existingQCChild.Where(x => !childModel.Select(y => y.RTVCID).Contains(x.RTVCID)).ToList();
            //    willDeleted.ForEach(x =>
            //    {
            //        x.SetDeleted();
            //        childModel.Add(x);
            //    });
            //}

            //return childModel;
        }

        private void SetQCChildNewId(QCChild child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("QCChild", AppContexts.User.CompanyID);
            child.QCCID = code.MaxNumber;
        }

        private string GenerateQCReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/QC/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{ GenerateSystemCode("QCMasterRefNo", AppContexts.User.CompanyID).SystemCode}";
            return format;
        }
        private void SetRTVChildNewId(RTVChild child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("RTVChild", AppContexts.User.CompanyID);
            child.RTVCID = code.MaxNumber;
        }

        private string GenerateRTVReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/RTV/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{ GenerateSystemCode("QCMasterRefNo", AppContexts.User.CompanyID).SystemCode}";
            return format;
        }

        public GridModel GetAllQCList(GridParameter parameters)
        {
            string filter = "";
            string where = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "My Pending":
                    filter = $@" AND CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@" AND QC.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND QC.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
                    break;
                case "My Approved":
                    filter = $@" AND AP.ApprovalProcessID IN (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Approved}))";
                    break;
                case "MyRejectReturnForwarded":
                    filter = $@" AND AP.ApprovalProcessID IN 
                                (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Rejected})
                                UNION  
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Returned})
                                UNION 
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Forwarded}))";
                    break;
                default:
                    break;
            }
            where = filter.IsNotNullOrEmpty() ? "WHERE " : "";
            string sql = $@"SELECT DISTINCT
	                             QC.QCMID
								,QC.CreatedBy
								,QC.ReferenceNo
								,QC.ApprovalStatusID
								,QC.CreatedDate
                                ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
                                ,PO.ReferenceNo AS PONo
                                ,PR.ReferenceNo AS PRNo
                                ,PO.POMasterID
                                ,PR.PRMasterID
                                ,S.SupplierName
								,ISNULL(MR.MRID, 0) MRID
                                ,ISNULL(MR.ReferenceNo, '') GRNNo
                                ,PO.ReferenceNo + ' ' + PR.ReferenceNo AS POPRNo
                                ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                                ,ISNULL(APPO.ApprovalProcessID, 0) POApprovalProcessID
                                ,ISNULL(APPR.ApprovalProcessID, 0) PRApprovalProcessID
	                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable
                                ,ISNULL(AEF.IsSCM,0) IsSCM
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate

                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                            FROM QCMaster QC
                            LEFT JOIN PurchaseOrderMaster PO ON PO.POMasterID = QC.POMasterID
                            LEFT JOIN PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID
						    LEFT JOIN Supplier S ON QC.SupplierID = S.SupplierID
                            LEFT JOIN Security..Users U ON U.UserID = QC.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = QC.ApprovalStatusID
							LEFT JOIN MaterialReceive MR ON MR.QCMID = QC.QCMID  AND MR.ApprovalStatusID != 24
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = QC.QCMID AND AP.APTypeID = {(int)Util.ApprovalType.QC}
                            LEFT JOIN Approval..ApprovalProcess APPO ON APPO.ReferenceID = PO.POMasterID AND APPO.APTypeID = {(int)Util.ApprovalType.PO} 
                            LEFT JOIN Approval..ApprovalProcess APPR ON APPR.ReferenceID = PO.PRMasterID AND APPR.APTypeID = {(int)Util.ApprovalType.PR}
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable,IsSCM
			                            FROM 
				                            Approval..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                                    UNION ALL 
										SELECT 
				                            EmployeeFeedbackID,ApprovalProcessID,0 IsEditable ,0 IsSCM 
			                            FROM 
				                            Approval..ApprovalForwardInfo  
			                            WHERE 
				                            EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL
                            )AEF ON AEF.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN 
		                            (SELECT 
				                            APForwardInfoID,ApprovalProcessID 
			                            FROM 
				                            Approval..ApprovalForwardInfo  
			                            WHERE 
				                            EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL) 
                            APForward ON APForward.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN 
									(
									SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM  Approval..ApprovalEmployeeFeedback
									UNION ALL 
									SELECT DISTINCT ApprovalProcessID,EmployeeID,EmployeeID FROM Approval..ApprovalForwardInfo 
									)							
							F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                            LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.QC} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.QC} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = QC.QCMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = QC.QCMID
                                    LEFT JOIN (SELECT DISTINCT
														FeedbackLastResponseDate,ApprovalProcessID
													FROM 
														Approval..ApprovalEmployeeFeedback AEF
													WHERE (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})                                    
									)APSubmitDate ON APSubmitDate.ApprovalProcessID = AP.ApprovalProcessID
									LEFT JOIN (
													SELECT 
													   MAX(CommentSubmitDate)  CommentSubmitDate,ApprovalProcessID 
													FROM 
														Approval..ApprovalForwardInfo  
													WHERE 
														EmployeeID = {AppContexts.User.EmployeeID} 
													GROUP BY ApprovalProcessID
										
														) APFSubmitDate ON APFSubmitDate.ApprovalProcessID = AP.ApprovalProcessID
                                    LEFT JOIN (
								       SELECT 
										    AEF.EmployeeCode PendingEmployeeCode,
										    EmployeeName PendingEmployeeName,
										    DepartmentName PendingDepartmentName,
										    AEF.ReferenceID PendingReferenceID
									    FROM 
										    Approval..viewApprovalEmployeeFeedback AEF 
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.QC} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = QC.QCMID
                                    {where} {filter}
                                    ";
            //WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
            var result = QCRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }

        //QC Approved List

        public GridModel GetQCList(GridParameter parameters)
        {
            string filter = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "My Pending":
                    filter = $@" AND CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@" AND QC.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND QC.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
                    break;
                case "My Approved":
                    filter = $@" AND AP.ApprovalProcessID IN (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Approved}))";
                    break;
                case "MyRejectReturnForwarded":
                    filter = $@" AND AP.ApprovalProcessID IN 
                                (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Rejected})
                                UNION  
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Returned})
                                UNION 
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Forwarded}))";
                    break;
                default:
                    break;
            }

            string sql = $@"SELECT DISTINCT
	                             QC.QCMID
								,QC.CreatedBy
								,QC.ReferenceNo
								,QC.ApprovalStatusID
								,QC.CreatedDate
	                            ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
                                ,PO.ReferenceNo AS PONo
                                ,PR.ReferenceNo AS PRNo
                                ,PO.POMasterID
                                ,PR.PRMasterID
                                ,S.SupplierName
								,ISNULL(MR.MRID, 0) MRID
                                ,ISNULL(MR.ReferenceNo, '') GRNNo
                                ,PO.ReferenceNo + ' ' + PR.ReferenceNo AS POPRNo
                                ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                                ,ISNULL(APPO.ApprovalProcessID, 0) POApprovalProcessID
                                ,ISNULL(APPR.ApprovalProcessID, 0) PRApprovalProcessID
	                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable
                                ,ISNULL(AEF.IsSCM,0) IsSCM
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate

                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                            FROM QCMaster QC
                            LEFT JOIN PurchaseOrderMaster PO ON PO.POMasterID = QC.POMasterID
                            LEFT JOIN PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID
						    LEFT JOIN Supplier S ON QC.SupplierID = S.SupplierID
                            LEFT JOIN Security..Users U ON U.UserID = QC.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = QC.ApprovalStatusID
							LEFT JOIN MaterialReceive MR ON MR.QCMID = QC.QCMID  AND MR.ApprovalStatusID != 24
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = QC.QCMID AND AP.APTypeID = {(int)Util.ApprovalType.QC}
                            LEFT JOIN Approval..ApprovalProcess APPO ON APPO.ReferenceID = PO.POMasterID AND APPO.APTypeID = {(int)Util.ApprovalType.PO} 
                            LEFT JOIN Approval..ApprovalProcess APPR ON APPR.ReferenceID = PO.PRMasterID AND APPR.APTypeID = {(int)Util.ApprovalType.PR}
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable,IsSCM
			                            FROM 
				                            Approval..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 8 OR AEF.APFeedbackID = 9) AND (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                                    UNION ALL 
										SELECT 
				                            EmployeeFeedbackID,ApprovalProcessID,0 IsEditable ,0 IsSCM 
			                            FROM 
				                            Approval..ApprovalForwardInfo  
			                            WHERE 
				                            EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL
                            )AEF ON AEF.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN 
		                            (SELECT 
				                            APForwardInfoID,ApprovalProcessID 
			                            FROM 
				                            Approval..ApprovalForwardInfo  
			                            WHERE 
				                            EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL) 
                            APForward ON APForward.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN 
									(
									SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM  Approval..ApprovalEmployeeFeedback
									UNION ALL 
									SELECT DISTINCT ApprovalProcessID,EmployeeID,EmployeeID FROM Approval..ApprovalForwardInfo 
									)							
							F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                            LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.QC} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.QC} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = QC.QCMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = QC.QCMID
                                    LEFT JOIN (SELECT DISTINCT
														FeedbackLastResponseDate,ApprovalProcessID
													FROM 
														Approval..ApprovalEmployeeFeedback AEF
													WHERE (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})                                    
									)APSubmitDate ON APSubmitDate.ApprovalProcessID = AP.ApprovalProcessID
									LEFT JOIN (
													SELECT 
													   MAX(CommentSubmitDate)  CommentSubmitDate,ApprovalProcessID 
													FROM 
														Approval..ApprovalForwardInfo  
													WHERE 
														EmployeeID = {AppContexts.User.EmployeeID} 
													GROUP BY ApprovalProcessID
										
														) APFSubmitDate ON APFSubmitDate.ApprovalProcessID = AP.ApprovalProcessID
                                    LEFT JOIN (
								       SELECT 
										    AEF.EmployeeCode PendingEmployeeCode,
										    EmployeeName PendingEmployeeName,
										    DepartmentName PendingDepartmentName,
										    AEF.ReferenceID PendingReferenceID
									    FROM 
										    Approval..viewApprovalEmployeeFeedback AEF 
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.QC} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = QC.QCMID WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
                            ";
            //WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
            var result = QCRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }

        public async Task<QCMasterDto> GetQCMaster(int QCMID)
        {
            string sql = $@"SELECT DISTINCT QC.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName, VA1.FullName AS POEmployeeName, VA1.EmployeeCode POEmployeeCode, VA1.DivisionID AS PODivisionID, VA1.DivisionName AS PODivisionName
                            ,PO.Remarks AS PORemarks
                            ,PO.ReferenceNo AS PONo
                            ,W.WarehouseName DeliveryLocationName,VA.WorkMobile
							,S.SupplierName
                            ,ISNULL(MR.MRID, 0) MRID
                            ,ISNULL(RM.RTVMID,0) AS RTVMID
                            ,(SELECT REPLACE(QC.ReferenceNo, 'QC', 'GRN' )) as GRNNo

	                        ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee](353,ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            --,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
                        from QCMaster QC
                        LEFT JOIN Security..Users U ON U.UserID = QC.CreatedBy
						LEFT JOIN PurchaseOrderMaster PO ON PO.POMasterID = QC.POMasterID
						LEFT JOIN Supplier S ON QC.SupplierID = S.SupplierID
                        LEFT JOIN Security..Users U1 ON U1.UserID = PO.CreatedBy
                        LEFT JOIN Warehouse W ON PO.DeliveryLocation=W.WarehouseID
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN HRMS..ViewALLEmployee VA1 ON VA1.PersonID = U1.PersonID
						LEFT JOIN MaterialReceive MR ON MR.QCMID = QC.QCMID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = QC.ApprovalStatusID
                        LEFT JOIN RTVMaster RM ON QC.QCMID = RM.QCMID

                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = QC.QCMID AND AP.APTypeID = {(int)Util.ApprovalType.QC} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.QC}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.QC} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = QC.QCMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = QC.QCMID
                                LEFT JOIN 
								(
								    SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID

                        WHERE QC.QCMID={QCMID}  AND (VA.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID})";
            var master = QCRepo.GetModelData<QCMasterDto>(sql);
            return master;
        }
        public async Task<QCMasterDto> GetQCMasterFromAllList(int QCMID)
        {
            string sql = $@"SELECT QC.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName, VA1.FullName AS POEmployeeName, VA1.EmployeeCode POEmployeeCode, VA1.DivisionID AS PODivisionID, VA1.DivisionName AS PODivisionName
                            ,PO.Remarks AS PORemarks
                            ,PO.ReferenceNo AS PONo
                            ,W.WarehouseName DeliveryLocationName,VA.WorkMobile
							,S.SupplierName
                            ,ISNULL(MR.MRID, 0) MRID
                            ,ISNULL(RM.RTVMID,0) AS RTVMID
                            ,(SELECT REPLACE(QC.ReferenceNo, 'QC', 'GRN' )) as GRNNo

	                        ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee](353,ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            --,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
                        from QCMaster QC
                        LEFT JOIN Security..Users U ON U.UserID = QC.CreatedBy
						LEFT JOIN PurchaseOrderMaster PO ON PO.POMasterID = QC.POMasterID
						LEFT JOIN Supplier S ON QC.SupplierID = S.SupplierID
                        LEFT JOIN Security..Users U1 ON U1.UserID = PO.CreatedBy
                        LEFT JOIN Warehouse W ON PO.DeliveryLocation=W.WarehouseID
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN HRMS..ViewALLEmployee VA1 ON VA1.PersonID = U1.PersonID
						LEFT JOIN MaterialReceive MR ON MR.QCMID = QC.QCMID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = QC.ApprovalStatusID
                        LEFT JOIN RTVMaster RM ON QC.QCMID = RM.QCMID

                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = QC.QCMID AND AP.APTypeID = {(int)Util.ApprovalType.QC} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.QC}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.QC} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = QC.QCMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = QC.QCMID

                        WHERE QC.QCMID={QCMID}";
            var master = QCRepo.GetModelData<QCMasterDto>(sql);
            return master;
        }


        public async Task<List<QCChildDto>> GetQCChild(int QCMID)
        {
            string sql = $@"SELECT  QCC.*,
					        VA.DepartmentID,
					        VA.DepartmentName,
					        VA.FullName AS EmployeeName,
					        SV.SystemVariableCode AS ApprovalStatus,
					        VA.ImagePath,
					        VA.EmployeeCode,
					        VA.DivisionID,
					        VA.DivisionName
					        ,I.ItemName, I.ItemCode,Unit.UnitCode
                            ,ISNULL(MR.Qty, 0) AS GRNReceivedQty
                            ,POC.Qty AS POQty
                            ,POC.Rate
                            ,POC.Description AS PODescription
							,POC.VatPercent POVatPercent
							,ISNULL(QC.QCSuppliedQty, 0) AS QCSuppliedQty
							,ISNULL(QC.QCAcceptedQty, 0) AS QCAcceptedQty
                    FROM QCChild QCC
                        LEFT JOIN QCMaster QCM ON QCM.QCMID = QCC.QCMID
                        LEFT JOIN Item I ON I.ItemID=QCC.ItemID
                        LEFT JOIN Security..Users U ON U.UserID = QCC.CreatedBy
						LEFT JOIN PurchaseOrderChild POC ON POC.POMasterID = QCM.POMasterID AND POC.ItemID = QCC.ItemID  AND POC.POCID = QCC.POCID
						
						LEFT JOIN (	SELECT QCC1.ItemID,SUM(QCC1.SuppliedQty) QCSuppliedQty, SUM(QCC1.AcceptedQty) QCAcceptedQty,POMasterID FROM QCChild QCC1
									JOIN QCMaster QCM1 ON QCC1.QCMID=QCM1.QCMID 
									WHERE QCM1.ApprovalStatusID	<> 24 AND QCC1.QCMID!= {QCMID}
									GROUP BY QCC1.ItemID,POMasterID) QC ON QC.ItemID = QCC.ItemID AND QC.POMasterID=QCM.POMasterID

						LEFT JOIN (	SELECT MRC.ItemID,SUM(MRC.ReceiveQty) Qty, MR1.POMasterID FROM MaterialReceiveChild MRC
									JOIN MaterialReceive MR1 ON MRC.MRID=MR1.MRID 
									WHERE MR1.ApprovalStatusID	<> 24
									GROUP BY MRC.ItemID, MR1.POMasterID) MR ON QCC.ItemID=MR.ItemID AND MR.POMasterID = QCM.POMasterID

                        LEFT JOIN Security..Unit  ON Unit.UnitID=POC.UOM
                        LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = QCM.ApprovalStatusID
                        WHERE QCC.QCMID={QCMID}";
            var childs = QCChildRepo.GetDataModelCollection<QCChildDto>(sql);
            return childs;
        }


        public async Task<RTVMasterDto> GetRTVMaster(int QCMID)
        {
            string sql = $@"SELECT RTV.*,S.SupplierName,PO.ReferenceNo AS PONo
                        from RTVMaster RTV
						LEFT JOIN PurchaseOrderMaster PO ON PO.POMasterID = RTV.POMasterID
						LEFT JOIN Supplier S ON RTV.SupplierID = S.SupplierID
                        WHERE RTV.QCMID={QCMID}";
            var master = QCRepo.GetModelData<RTVMasterDto>(sql);
            return master;
        }

        public async Task<List<RTVChildDto>> GetRTVChild(int QCMID)
        {
            string sql = $@"SELECT  RTVC.*,I.ItemName, I.ItemCode,Unit.UnitCode
                        FROM RTVChild RTVC
                        LEFT JOIN RTVMaster RTVM ON RTVM.RTVMID = RTVC.RTVMID
                        LEFT JOIN QCMaster QCM ON QCM.QCMID = RTVM.QCMID
                        LEFT JOIN Item I ON I.ItemID=RTVC.ItemID
						LEFT JOIN PurchaseOrderChild POC ON POC.POMasterID = QCM.POMasterID AND POC.ItemID = RTVC.ItemID
						
                        LEFT JOIN Security..Unit  ON Unit.UnitID=POC.UOM   
                        WHERE RTVM.QCMID={QCMID} AND RTVC.ReturnQty > 0";
            var childs = QCChildRepo.GetDataModelCollection<RTVChildDto>(sql);
            return childs;
        }


        public async Task<List<ManualApprovalPanelEmployeeDto>> GetQCApprovalPanelDefault(int QCMID)
        {
            string sql = $@"SELECT APE.*, Emp.EmployeeCode, Emp.FullName AS EmployeeName, EmpPr.FullName AS ProxyEmployeeName, AP.Name AS PanelName, SV.SystemVariableCode AS NFAApprovalSequenceTypeName                     
                            FROM Approval..ManualApprovalPanelEmployee APE
                            LEFT JOIN HRMS..ViewALLEmployee Emp ON APE.EmployeeID = Emp.EmployeeID							
                            LEFT JOIN HRMS..Employee EmpPr ON APE.ProxyEmployeeID = EmpPr.EmployeeID					
                            LEFT JOIN Approval..ApprovalPanel AP ON APE.APPanelID = AP.APPanelID
							LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = APE.NFAApprovalSequenceType
                            WHERE APE.ReferenceID={QCMID} AND APE.APTypeID ={(int)Util.ApprovalType.QC}";

            var maps = QCRepo.GetDataModelCollection<ManualApprovalPanelEmployeeDto>(sql);

            return maps;
        }

        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.QC);
            return comments.Result;
        }
        public async Task<List<ComboModel>> GetForwardingMemberList(int ReferenceID, int APTypeID, int APPanelID)
        {
            return GetApprovalForwardingMembers(ReferenceID, APTypeID, APPanelID).Result;
        }
        public async Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID)
        {
            return GetApprovalForwardingMembers(ApprovalProcessID).Result;
        }
        public async Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId)
        {
            return GetApprovalRejectedMembers(aprovalProcessId).Result;
        }

        public IEnumerable<Dictionary<string, object>> ReportForQCApprovalFeedback(int QCMID)
        {
            string sql = $@" EXEC SCM..spRPTQCApprovalFeedback {QCMID}";
            var feedback = QCRepo.GetDataDictCollection(sql);
            return feedback;
        }

        public IEnumerable<Dictionary<string, object>> ReportForGRNApprovalFeedback(int QCMID)
        {
            string sql = $@" EXEC SCM..spRPTPOApprovalFeedback {QCMID}";
            var feedback = QCRepo.GetDataDictCollection(sql);
            return feedback;
        }
        public List<Attachments> GetAttachments(int QCMID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='QCMaster' AND ReferenceID={QCMID}";
            var attachment = QCRepo.GetDataDictCollection(attachmentSql);
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
        public List<ManualApprovalPanelEmployeeDto> GetGRNApprovalPanelDefault(int QCMID)
        {
            string sql = $@"SELECT Emp.FullName AS EmployeeName, Emp.EmployeeCode,Emp.EmployeeID 
                        FROM QCMaster QC 
                        LEFT JOIN PurchaseRequisitionMaster PR ON QC.PRMasterID = PR.PRMasterID
                        LEFT JOIN Security..Users U ON PR.CreatedBy = U.UserID
                        LEFT JOIN HRMS..ViewALLEmployee Emp ON U.PersonID=Emp.PersonID
                        WHERE QC.QCMID={QCMID}";

            var pr = QCRepo.GetModelData<QCMasterDto>(sql);

            List<ManualApprovalPanelEmployeeDto> apList = new List<ManualApprovalPanelEmployeeDto>();
            ManualApprovalPanelEmployeeDto seq1 = new ManualApprovalPanelEmployeeDto { APPanelID = (int)Util.ApprovalPanel.GRNBelowTheLimit, EmployeeID = AppContexts.User.EmployeeID.Value, IsProxyEmployeeEnabled = false, ProxyEmployeeID = 0, APTypeID = (int)Util.ApprovalType.QC, SequenceNo = 1, EmployeeCode = AppContexts.User.EmployeeCode, EmployeeName = AppContexts.User.EmployeeCode + "-" + AppContexts.User.FullName, NFAApprovalSequenceTypeName = "Proposed", NFAApprovalSequenceType = 62, IsSystemGenerated = true };
            apList.Add(seq1);
            //if (pr.EmployeeID != AppContexts.User.EmployeeID)
            //{
            //    ManualApprovalPanelEmployeeDto seq2 = new ManualApprovalPanelEmployeeDto { APPanelID = (int)Util.ApprovalPanel.GRNBelowTheLimit, EmployeeID = pr.EmployeeID, IsProxyEmployeeEnabled = false, ProxyEmployeeID = 0, APTypeID = (int)Util.ApprovalType.QC, SequenceNo = 2, EmployeeName = pr.EmployeeCode + "-" + pr.EmployeeName, EmployeeCode = pr.EmployeeCode, NFAApprovalSequenceTypeName = "Prepared", NFAApprovalSequenceType = 55, IsSystemGenerated = true };
            //    apList.Add(seq2);
            //}
            return apList;
        }

    }



}
