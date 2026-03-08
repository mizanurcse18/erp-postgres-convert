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
    public class SCCManager : ManagerBase, ISCCManager
    {

        private readonly IRepository<SCCMaster> SCCRepo;
        private readonly IRepository<SCCChild> SCCChildRepo;
        private readonly IRepository<Item> ItemRepo;
        private readonly IRepository<RTVMaster> RTVMasterRepo;
        private readonly IRepository<RTVChild> RTVChildRepo;
        public SCCManager(IRepository<SCCMaster> qcMasterRepo, IRepository<SCCChild> qcChildRepo, IRepository<Item> itemRepo, IRepository<InventoryTransaction> inventoryTransactionRepo
            , IRepository<RTVMaster> rTVMasterRepo
            , IRepository<RTVChild> rTVChildRepo)
        {
            SCCRepo = qcMasterRepo;
            SCCChildRepo = qcChildRepo;
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
                        string filename = $"SCC-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "SCC\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

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
        private List<Attachment> RemoveAttachments(SCCDto SCC)
        {
            if (SCC.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachment>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='SCCMaster' AND ReferenceID={SCC.SCCMID}";
                var prevAttachment = SCCRepo.GetDataDictCollection(attachmentSql);

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
                var removeList = attachemntList.Where(x => !SCC.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "SCC";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        System.IO.File.Delete(str + "\\" + data.FileName);

                    }

                }
                return removeList;
            }

            return null;
        }
        private List<Attachment> RemoveProposedAttachments(SCCDto SCC)
        {
            if (SCC.ProposedAttachments.Count > 0)
            {
                var attachemntList = new List<Attachment>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='SCCMasterProposed' AND ReferenceID={SCC.SCCMID}";
                var prevAttachment = SCCRepo.GetDataDictCollection(attachmentSql);

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
                var removeList = attachemntList.Where(x => !SCC.ProposedAttachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "SCCProposed";
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

        public async Task<(bool, string)> SaveChanges(SCCDto SCC)
        {
            var existingMR = SCCRepo.Entities.Where(x => x.SCCMID == SCC.SCCMID).SingleOrDefault();
            if (SCC.SCCMID > 0 && (existingMR.IsNullOrDbNull() || existingMR.CreatedBy != AppContexts.User.UserID))
            {
                return (false, "You don't have permission to save this SCC");
            }


            foreach (var item in SCC.Attachments)
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

            var removeList = RemoveAttachments(SCC);
            var removeProposedList = RemoveProposedAttachments(SCC);


            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (SCC.SCCMID > 0 && SCC.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                                
                            FROM SCCMaster MR
                            LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MR.SCCMID AND AP.APTypeID = {(int)Util.ApprovalType.SCC}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID =  {(int)Util.ApprovalType.SCC} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID =  {(int)Util.ApprovalType.SCC} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MR.SCCMID                                   
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {SCC.ApprovalProcessID}";
                var canReassesstment = SCCRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, $"Sorry, You can not edit SCC once it processed from approval panel");
                    }
                }
                else
                {
                    return (false, $"Sorry, You can not edit SCC once it processed from approval panel");
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)SCC.SCCMID, SCC.ApprovalProcessID, (int)Util.ApprovalType.SCC);
            }

            string ApprovalProcessID = "0";
            var masterModel = new SCCMaster
            {
                InvoiceNoFromVendor = SCC.InvoiceNoFromVendor,
                InvoiceDateFromVendor = SCC.InvoiceDateFromVendor,
                InvoiceAmountFromVendor = SCC.InvoiceAmountFromVendor,
                ServicePeriodFrom = SCC.ServicePeriodFrom.IsNull() ? null : SCC.ServicePeriodFrom ,
                ServicePeriodTo = SCC.ServicePeriodTo.IsNull() ? null : SCC.ServicePeriodTo,
                PaymentType = SCC.PaymentType,
                PaymentFixedOrPercent = SCC.PaymentFixedOrPercent,
                PaymentFixedOrPercentAmount = SCC.PaymentFixedOrPercentAmount,
                PaymentFixedOrPercentTotalAmount = SCC.PaymentFixedOrPercentTotalAmount,
                SCCAmount = SCC.PaymentFixedOrPercentTotalAmount,
                PerformanceAssessment1 = SCC.PerformanceAssessment1,
                PerformanceAssessment2 = SCC.PerformanceAssessment2,
                PerformanceAssessment3 = SCC.PerformanceAssessment3,
                PerformanceAssessment4 = SCC.PerformanceAssessment4,
                PerformanceAssessment5 = SCC.PerformanceAssessment5,
                PerformanceAssessment6 = SCC.PerformanceAssessment6,
                PerformanceAssessmentComment = SCC.PerformanceAssessmentComment,

                SupplierID = SCC.SupplierID,
                PRMasterID = SCC.PRMasterID,
                POMasterID = SCC.POMasterID,
                TotalReceivedQty = (decimal)SCC.SCCItemDetails.Sum(x => x.ReceivedQty),
                ReferenceKeyword = SCC.ReferenceKeyword,
                ApprovalStatusID = SCC.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
                IsDraft = SCC.IsDraft
            };

            

            using (var unitOfWork = new UnitOfWork())
            {
                if (SCC.SCCMID.IsZero() && existingMR.IsNull())
                {
                    masterModel.ReferenceNo = GenerateSCCReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");
                    masterModel.SetAdded();
                    SetSCCNewId(masterModel);
                    SCC.SCCMID = (int)masterModel.SCCMID;

                   
                }
                else
                {
                    masterModel.CreatedBy = existingMR.CreatedBy;
                    masterModel.CreatedDate = existingMR.CreatedDate;
                    masterModel.CreatedIP = existingMR.CreatedIP;
                    masterModel.RowVersion = existingMR.RowVersion;
                    masterModel.SCCMID = SCC.SCCMID;
                    masterModel.ReferenceNo = existingMR.ReferenceNo;
                    //masterModel.ApprovalStatusID = existingMR.ApprovalStatusID;
                    masterModel.SetModified();

                   
                }
                var childModel = GenerateSCCChild(SCC);



                SetAuditFields(masterModel);
                SetAuditFields(childModel);


                if (SCC.Attachments.IsNotNull() && SCC.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(SCC.Attachments.Where(x => x.ID == 0).ToList());
                    var proposedAttachmentList = AddAttachments(SCC.ProposedAttachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)SCC.SCCMID, "SCCMaster", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)SCC.SCCMID, "SCCMaster", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }

                    //For Propossed Add new File
                    if (proposedAttachmentList.IsNotNull() && proposedAttachmentList.Count > 0)
                    {
                        foreach (var attachemnt in proposedAttachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)SCC.SCCMID, "SCCMasterProposed", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Propossed Attachment                    
                    if (removeProposedList.IsNotNull() && removeProposedList.Count > 0)
                    {
                        foreach (var attachemnt in removeProposedList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)SCC.SCCMID, "SCCMasterProposed", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }

                if (masterModel.IsAdded || (masterModel.IsDraft && masterModel.IsModified))
                {
                    if (SCC.SCCApprovalPanelList.IsNotNull() && SCC.SCCApprovalPanelList.Count > 0)
                    {
                        DeleteManualApprovalPanel((int)masterModel.SCCMID, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext), (int)Util.ApprovalType.SCC);
                        foreach (var item in SCC.SCCApprovalPanelList)
                        {
                            SaveManualApprovalPanel(item.EmployeeID, (int)Util.ApprovalPanel.SCC, item.SequenceNo, item.ProxyEmployeeID.Value, item.IsProxyEmployeeEnabled, item.NFAApprovalSequenceType.Value, item.IsEditable, item.IsSCM, item.IsMultiProxy, (int)Util.ApprovalType.SCC, (int)masterModel.SCCMID, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext));

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

                SCCRepo.Add(masterModel);
                SCCChildRepo.AddRange(childModel);


                unitOfWork.CommitChangesWithAudit();

                if (!masterModel.IsDraft && SCC.ApprovalProcessID == 0)
                {
                    string approvalTitle = $"{Util.SCCApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, SCC Reference No:{masterModel.ReferenceNo}";
                    var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
                    var obj = CreateManualApprovalProcess((int)masterModel.SCCMID, Util.AutoSCCAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.SCC, (int)Util.ApprovalPanel.SCC, context, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext));
                    ApprovalProcessID = obj.ApprovalProcessID;
                }
                if (!masterModel.IsDraft)
                {
                    if (ApprovalProcessID.ToInt() > 0)
                        // await Extension.Post<string>($"/SendMail/SendEMailToRecipients", "Test API Call");
                        await SendMailFromManagerBase(ApprovalProcessID, false, masterModel.SCCMID, (int)Util.MailGroupSetup.SCCInitiatedMail, (int)Util.ApprovalType.SCC);
                }
            }
            await Task.CompletedTask;

            return (true, $"SCC Submitted Successfully"); ;
        }

        private void SetSCCNewId(SCCMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("SCCMaster", AppContexts.User.CompanyID);
            master.SCCMID = code.MaxNumber;
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

        private List<SCCChild> GenerateSCCChild(SCCDto SCC)
        {
            var existingSCCChild = SCCChildRepo.Entities.Where(x => x.SCCMID == SCC.SCCMID).ToList();
            var childModel = new List<SCCChild>();
            if (SCC.SCCItemDetails.IsNotNull())
            {
                SCC.SCCItemDetails.ForEach(x =>
                {
                    //if (x.ReceivedQty > 0)
                    //{
                        childModel.Add(new SCCChild
                        {
                            SCCCID = x.SCCCID,
                            SCCMID = SCC.SCCMID,
                            POCID = x.POCID,
                            ItemID = x.ItemID,
                            ReceivedQty = x.ReceivedQty,
                            SCCCNote = x.SCCCNote,
                            Rate = x.Rate,
                            TotalAmount = x.TotalAmount,
                            TotalAmountIncludingVat = x.TotalAmountIncludingVat,
                            VatAmount = x.VatAmount,
                            InvoiceAmount = x.InvoiceAmount
                        });
                    //}

                });

                childModel.ForEach(x =>
                {
                    if (existingSCCChild.Count > 0 && x.SCCCID > 0)
                    {
                        var existingModelData = existingSCCChild.FirstOrDefault(y => y.SCCCID == x.SCCCID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.SCCMID = SCC.SCCMID;
                        x.SetAdded();
                        //SetSCCChildNewId(x);
                    }
                });

                var willDeleted = existingSCCChild.Where(x => !childModel.Select(y => y.SCCCID).Contains(x.SCCCID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    childModel.Add(x);
                });
            }

            return childModel;
        }

        private void SetSCCChildNewId(SCCChild child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("SCCChild", AppContexts.User.CompanyID);
            child.SCCCID = code.MaxNumber;
        }

        private string GenerateSCCReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/SCC/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{ GenerateSystemCode("SCCMasterRefNo", AppContexts.User.CompanyID).MaxNumber}";
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
            string format = @$"{AppContexts.User.CompanyShortCode}/RTV/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{ GenerateSystemCode("SCCMasterRefNo", AppContexts.User.CompanyID).SystemCode}";
            return format;
        }

        public GridModel GetAllSCCList(GridParameter parameters)
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
                    filter = $@" AND SCC.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND SCC.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
	                             SCC.SCCMID
								,SCC.CreatedBy
								,SCC.ReferenceNo
								,SCC.ApprovalStatusID
								,SCC.CreatedDate
                                ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
                                ,PO.ReferenceNo AS PONo
                                ,PR.ReferenceNo AS PRNo
                                ,S.SupplierName
								,ISNULL(MR.MRID, 0) MRID
                                ,ISNULL(MR.ReferenceNo, '') GRNNo
                                ,PO.ReferenceNo + ' ' + PR.ReferenceNo AS POPRNo
                                ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
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
                            FROM SCCMaster SCC
                            LEFT JOIN PurchaseOrderMaster PO ON PO.POMasterID = SCC.POMasterID
                            LEFT JOIN PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID
						    LEFT JOIN Supplier S ON SCC.SupplierID = S.SupplierID
                            LEFT JOIN Security..Users U ON U.UserID = SCC.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = SCC.ApprovalStatusID
							LEFT JOIN MaterialReceive MR ON MR.SCCMID = SCC.SCCMID  AND MR.ApprovalStatusID != 24
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = SCC.SCCMID AND AP.APTypeID = {(int)Util.ApprovalType.SCC}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.SCC} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.SCC} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = SCC.SCCMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = SCC.SCCMID
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
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.SCC} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = SCC.SCCMID
                                    {where} {filter}
                                    ";
            //WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
            var result = SCCRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }

        //SCC Approved List

        public GridModel GetSCCList(GridParameter parameters)
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
                    filter = $@" AND SCC.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND SCC.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
	                             SCC.SCCMID
								,SCC.CreatedBy
								,SCC.ReferenceNo
								,SCC.ApprovalStatusID
								,SCC.CreatedDate
	                            ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
                                ,PO.POMasterID
                                ,PO.ReferenceNo AS PONo
                                ,PR.ReferenceNo AS PRNo
                                ,PR.PRMasterID
                                ,S.SupplierName
                                ,PO.ReferenceNo + ' ' + PR.ReferenceNo AS POPRNo
                                ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                                ,ISNULL(AP1.ApprovalProcessID, 0) PRApprovalProcessID
                                ,ISNULL(APPO.ApprovalProcessID, 0) POApprovalProcessID
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
                            FROM SCCMaster SCC
                            LEFT JOIN PurchaseOrderMaster PO ON PO.POMasterID = SCC.POMasterID
                            LEFT JOIN PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID
						    LEFT JOIN Supplier S ON SCC.SupplierID = S.SupplierID
                            LEFT JOIN Security..Users U ON U.UserID = SCC.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = SCC.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = SCC.SCCMID AND AP.APTypeID = {(int)Util.ApprovalType.SCC}
                            LEFT JOIN Approval..ApprovalProcess AP1 ON AP1.ReferenceID = PO.PRMasterID AND AP1.APTypeID = {(int)Util.ApprovalType.PR}
                            LEFT JOIN Approval..ApprovalProcess APPO ON APPO.ReferenceID = PO.POMasterID AND APPO.APTypeID = {(int)Util.ApprovalType.PO}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.SCC} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.SCC} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = SCC.SCCMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = SCC.SCCMID
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
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.SCC} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = SCC.SCCMID WHERE 
        (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
                            ";
            //WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
            var result = SCCRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }

        public GridModel GetSCCListAll(GridParameter parameters)
        {
            string filter = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "1=1";
                    break;
                case "My Pending":
                    filter = $@"CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@"SCC.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@"SCC.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
                    break;
                case "My Approved":
                    filter = $@"AP.ApprovalProcessID IN (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Approved})) AND SCC.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved} AND VA.EmployeeID = {AppContexts.User.EmployeeID}";
                    break;
                case "MyRejectReturnForwarded":
                    filter = $@"AP.ApprovalProcessID IN 
                                (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Rejected})
                                UNION  
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Returned})
                                UNION 
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Forwarded}))";
                    break;
                default:
                    break;
            }

            string sql = $@" SELECT X.*,COALESCE(INVCC.InvoiceMasterID, 0) InvoiceID
								,COALESCE(INV.ApprovalStatusID, 0) InvoiceApprovalStatusID
								,CASE WHEN COALESCE(INV.ApprovalStatusID, 0) = 24 THEN 1 ELSE 0 END InvoiceRejectStatus
								from (SELECT DISTINCT
	                             SCC.SCCMID
								,SCC.CreatedBy
								,SCC.ReferenceNo
								,SCC.ApprovalStatusID
								,SCC.CreatedDate
	                            ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
                                ,PO.POMasterID
                                ,PO.ReferenceNo AS PONo
                                ,PR.ReferenceNo AS PRNo
                                ,PR.PRMasterID
                                ,S.SupplierName
                                ,PO.ReferenceNo + ' ' + PR.ReferenceNo AS POPRNo
                                ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                                ,ISNULL(AP1.ApprovalProcessID, 0) PRApprovalProcessID
                                ,ISNULL(APPO.ApprovalProcessID, 0) POApprovalProcessID
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
                            FROM SCCMaster SCC
                            LEFT JOIN PurchaseOrderMaster PO ON PO.POMasterID = SCC.POMasterID
                            LEFT JOIN PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID
						    LEFT JOIN Supplier S ON SCC.SupplierID = S.SupplierID
                            LEFT JOIN Security..Users U ON U.UserID = SCC.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = SCC.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = SCC.SCCMID AND AP.APTypeID = {(int)Util.ApprovalType.SCC}
                            LEFT JOIN Approval..ApprovalProcess AP1 ON AP1.ReferenceID = PO.PRMasterID AND AP1.APTypeID = {(int)Util.ApprovalType.PR}
                            LEFT JOIN Approval..ApprovalProcess APPO ON APPO.ReferenceID = PO.POMasterID AND APPO.APTypeID = {(int)Util.ApprovalType.PO}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.SCC} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.SCC} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = SCC.SCCMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = SCC.SCCMID
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
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.SCC} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = SCC.SCCMID WHERE {filter} ) X
                                    LEFT JOIN SCM..InvoiceChildSCC INVCC  ON INVCC.SCCMID = X.SCCMID
									LEFT JOIN SCM..InvoiceMaster INV ON INV.InvoiceMasterID = INVCC.InvoiceMasterID 
                            ";
            //WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
            var result = SCCRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }

        public async Task UpdateSCCMasterAfterReset(SCCMasterDto SCC)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existingSCC = SCCRepo.Entities.Where(x => x.POMasterID == SCC.POMasterID).ToList();
                if (existingSCC.Count()>0 )
                {
                    foreach (var item in existingSCC)
                    {
                        item.CreatedBy = item.CreatedBy;
                        item.CreatedDate = item.CreatedDate;
                        item.CreatedIP = item.CreatedIP;
                        item.RowVersion = item.RowVersion;
                        item.POMasterID = SCC.POMasterID;
                        item.ReferenceNo = item.ReferenceNo;
                        item.ApprovalStatusID = 22;
                        item.SetModified();

                        SetAuditFields(item);
                        SCCRepo.Add(item);
                    }
                }
               
                unitOfWork.CommitChangesWithAudit();
            }
            await Task.CompletedTask;
        }

        public async Task<SCCMasterDto> GetSCCMaster(int SCCMID)
        {
            string sql = $@"SELECT DISTINCT SCC.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName, VA1.FullName AS POEmployeeName, VA1.EmployeeCode POEmployeeCode, VA1.DivisionID AS PODivisionID, VA1.DivisionName AS PODivisionName
                            ,PO.Remarks AS PORemarks
                            ,PO.ReferenceNo AS PONo
                            ,PR.ReferenceNo PRNo
                            ,PO.PODate
                            ,PO.GrandTotal
                            ,W.WarehouseName DeliveryLocationName,VA.WorkMobile
							,S.SupplierName

	                        ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee](353,ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            ,ISNULL(AP1.ApprovalProcessID, 0) PRApprovalProcessID
                            ,ISNULL(APPO.ApprovalProcessID, 0) POApprovalProcessID
                        from SCCMaster SCC
                        LEFT JOIN Security..Users U ON U.UserID = SCC.CreatedBy
						LEFT JOIN PurchaseOrderMaster PO ON PO.POMasterID = SCC.POMasterID
                        LEFT JOIN PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID
						LEFT JOIN Supplier S ON SCC.SupplierID = S.SupplierID
                        LEFT JOIN Security..Users U1 ON U1.UserID = PO.CreatedBy
                        LEFT JOIN Warehouse W ON PO.DeliveryLocation=W.WarehouseID
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN HRMS..ViewALLEmployee VA1 ON VA1.PersonID = U1.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = SCC.ApprovalStatusID

                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = SCC.SCCMID AND AP.APTypeID = {(int)Util.ApprovalType.SCC}
                        LEFT JOIN Approval..ApprovalProcess AP1 ON AP1.ReferenceID = PO.PRMasterID AND AP1.APTypeID = {(int)Util.ApprovalType.PR}
                        LEFT JOIN Approval..ApprovalProcess APPO ON APPO.ReferenceID = PO.POMasterID AND APPO.APTypeID = {(int)Util.ApprovalType.PO}
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.SCC}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.SCC} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = SCC.SCCMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = SCC.SCCMID
                                LEFT JOIN 
								(
								    SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID

                        WHERE SCC.SCCMID={SCCMID}  AND (VA.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID})";
            var master = SCCRepo.GetModelData<SCCMasterDto>(sql);
            return master;
        }

        public async Task<SCCMasterDto> GetSCCMasterAll(int SCCMID)
        {
            string sql = $@"SELECT DISTINCT SCC.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName, VA1.FullName AS POEmployeeName, VA1.EmployeeCode POEmployeeCode, VA1.DivisionID AS PODivisionID, VA1.DivisionName AS PODivisionName
                            ,PO.Remarks AS PORemarks
                            ,PO.ReferenceNo AS PONo
                            ,PR.ReferenceNo PRNo
                            ,PO.PODate
                            ,PO.GrandTotal
                            ,W.WarehouseName DeliveryLocationName,VA.WorkMobile
							,S.SupplierName

	                        ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee](353,ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            ,ISNULL(AP1.ApprovalProcessID, 0) PRApprovalProcessID
                            ,ISNULL(APPO.ApprovalProcessID, 0) POApprovalProcessID
                        from SCCMaster SCC
                        LEFT JOIN Security..Users U ON U.UserID = SCC.CreatedBy
						LEFT JOIN PurchaseOrderMaster PO ON PO.POMasterID = SCC.POMasterID
                        LEFT JOIN PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID
						LEFT JOIN Supplier S ON SCC.SupplierID = S.SupplierID
                        LEFT JOIN Security..Users U1 ON U1.UserID = PO.CreatedBy
                        LEFT JOIN Warehouse W ON PO.DeliveryLocation=W.WarehouseID
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN HRMS..ViewALLEmployee VA1 ON VA1.PersonID = U1.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = SCC.ApprovalStatusID

                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = SCC.SCCMID AND AP.APTypeID = {(int)Util.ApprovalType.SCC}
                        LEFT JOIN Approval..ApprovalProcess AP1 ON AP1.ReferenceID = PO.PRMasterID AND AP1.APTypeID = {(int)Util.ApprovalType.PR}
                        LEFT JOIN Approval..ApprovalProcess APPO ON APPO.ReferenceID = PO.POMasterID AND APPO.APTypeID = {(int)Util.ApprovalType.PO}
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.SCC}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.SCC} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = SCC.SCCMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = SCC.SCCMID
                                LEFT JOIN 
								(
								    SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID

                        WHERE SCC.SCCMID={SCCMID}";
            var master = SCCRepo.GetModelData<SCCMasterDto>(sql);
            return master;
        }
        public async Task<SCCMasterDto> GetSCCMasterFromAllList(int SCCMID)
        {
            string sql = $@"SELECT SCC.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName, VA1.FullName AS POEmployeeName, VA1.EmployeeCode POEmployeeCode, VA1.DivisionID AS PODivisionID, VA1.DivisionName AS PODivisionName
                            ,PO.Remarks AS PORemarks
                            ,PO.ReferenceNo AS PONo
                            ,W.WarehouseName DeliveryLocationName,VA.WorkMobile
							,S.SupplierName
                            ,ISNULL(MR.MRID, 0) MRID
                            ,ISNULL(RM.RTVMID,0) AS RTVMID
                            ,(SELECT REPLACE(SCC.ReferenceNo, 'SCC', 'GRN' )) as GRNNo

	                        ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee](353,ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            --,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
                        from SCCMaster SCC
                        LEFT JOIN Security..Users U ON U.UserID = SCC.CreatedBy
						LEFT JOIN PurchaseOrderMaster PO ON PO.POMasterID = SCC.POMasterID
						LEFT JOIN Supplier S ON SCC.SupplierID = S.SupplierID
                        LEFT JOIN Security..Users U1 ON U1.UserID = PO.CreatedBy
                        LEFT JOIN Warehouse W ON PO.DeliveryLocation=W.WarehouseID
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN HRMS..ViewALLEmployee VA1 ON VA1.PersonID = U1.PersonID
						LEFT JOIN MaterialReceive MR ON MR.SCCMID = SCC.SCCMID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = SCC.ApprovalStatusID
                        LEFT JOIN RTVMaster RM ON SCC.SCCMID = RM.SCCMID

                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = SCC.SCCMID AND AP.APTypeID = {(int)Util.ApprovalType.SCC} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.SCC}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.SCC} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = SCC.SCCMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = SCC.SCCMID

                        WHERE SCC.SCCMID={SCCMID}";
            var master = SCCRepo.GetModelData<SCCMasterDto>(sql);
            return master;
        }

        
        public async Task<List<SCCChildDto>> GetSCCChild(int SCCMID)
        {
            string sql = $@"SELECT  SCCC.*,
					        VA.DepartmentID,
					        VA.DepartmentName,
					        VA.FullName AS EmployeeName,
					        SV.SystemVariableCode AS ApprovalStatus,
					        VA.ImagePath,
					        VA.EmployeeCode,
					        VA.DivisionID,
					        VA.DivisionName
					        ,I.ItemName, I.ItemCode,Unit.UnitCode
                            ,I.ItemDescription
                            ,POC.Qty AS POQty
                            ,POC.Rate
                            ,POC.Description AS PODescription
							,POC.VatPercent POVatPercent
							,ISNULL(PO.ReceivedQty, 0) AlreadyReceivedQty
                    FROM SCCChild SCCC
                        LEFT JOIN SCCMaster SCCM ON SCCM.SCCMID = SCCC.SCCMID
                        LEFT JOIN Item I ON I.ItemID=SCCC.ItemID
                        LEFT JOIN Security..Users U ON U.UserID = SCCC.CreatedBy
						LEFT JOIN PurchaseOrderChild POC ON POC.POMasterID = SCCM.POMasterID AND POC.ItemID = SCCC.ItemID AND POC.POCID = SCCC.POCID
						
						LEFT JOIN (	SELECT POC1.ItemID,SUM(POC1.ReceivedQty) ReceivedQty,POMasterID FROM SCCChild POC1
									JOIN SCCMaster POM1 ON POC1.SCCMID=POM1.SCCMID 
									WHERE POM1.ApprovalStatusID	<> 24 AND POM1.SCCMID!= {SCCMID}
									GROUP BY POC1.ItemID,POMasterID) PO ON POC.ItemID=PO.ItemID  AND PO.POMasterID=SCCM.POMasterID
                        LEFT JOIN Security..Unit  ON Unit.UnitID=POC.UOM
                        LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = SCCM.ApprovalStatusID
                        WHERE SCCC.SCCMID={SCCMID}";
            var childs = SCCChildRepo.GetDataModelCollection<SCCChildDto>(sql);
            return childs;
        }


        public async Task<RTVMasterDto> GetRTVMaster(int SCCMID)
        {
            string sql = $@"SELECT RTV.*,S.SupplierName,PO.ReferenceNo AS PONo
                        from RTVMaster RTV
						LEFT JOIN PurchaseOrderMaster PO ON PO.POMasterID = RTV.POMasterID
						LEFT JOIN Supplier S ON RTV.SupplierID = S.SupplierID
                        WHERE RTV.SCCMID={SCCMID}";
            var master = SCCRepo.GetModelData<RTVMasterDto>(sql);
            return master;
        }

        public async Task<List<RTVChildDto>> GetRTVChild(int SCCMID)
        {
            string sql = $@"SELECT  RTVC.*,I.ItemName, I.ItemCode,Unit.UnitCode
                        FROM RTVChild RTVC
                        LEFT JOIN RTVMaster RTVM ON RTVM.RTVMID = RTVC.RTVMID
                        LEFT JOIN SCCMaster SCCM ON SCCM.SCCMID = RTVM.SCCMID
                        LEFT JOIN Item I ON I.ItemID=RTVC.ItemID
						LEFT JOIN PurchaseOrderChild POC ON POC.POMasterID = SCCM.POMasterID AND POC.ItemID = RTVC.ItemID
						
                        LEFT JOIN Security..Unit  ON Unit.UnitID=POC.UOM   
                        WHERE RTVM.SCCMID={SCCMID} AND RTVC.ReturnQty > 0";
            var childs = SCCChildRepo.GetDataModelCollection<RTVChildDto>(sql);
            return childs;
        }


        public async Task<List<ManualApprovalPanelEmployeeDto>> GetSCCApprovalPanelDefault(int SCCMID)
        {
            //     string sql = $@"SELECT APE.*, Emp.EmployeeCode, Emp.FullName AS EmployeeName, EmpPr.FullName AS ProxyEmployeeName, AP.Name AS PanelName, SV.SystemVariableCode AS NFAApprovalSequenceTypeName                     
            //                     FROM Approval..ManualApprovalPanelEmployee APE
            //                     LEFT JOIN HRMS..ViewALLEmployee Emp ON APE.EmployeeID = Emp.EmployeeID							
            //                     LEFT JOIN HRMS..Employee EmpPr ON APE.ProxyEmployeeID = EmpPr.EmployeeID					
            //                     LEFT JOIN Approval..ApprovalPanel AP ON APE.APPanelID = AP.APPanelID
            //LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = APE.NFAApprovalSequenceType
            //                     WHERE APE.ReferenceID={SCCMID} AND APE.APTypeID ={(int)Util.ApprovalType.SCC}";

            string sql = $@"SELECT APE.*, CASE WHEN Emp1.EmployeeID = APE.EmployeeID THEN 1 ELSE 0 END IsSystemGenerated, Emp.EmployeeCode, Emp.FullName AS EmployeeName, EmpPr.FullName AS ProxyEmployeeName, AP.Name AS PanelName, SV.SystemVariableCode AS NFAApprovalSequenceTypeName                     
                            FROM Approval..ManualApprovalPanelEmployee APE
                            LEFT JOIN HRMS..ViewALLEmployee Emp ON APE.EmployeeID = Emp.EmployeeID							
                            LEFT JOIN HRMS..Employee EmpPr ON APE.ProxyEmployeeID = EmpPr.EmployeeID					
                            LEFT JOIN Approval..ApprovalPanel AP ON APE.APPanelID = AP.APPanelID
							LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = APE.NFAApprovalSequenceType

                            LEFT JOIN SCCMaster SCC ON SCC.SCCMID={SCCMID}
							LEFT JOIN PurchaseRequisitionMaster PR ON SCC.PRMasterID = PR.PRMasterID
							LEFT JOIN Security..Users U ON PR.CreatedBy = U.UserID
							LEFT JOIN HRMS..ViewALLEmployee Emp1 ON U.PersonID=Emp1.PersonID

                            WHERE APE.ReferenceID={SCCMID} AND APE.APTypeID ={(int)Util.ApprovalType.SCC}";

            var maps = SCCRepo.GetDataModelCollection<ManualApprovalPanelEmployeeDto>(sql);

            return maps;
        }

        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.SCC);
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

        public IEnumerable<Dictionary<string, object>> ReportForSCCApprovalFeedback(int SCCMID)
        {
            string sql = $@" EXEC SCM..spRPTSCCApprovalFeedback {SCCMID}";
            var feedback = SCCRepo.GetDataDictCollection(sql);
            return feedback;
        }

        public IEnumerable<Dictionary<string, object>> ReportForGRNApprovalFeedback(int SCCMID)
        {
            string sql = $@" EXEC SCM..spRPTPOApprovalFeedback {SCCMID}";
            var feedback = SCCRepo.GetDataDictCollection(sql);
            return feedback;
        }
        public List<Attachments> GetAttachments(int SCCMID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='SCCMaster' AND ReferenceID={SCCMID}";
            var attachment = SCCRepo.GetDataDictCollection(attachmentSql);
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

        public List<Attachments> GetProposedAttachments(int SCCMID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='SCCMasterProposed' AND ReferenceID={SCCMID}";
            var attachment = SCCRepo.GetDataDictCollection(attachmentSql);
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

        public List<ManualApprovalPanelEmployeeDto> GetGRNApprovalPanelDefault(int SCCMID)
        {
            string sql = $@"SELECT Emp.FullName AS EmployeeName, Emp.EmployeeCode,Emp.EmployeeID 
                        FROM SCCMaster SCC 
                        LEFT JOIN PurchaseRequisitionMaster PR ON SCC.PRMasterID = PR.PRMasterID
                        LEFT JOIN Security..Users U ON PR.CreatedBy = U.UserID
                        LEFT JOIN HRMS..ViewALLEmployee Emp ON U.PersonID=Emp.PersonID
                        WHERE SCC.SCCMID={SCCMID}";

            var pr = SCCRepo.GetModelData<SCCMasterDto>(sql);

            List<ManualApprovalPanelEmployeeDto> apList = new List<ManualApprovalPanelEmployeeDto>();
            ManualApprovalPanelEmployeeDto seq1 = new ManualApprovalPanelEmployeeDto { APPanelID = (int)Util.ApprovalPanel.GRNBelowTheLimit, EmployeeID = AppContexts.User.EmployeeID.Value, IsProxyEmployeeEnabled = false, ProxyEmployeeID = 0, APTypeID = (int)Util.ApprovalType.SCC, SequenceNo = 1, EmployeeCode = AppContexts.User.EmployeeCode, EmployeeName = AppContexts.User.EmployeeCode + "-" + AppContexts.User.FullName, NFAApprovalSequenceTypeName = "Proposed", NFAApprovalSequenceType = 62, IsSystemGenerated = true };
            apList.Add(seq1);
            //if (pr.EmployeeID != AppContexts.User.EmployeeID)
            //{
            //    ManualApprovalPanelEmployeeDto seq2 = new ManualApprovalPanelEmployeeDto { APPanelID = (int)Util.ApprovalPanel.GRNBelowTheLimit, EmployeeID = pr.EmployeeID, IsProxyEmployeeEnabled = false, ProxyEmployeeID = 0, APTypeID = (int)Util.ApprovalType.SCC, SequenceNo = 2, EmployeeName = pr.EmployeeCode + "-" + pr.EmployeeName, EmployeeCode = pr.EmployeeCode, NFAApprovalSequenceTypeName = "Prepared", NFAApprovalSequenceType = 55, IsSystemGenerated = true };
            //    apList.Add(seq2);
            //}
            return apList;
        }
        public async Task<List<SCCChildDto>> GetSCCChildForAllItem(int POMasterID,int SCCMID)
        {
            string sql = $@"SELECT POC.POCID POCID
                            ,ISNULL(SCCC.SCCCID,0) SCCCID
                            ,SCCC.SCCCNote
							,POC.ItemID
							,POC.Description
							,POC.UOM
							,POC.Qty
							,POC.Rate
							,POC.Amount
							,POC.VatInfoID
							,POC.VatPercent
							,POC.IsRebateable
							,POC.RebatePercentage
							,POC.TotalAmountIncludingVat
							,POC.POMasterID
							,POC.PRCID
							,POC.PRQID
							,POC.VatAmount
	                        ,VA.DepartmentID
	                        ,VA.DepartmentName
	                        ,VA.FullName AS EmployeeName
	                        ,SV.SystemVariableCode AS ApprovalStatus
	                        ,VA.ImagePath
	                        ,VA.EmployeeCode
	                        ,VA.DivisionID
	                        ,VA.DivisionName
	                        ,I.ItemName
	                        ,I.ItemCode
	                        ,Unit.UnitCode
	                        ,POC.Qty AS POQty
	                        ,POC.Rate AS Price
	                        ,I.InventoryTypeID
                            ,I.ItemDescription
							,ISNULL(SCCC.SCCReceivedQty,0) ReceivedQty
                            ,ISNULL(TotalSCCReceivedQty,0) SccReceivedQty
	                        ,ISNULL(POC.Qty, 0) - ISNULL(TotalSCCReceivedQty, 0) AS BalanceQty
                            ,ISNULL(SCCC.InvoiceAmount, 0) InvoiceAmount
                            ,SCCC.DeliveryOrJobCompletionDate
                        FROM PurchaseOrderChild POC
                        LEFT JOIN PurchaseOrderMaster POM ON POM.POMasterID = POC.POMasterID
                        LEFT JOIN Item I ON I.ItemID = POC.ItemID
						LEFT JOIN (
						SELECT ItemID,SCCCID,MRC.SCCCNote,MRC.POCID,
		                       MRc.ReceivedQty SCCReceivedQty, MRC.InvoiceAmount, DeliveryOrJobCompletionDate
	                        FROM SCCChild MRC
	                        LEFT JOIN SCCMaster MR ON MRC.SCCMID = MR.SCCMID
							where MR.SCCMID = {SCCMID}
						) SCCC ON SCCC.ItemID = POC.ItemID AND SCCC.POCID = POC.POCID
						LEFT JOIN (SELECT ItemID,MRC.POCID,
		                       SUM(MRc.ReceivedQty) TotalSCCReceivedQty,
							   POMasterID
	                        FROM SCCChild MRC
	                       LEFT JOIN SCCMaster MR ON MRC.SCCMID = MR.SCCMID
							where  MR.SCCMID <> {SCCMID}
							GROUP BY ItemID,POCID,POMasterID
							)Total ON Total.ItemID = POC.ItemID AND Total.POCID = POC.POCID AND Total.POMasterID = POC.POMasterID
                        
                        --LEFT JOIN (
	                       -- SELECT ItemID
		                      --  ,SUM(MRC.ReceivedQty) TotalSCCReceivedQty
		                  --      ,MR.POMasterID
                          --      ,SUM(MRC.InvoiceAmount) InvoiceAmount
	                     --   FROM SCCChild MRC
	                     --   LEFT JOIN SCCMaster MR ON MRC.SCCMID = MR.SCCMID AND MR.ApprovalStatusID <> 24
	                     --   GROUP BY POMasterID
		                 --       ,ItemID
	                     --   ) Total ON POC.ItemID = Total.ItemID
	                     --   AND Total.POMasterID = POM.POMasterID
                        LEFT JOIN Security..Unit ON Unit.UnitID = POC.UOM
                        LEFT JOIN Security..Users U ON U.UserID = POC.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = POM.ApprovalStatusID
                        WHERE POC.POMasterID = {POMasterID} AND (ISNULL(POC.Qty, 0) - ISNULL(TotalSCCReceivedQty, 0) > 0 )";
            var childs = SCCRepo.GetDataModelCollection<SCCChildDto>(sql);
            return childs;
        }
    }



}
