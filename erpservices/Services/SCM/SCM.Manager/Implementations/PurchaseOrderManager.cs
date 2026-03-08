using Core;
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
using static Core.Util;

namespace SCM.Manager.Implementations
{
    public class PurchaseOrderManager : ManagerBase, IPurchaseOrderManager
    {

        private readonly IRepository<PurchaseOrderMaster> PurchaseOrderMasterRepo;
        private readonly IRepository<PurchaseOrderChild> PurchaseOrderChildRepo;
        private readonly IRepository<QCMaster> QCMasterRepo;
        private readonly IRepository<Item> ItemRepo;
        private readonly IRepository<PurchaseRequisitionMaster> PurchaseRequisitionMasterRepo;
        public PurchaseOrderManager(IRepository<PurchaseOrderMaster> purchaseOrderMasterRepo, IRepository<PurchaseOrderChild> purchaseOrderChildRepo, IRepository<Item> itemRepo, IRepository<QCMaster> qcMasterRepo, IRepository<PurchaseRequisitionMaster> purchaseRequisitionMasterRepo)
        {
            PurchaseOrderMasterRepo = purchaseOrderMasterRepo;
            PurchaseOrderChildRepo = purchaseOrderChildRepo;
            QCMasterRepo = qcMasterRepo;
            ItemRepo = itemRepo;
            PurchaseRequisitionMasterRepo = purchaseRequisitionMasterRepo;
        }

        public async Task<(bool, string)> SaveChanges(PurchaseOrderDto PO)
        {

            //var exitingPoBalance = GetPurchaseRequisitionForBalance((int)PO.PRMasterID).Select(x => new { x.PRCID, x.PRQID, x.ItemID, x.PurchasedQty, x.PurchasedAmount, x.Qty, x.PRAmount });
            var exitingPoBalance = GetPurchaseRequisitionForBalance((int)PO.PRMasterID, (int)PO.POMasterID);

            bool isValidBalance = true;
            foreach (var data in PO.ItemDetails)
            {
                //var checkOverdBalance = exitingPoBalance.Any(x => x.ItemID == data.ItemID && (data.POQty > (x.Qty + x.PurchasedQty) || (data.TotalAmountIncludingVat > (x.PRAmount + x.PurchasedAmount))));
                var checkOverdBalance = exitingPoBalance.Any(x => x.PRQID == data.PRCID && (data.POQty > (x.Qty - x.PurchasedQty) || (data.TotalAmountIncludingVat > (x.PRAmount - x.PurchasedAmount))));
                if (checkOverdBalance)
                {
                    isValidBalance = false;
                    return (false, $"Purchase Quantity Or Amount Exceed PR Quantity Or Amount");
                }
            }
            if (!isValidBalance)
            {
                return (false, $"Purchase Quantity Or Amount Exceed PR Quantity Or Amount");
            }
            var existingPO = PurchaseOrderMasterRepo.Entities.Where(x => x.POMasterID == PO.POMasterID).SingleOrDefault();
            if (PO.POMasterID > 0 && (existingPO.IsNullOrDbNull() || existingPO.CreatedBy != AppContexts.User.UserID))
            {
                return (false, "You don't have permission to save this PO.");
            }


            foreach (var item in PO.Attachments)
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

            var removeList = RemoveAttachments(PO);



            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (PO.POMasterID > 0 && PO.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                                
                            FROM PurchaseOrderMaster PurchaseOrder
                            LEFT JOIN Security..Users U ON U.UserID = PurchaseOrder.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PurchaseOrder.POMasterID AND AP.APTypeID = 8
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = 8 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = 8 AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PurchaseOrder.POMasterID                                       
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {PO.ApprovalProcessID}";
                var canReassesstment = PurchaseOrderMasterRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, $"Sorry, You can not edit Purchase Order once it processed from approval panel");
                    }
                }
                else
                {
                    return (false, $"Sorry, You can not edit Purchase Order once it processed from approval panel");
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)PO.POMasterID, PO.ApprovalProcessID, (int)Util.ApprovalType.PO);
            }

            string ApprovalProcessID = "0";
            var masterModel = new PurchaseOrderMaster
            {
                PODate = PO.PODate,
                DeliveryWithinDate = PO.DeliveryWithinDate,
                ContactPerson = PO.ContactPerson,
                ContactNumber = PO.ContactNumber,
                Remarks = PO.PORemarks,
                QuotationNo = PO.QuotationNo,
                QuotationDate = PO.QuotationDate,
                PaymentTermsID = PO.PaymentTermsID,
                InventoryTypeID = PO.InventoryTypeID,
                SCMRemarks = PO.SCMRemarks,
                PRMasterID = PO.PRMasterID,
                SupplierID = PO.SupplierID,
                CreditDay = PO.CreditDay,

                BudgetPlanRemarks = PO.BudgetPlanRemarks,
                GrandTotal = PO.ItemDetails.Select(x => x.TotalAmountIncludingVat).DefaultIfEmpty(0).Sum(),
                TotalWithoutVatAmount = PO.ItemDetails.Select(x => x.Amount).DefaultIfEmpty(0).Sum(),
                TotalVatAmount = PO.ItemDetails.Select(x => x.TotalAmountIncludingVat).DefaultIfEmpty(0).Sum() - PO.ItemDetails.Select(x => x.Amount).DefaultIfEmpty(0).Sum(),
                ReferenceKeyword = PO.ReferenceKeyword,
                DeliveryLocation = PO.DeliveryLocation,
                IsDraft = PO.IsDraft,
                ApprovalStatusID = PO.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
            };

            using (var unitOfWork = new UnitOfWork())
            {
                if (PO.POMasterID.IsZero() && existingPO.IsNull())
                {
                    //masterModel.PODate = DateTime.Now;
                    masterModel.ReferenceNo = GeneratePurchaseOrderReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");

                    masterModel.SetAdded();
                    SetPurchaseOrderMasterNewId(masterModel);
                    PO.POMasterID = (int)masterModel.POMasterID;
                }
                else
                {
                    masterModel.CreatedBy = existingPO.CreatedBy;
                    masterModel.CreatedDate = existingPO.CreatedDate;
                    masterModel.CreatedIP = existingPO.CreatedIP;
                    masterModel.RowVersion = existingPO.RowVersion;
                    masterModel.POMasterID = PO.POMasterID;
                    masterModel.ReferenceNo = existingPO.ReferenceNo;
                    //masterModel.PODate = existingPO.PODate;
                    //masterModel.BudgetPlanRemarks = existingPO.BudgetPlanRemarks;
                    //masterModel.ApprovalStatusID = existingPO.ApprovalStatusID;
                    masterModel.SetModified();
                }
                var childModel = GeneratePurchaseOrderChild(PO);

                SetAuditFields(masterModel);
                SetAuditFields(childModel);



                if (PO.Attachments.IsNotNull() && PO.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(PO.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)PO.POMasterID, "PurchaseOrderMaster", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)PO.POMasterID, "PurchaseOrderMaster", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }

                PurchaseOrderMasterRepo.Add(masterModel);
                PurchaseOrderChildRepo.AddRange(childModel);

                if (!masterModel.IsDraft)
                {
                    if (masterModel.IsAdded || (existingPO.IsDraft && masterModel.IsModified))
                    {
                        string approvalTitle = $"{Util.POApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, Purchase Order Reference No:{masterModel.ReferenceNo}";
                        var obj = CreateApprovalProcessForLimit((int)masterModel.POMasterID, Util.AutoPOAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.PO, masterModel.GrandTotal, "0");
                        ApprovalProcessID = obj.ApprovalProcessID;
                    }
                    else
                    {
                        if (approvalProcessFeedBack.Count > 0)
                        {
                            UpdateApprovalProcessFeedback((int)approvalProcessFeedBack["ApprovalProcessID"],
                                (int)approvalProcessFeedBack["APEmployeeFeedbackID"], (int)Util.ApprovalFeedback.Approved,
                                !string.IsNullOrWhiteSpace(PO.Comment) ? PO.Comment : $@"Reviewed And Resubmited by {AppContexts.User.FullName} - {AppContexts.User.EmployeeCode}",
                                (int)approvalProcessFeedBack["APTypeID"],
                                (int)approvalProcessFeedBack["ReferenceID"], 0);
                            ApprovalProcessID = approvalProcessFeedBack["ApprovalProcessID"].ToString();
                        }
                    }
                }
                unitOfWork.CommitChangesWithAudit();
                if (!masterModel.IsDraft)
                {
                    if (ApprovalProcessID.ToInt() > 0)
                        // await Extension.Post<string>($"/SendMail/SendEMailToRecipients", "Test API Call");
                        await SendMailFromManagerBase(ApprovalProcessID, false, masterModel.POMasterID, (int)Util.MailGroupSetup.POInitiatedMail, (int)Util.ApprovalType.PO);
                }
            }
            await Task.CompletedTask;

            return (true, $"Purchase Order Submitted Successfully");
        }

        private void SetPurchaseOrderMasterNewId(PurchaseOrderMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("PurchaseOrderMaster", AppContexts.User.CompanyID);
            master.POMasterID = code.MaxNumber;
        }

        private List<PurchaseOrderChild> GeneratePurchaseOrderChild(PurchaseOrderDto PO)
        {
            var existingPurchaseOrderChild = PurchaseOrderChildRepo.Entities.Where(x => x.POMasterID == PO.POMasterID).ToList();
            var childModel = new List<PurchaseOrderChild>();
            if (PO.ItemDetails.IsNotNull())
            {
                PO.ItemDetails.ForEach(x =>
                {
                    if (x.ItemID > 0)
                    {
                        childModel.Add(new PurchaseOrderChild
                        {
                            POCID = x.POCID,
                            POMasterID = PO.POMasterID,
                            ItemID = x.ItemID,
                            Description = x.Description,
                            Qty = x.POQty,
                            UOM = x.UOM,
                            Rate = x.Price,
                            Amount = x.Amount,
                            VatInfoID = x.VatInfoID,
                            VatPercent = x.VatPercent,
                            IsRebateable = x.IsRebateable,
                            RebatePercentage = x.RebatePercentage,
                            TotalAmountIncludingVat = x.TotalAmountIncludingVat,
                            PRQID = x.PRCID,
                            PRCID = 0,
                            VatAmount = x.TotalAmountIncludingVat - x.Amount
                        });
                    }

                });

                childModel.ForEach(x =>
                {
                    if (existingPurchaseOrderChild.Count > 0 && x.POCID > 0)
                    {
                        var existingModelData = existingPurchaseOrderChild.FirstOrDefault(y => y.POCID == x.POCID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.POMasterID = PO.POMasterID;
                        x.SetAdded();
                        SetPurchaseOrderChildNewId(x);
                    }
                });

                var willDeleted = existingPurchaseOrderChild.Where(x => !childModel.Select(y => y.POCID).Contains(x.POCID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    childModel.Add(x);
                });
            }

            return childModel;
        }

        private void SetPurchaseOrderChildNewId(PurchaseOrderChild child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("PurchaseOrderChild", AppContexts.User.CompanyID);
            child.POCID = code.MaxNumber;
        }

        private string GeneratePurchaseOrderReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/PO/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{ GenerateSystemCode("PurchaseOrderRefNo", AppContexts.User.CompanyID).SystemCode}";
            return format;
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
                        string filename = $"PurchaseOrder-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "PurchaseOrder\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

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
        private List<Attachment> RemoveAttachments(PurchaseOrderDto PO)
        {
            if (PO.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachment>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='PurchaseOrderMaster' AND ReferenceID={PO.POMasterID}";
                var prevAttachment = PurchaseOrderMasterRepo.GetDataDictCollection(attachmentSql);

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
                var removeList = attachemntList.Where(x => !PO.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "PurchaseOrder";
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

        public async Task<List<PurchaseOrderMasterDto>> GetPurchaseOrderList(string filterData)
        {
            // "All","Pending Action","Action Taken"
            string filter = "";
            switch (filterData)
            {
                case "All":
                    filter = "";
                    break;
                case "My Pending":
                    filter = $@" AND CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@" AND PO.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND PO.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
            string sql = $@"SELECT 
                            DISTINCT
	                            PO.*,
	                            VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
	                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
	                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable
                                ,ISNULL(AEF.IsSCM,0) IsSCM
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                            FROM PurchaseOrderMaster PO
                            LEFT JOIN Security..Users U ON U.UserID = PO.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PO.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PO.POMasterID AND AP.APTypeID = {(int)Util.ApprovalType.PO}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PO} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PO} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PO.POMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = PO.POMasterID
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
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.PO} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = PR.PRMasterID
							WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
                            ORDER BY CreatedDate desc";
            var prList = PurchaseOrderMasterRepo.GetDataModelCollection<PurchaseOrderMasterDto>(sql);
            return prList;
        }

        public async Task<PurchaseOrderMasterDto> GetPurchaseOrderMasterFromApprovedPR(int POMasterID)
        {
            string sql = $@"select PO.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName ,VA.WorkMobile
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,W.WarehouseName DeliveryLocationName
							, SV1.SystemVariableCode AS InventoryTypeName
							, SV2.SystemVariableCode AS PaymentTermsName
							,S.SupplierName
                            ,PR.ReferenceNo AS PRReferenceNo
                            ,PR.PRDate
                            ,VA1.FullName AS PREmployeeName
                            , ISNULL(S.RegisteredAddress, '') AS SupplierAddress
                            ,S.PhoneNumber AS SupplierContact
                            ,(SELECT Security.dbo.NumericToBDT(PO.GrandTotal)) AmountInWords
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            ,ISNULL(AP1.ApprovalProcessID, 0) PRApprovalProcessID
                            ,PR.BudgetPlanRemarks AS PRBudgetPlanRemarks
                        from PurchaseOrderMaster PO
                        LEFT JOIN PurchaseRequisitionMaster PR ON PO.PRMasterID=PR.PRMasterID
                        LEFT JOIN Warehouse W ON PO.DeliveryLocation=W.WarehouseID
                        LEFT JOIN Security..Users U1 ON U1.UserID = PR.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA1 ON VA1.PersonID = U1.PersonID
                        LEFT JOIN Security..Users U ON U.UserID = PO.CreatedBy
						LEFT JOIN security..SystemVariable SV1 ON SV1.SystemVariableID = PO.InventoryTypeID
						LEFT JOIN security..SystemVariable SV2 ON SV2.SystemVariableID = PO.PaymentTermsID
						LEFT JOIN Supplier S ON PO.SupplierID = S.SupplierID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = PO.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PO.POMasterID AND AP.APTypeID = {(int)Util.ApprovalType.PO} 
                        LEFT JOIN Approval..ApprovalProcess AP1 ON AP1.ReferenceID = PO.PRMasterID AND AP1.APTypeID = {(int)Util.ApprovalType.PR} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PO}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PO} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PO.POMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = PO.POMasterID
                        WHERE PO.POMasterID={POMasterID} ";
            var master = PurchaseOrderMasterRepo.GetModelData<PurchaseOrderMasterDto>(sql);
            return master;
        }

        public async Task<PurchaseOrderMasterDto> GetPurchaseOrderMaster(int POMasterID)
        {
            string sql = $@"select PO.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName ,VA.WorkMobile
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,W.WarehouseName DeliveryLocationName
							, SV1.SystemVariableCode AS InventoryTypeName
							, SV2.SystemVariableCode AS PaymentTermsName
							,S.SupplierName
                            ,PR.ReferenceNo AS PRReferenceNo
                            ,PR.PRDate
                            ,VA1.FullName AS PREmployeeName
                            , ISNULL(S.RegisteredAddress, '') AS SupplierAddress
                            ,S.PhoneNumber AS SupplierContact
                            ,(SELECT Security.dbo.NumericToBDT(PO.GrandTotal)) AmountInWords
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            ,ISNULL(AP1.ApprovalProcessID, 0) PRApprovalProcessID
                            ,PR.BudgetPlanRemarks AS PRBudgetPlanRemarks
                        from PurchaseOrderMaster PO
                        LEFT JOIN PurchaseRequisitionMaster PR ON PO.PRMasterID=PR.PRMasterID
                        LEFT JOIN Warehouse W ON PO.DeliveryLocation=W.WarehouseID
                        LEFT JOIN Security..Users U1 ON U1.UserID = PR.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA1 ON VA1.PersonID = U1.PersonID
                        LEFT JOIN Security..Users U ON U.UserID = PO.CreatedBy
						LEFT JOIN security..SystemVariable SV1 ON SV1.SystemVariableID = PO.InventoryTypeID
						LEFT JOIN security..SystemVariable SV2 ON SV2.SystemVariableID = PO.PaymentTermsID
						LEFT JOIN Supplier S ON PO.SupplierID = S.SupplierID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = PO.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PO.POMasterID AND AP.APTypeID = {(int)Util.ApprovalType.PO} 
                        LEFT JOIN Approval..ApprovalProcess AP1 ON AP1.ReferenceID = PO.PRMasterID AND AP1.APTypeID = {(int)Util.ApprovalType.PR} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PO}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PO} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PO.POMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = PO.POMasterID
                        WHERE PO.POMasterID={POMasterID}";
            var master = PurchaseOrderMasterRepo.GetModelData<PurchaseOrderMasterDto>(sql);
            return master;
        }


        public async Task<PurchaseOrderMasterDto> GetPurchaseOrderMasterReassessment(int POMasterID)
        {
            string sql = $@"select PO.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName ,VA.WorkMobile
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,W.WarehouseName DeliveryLocationName
							, SV1.SystemVariableCode AS InventoryTypeName
							, SV2.SystemVariableCode AS PaymentTermsName
							,S.SupplierName
                            ,PR.ReferenceNo AS PRReferenceNo
                            ,PR.PRDate
                            ,VA1.FullName AS PREmployeeName
                            , ISNULL(S.RegisteredAddress, '') AS SupplierAddress
                            ,S.PhoneNumber AS SupplierContact
                            ,(SELECT Security.dbo.NumericToBDT(PO.GrandTotal)) AmountInWords
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            ,ISNULL(AP1.ApprovalProcessID, 0) PRApprovalProcessID
                            ,PR.BudgetPlanRemarks AS PRBudgetPlanRemarks
                        from PurchaseOrderMaster PO
                        LEFT JOIN PurchaseRequisitionMaster PR ON PO.PRMasterID=PR.PRMasterID
                        LEFT JOIN Warehouse W ON PO.DeliveryLocation=W.WarehouseID
                        LEFT JOIN Security..Users U1 ON U1.UserID = PR.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA1 ON VA1.PersonID = U1.PersonID
                        LEFT JOIN Security..Users U ON U.UserID = PO.CreatedBy
						LEFT JOIN security..SystemVariable SV1 ON SV1.SystemVariableID = PO.InventoryTypeID
						LEFT JOIN security..SystemVariable SV2 ON SV2.SystemVariableID = PO.PaymentTermsID
						LEFT JOIN Supplier S ON PO.SupplierID = S.SupplierID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = PO.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PO.POMasterID AND AP.APTypeID = {(int)Util.ApprovalType.PO} 
                        LEFT JOIN Approval..ApprovalProcess AP1 ON AP1.ReferenceID = PO.PRMasterID AND AP1.APTypeID = {(int)Util.ApprovalType.PR} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PO}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PO} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PO.POMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = PO.POMasterID
                        WHERE PO.POMasterID={POMasterID} AND PO.CreatedBy={AppContexts.User.UserID}";
            var master = PurchaseOrderMasterRepo.GetModelData<PurchaseOrderMasterDto>(sql);
            return master;
        }
        public async Task<List<PurchaseOrderChildDto>> GetPurchaseOrderChild(int POMasterID)
        {
            string sql = $@"select POC.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode , VA.DivisionID, VA.DivisionName,I.ItemName, I.ItemCode,Unit.UnitCode
                        ,'Vat Percent: '+VI.VatPolicies+' , Rebateable: '+ CASE WHEN VI.IsRebateable = 0 THEN 'No' ELSE 'YES' END +
	                        ', Rebateable: '+Cast(VI.RebatePercentage as nvarchar(100)) VatInfo
                        ,POC.Qty AS POQty
                        ,POC.Rate AS Price
						,CASE WHEN POC.IsRebateable = 0 THEN 'No' ELSE 'YES' END Rebateable
                        ,I.InventoryTypeID
                        ,POM.SupplierID
                        ,POM.DeliveryWithinDate
						,PRM.QuotedQty AS PRQty
                        ,PRM.QuotedUnitPrice AS PRPrice
						,POC.PRQID PRCID
						,POC.PRQID
						,ISNULL(PO.Amount,0) AS PurchasedAmount
						,ISNULL(PO.Qty,0) AS PurchasedQty
						,ISNULL(PRM.Amount, 0) AS PRAmount
	                    ,ISNULL(POC.Qty, 0) - ISNULL(MR.ReceivedQty, 0) AS BalanceQty
	                    ,ISNULL(QC.QCSuppliedQty, 0) AS QCSuppliedQty
	                    ,ISNULL(QC.QCAcceptedQty, 0) AS QCAcceptedQty
                        from PurchaseOrderChild POC
                        LEFT JOIN PurchaseOrderMaster POM ON POM.POMasterID = POC.POMasterID
                        LEFT JOIN Item I ON I.ItemID=POC.ItemID
                        LEFT JOIN PurchaseRequisitionQuotation PRM ON PRM.PRQID = POC.PRQID
						LEFT JOIN (	SELECT POC1.ItemID,SUM(POC1.Qty) Qty,SUM(POC1.TotalAmountIncludingVat) Amount,PRMasterID FROM PurchaseOrderChild POC1
									JOIN PurchaseOrderMaster POM1 ON POC1.POMasterID=POM1.POMasterID 
									WHERE POM1.ApprovalStatusID	<> 24 AND POM1.POMasterID!= {POMasterID}
									GROUP BY POC1.ItemID,PRMasterID) PO ON PRM.ItemID=PO.ItemID  AND PO.PRMasterID=POM.PRMasterID
                        LEFT JOIN Security..Unit  ON Unit.UnitID=POC.UOM
						
                        LEFT JOIN (
	                        SELECT ItemID
		                        ,SUM(MRC.ReceiveQty) ReceivedQty
		                        ,MR.POMasterID
	                        FROM MaterialReceiveChild MRC
	                        LEFT JOIN MaterialReceive MR ON MRC.MRID = MR.MRID
	                        GROUP BY POMasterID
		                        ,ItemID
	                        ) MR ON POC.ItemID = MR.ItemID
	                        AND MR.POMasterID = POM.POMasterID
                        LEFT JOIN Security..Users U ON U.UserID = POC.CreatedBy
						LEFT JOIN VatInfo VI ON POC.VatInfoID = VI.VatInfoID
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = POM.ApprovalStatusID
                        LEFT JOIN (
	                        SELECT ItemID
		                        ,SUM(QCC.SuppliedQty) QCSuppliedQty, SUM(QCC.AcceptedQty) QCAcceptedQty 
		                        ,QCM.POMasterID, QCC.POCID
	                        FROM QCChild QCC
	                        LEFT JOIN QCMaster QCM ON QCC.QCMID = QCM.QCMID AND ApprovalStatusID <> 24
	                        GROUP BY POMasterID,POCID
		                        ,ItemID
	                        ) QC ON POC.ItemID = QC.ItemID
	                        AND QC.POMasterID = POC.POMasterID AND QC.POCID = POC.POCID
                        WHERE POC.POMasterID= {POMasterID}";
            var childs = PurchaseOrderChildRepo.GetDataModelCollection<PurchaseOrderChildDto>(sql);
            return childs;
        }

        public async Task<List<PurchaseOrderChildDto>> GetPurchaseOrderChildSCC(int POMasterID)
        {
            string sql = $@"



select POC.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode , VA.DivisionID, VA.DivisionName,I.ItemName, I.ItemCode,Unit.UnitCode
                        
                        ,'Vat Percent: '+VI.VatPolicies+' , Rebateable: '+ CASE WHEN VI.IsRebateable = 0 THEN 'No' ELSE 'YES' END +
	                        ', Rebateable: '+Cast(VI.RebatePercentage as nvarchar(100)) VatInfo
                        ,POC.Qty AS POQty
                        ,POC.Rate AS Price
						,CASE WHEN POC.IsRebateable = 0 THEN 'No' ELSE 'YES' END Rebateable
                        ,I.InventoryTypeID
                        ,POM.SupplierID
                        ,POM.DeliveryWithinDate
						,POC.PRQID PRCID
						,POC.PRQID
                        ,ISNULL(SCC.SCCReceivedQty, 0) AS ReceivedQty
	                    ,ISNULL(POC.Qty, 0) - ISNULL(SCC.SCCReceivedQty, 0) AS BalanceQty
                        from PurchaseOrderChild POC
                        LEFT JOIN PurchaseOrderMaster POM ON POM.POMasterID = POC.POMasterID
                        LEFT JOIN Item I ON I.ItemID=POC.ItemID
						LEFT JOIN (
	                        SELECT ItemID
		                        ,SUM(QCC.ReceivedQty) SCCReceivedQty 
		                        ,QCM.POMasterID, QCC.POCID
	                        FROM SCCChild QCC
	                        LEFT JOIN SCCMaster QCM ON QCC.SCCMID = QCM.SCCMID AND ApprovalStatusID <> 24
	                        GROUP BY POMasterID,POCID
		                        ,ItemID
	                        ) SCC ON POC.ItemID = SCC.ItemID
	                        AND SCC.POMasterID = POC.POMasterID AND SCC.POCID = POC.POCID

                        LEFT JOIN Security..Unit  ON Unit.UnitID=POC.UOM
						
                        LEFT JOIN Security..Users U ON U.UserID = POC.CreatedBy
						LEFT JOIN VatInfo VI ON POC.VatInfoID = VI.VatInfoID
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = POM.ApprovalStatusID
                        
                        WHERE POC.POMasterID= {POMasterID}";
            var childs = PurchaseOrderChildRepo.GetDataModelCollection<PurchaseOrderChildDto>(sql);
            return childs;
        }


        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.PO);
            return comments.Result;
        }
        public List<Attachments> GetAttachments(int POMasterID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='PurchaseOrderMaster' AND ReferenceID={POMasterID}";
            var attachment = PurchaseOrderMasterRepo.GetDataDictCollection(attachmentSql);
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

        public async Task<IEnumerable<Dictionary<string, object>>> GetsSupplierFromPOQuotation(int POID)
        {
            string sql = $@"select prq.SupplierID value, ISNULL(s.SupplierName, '') label, ISNULL(s.CorrespondingAddress, '') CorrespondingAddress, ISNULL(s.RegisteredAddress, '') RegisteredAddress
                            ,'' AS SelectedSupplier, s.PhoneNumber
                            from PurchaseOrderQuotation prq
                            left join Supplier s on prq.SupplierID = s.SupplierID
                            where prq.POMasterID={POID} 
                            ORDER BY prq.POQID asc";

            var listDict = ItemRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }
        public GridModel GetAllApproved(GridParameter parameters)
        {
            string fromDateToDateCondition = string.Empty;
            if (parameters.Parameters.Count == 2)
            {
                string fdate = parameters.Parameters.Single(x => x.name == "FromDate").value;
                string tdate = parameters.Parameters.Single(x => x.name == "ToDate").value;
                fromDateToDateCondition = @$" AND CAST(PO.PODate AS Date) between '{fdate}' AND '{tdate}'";
            }

            string sql = $@"SELECT 
	                             PO.POMasterID
								,PO.ReferenceNo
								,PO.CreatedDate
								,PO.ApprovalStatusID
								,PO.PODate
								,PO.CreditDay
	                            ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
                                ,PR.ReferenceNo AS PRNo
                                ,ISNULL(PR.PRMasterID,'') AS PRMasterID
								,S.SupplierName
                                ,ISNULL(CountMR, 0) CountMR
                                ,ISNULL(CountQC, 0) CountQC
                                ,(
		                            SELECT ApprovalProcessID, QCMID,ReferenceNo QCRefNo, ReceiptDate
									FROM QCMaster QCM
									LEFT JOIN Approval..ApprovalProcess InAP ON InAP.ReferenceID = QCM.QCMID AND InAP.APTypeID =  {(int)Util.ApprovalType.QC}
                                    LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = QCM.ApprovalStatusID
									WHERE POMasterID = PO.POMasterID
									FOR JSON PATH
		                        ) AS QCDetails
                            ,CASE WHEN ISNULL(PO.IsClosed, 0) = 1 THEN 'Yes' ELSE 'No' END AS CloseStatus
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            ,ISNULL(APPR.ApprovalProcessID, 0) PRApprovalProcessID
	                            
                            FROM PurchaseOrderMaster PO
                            LEFT JOIN (
		                                    SELECT 
			                                    COUNT(MRID) CountMR,POMasterID
		                                    FROM 
			                                    MaterialReceive
                                            
                                            WHERE ApprovalStatusID <> {(int)Util.ApprovalStatus.Rejected}
		                                    GROUP BY POMasterID
	                                    ) MR ON MR.POMasterID = PO.POMasterID
                            LEFT JOIN (
		                                    SELECT 
			                                    COUNT(QCMID) CountQC,POMasterID
		                                    FROM 
			                                    QCMaster
                                            
                                            WHERE ApprovalStatusID <> {(int)Util.ApprovalStatus.Rejected}
		                                    GROUP BY POMasterID
	                                    ) QC ON QC.POMasterID = PO.POMasterID
                            LEFT JOIN(
                                select count(InventoryTypeID) ServiceTypeCount, POC.POMasterID
                                from PurchaseOrderChild poc 
                                left join item i on i.ItemID = poc.ItemID
                                left join Security..SystemVariable sv on sv.SystemVariableID = i.InventoryTypeID
                                where i.InventoryTypeID<>{(int)Util.InventoryType.Services}
                                group by i.InventoryTypeID, POC.POMasterID
                            ) POC ON POC.POMasterID = PO.POMasterID
                            LEFT JOIN PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID
							LEFT JOIN Supplier S ON S.SupplierID = PO.SupplierID
                            LEFT JOIN Security..Users U ON U.UserID = PO.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PO.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PO.POMasterID AND AP.APTypeID = {(int)Util.ApprovalType.PO}
                            LEFT JOIN Approval..ApprovalProcess APPR ON APPR.ReferenceID = PO.PRMasterID AND APPR.APTypeID = {(int)Util.ApprovalType.PR}
                            
							WHERE ISNULL(POC.ServiceTypeCount, 0) > 0 AND PO.ApprovalStatusID={(int)Util.ApprovalStatus.Approved} {fromDateToDateCondition}";

            var result = PurchaseOrderMasterRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }

        public GridModel GetAllApprovedForSCC(GridParameter parameters)
        {
            string fromDateToDateCondition = string.Empty;
            if (parameters.Parameters.Count == 2)
            {
                string fdate = parameters.Parameters.Single(x => x.name == "FromDate").value;
                string tdate = parameters.Parameters.Single(x => x.name == "ToDate").value;
                fromDateToDateCondition = @$" AND CAST(PO.PODate AS Date) between '{fdate}' AND '{tdate}'";
            }

            string sql = $@"SELECT 
	                             PO.POMasterID
								,PO.ReferenceNo
								,PO.CreatedDate
								,PO.ApprovalStatusID
								,PO.PODate
	                            ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
                                ,ISNULL(PR.ReferenceNo,'') AS PRNo
                                ,ISNULL(PR.PRMasterID,'') AS PRMasterID
								,ISNULL(S.SupplierName,'') SupplierName
                                ,ISNULL(CountSCC, 0) CountSCC
                                ,(
		                             SELECT ApprovalProcessID, SCCMID,ReferenceNo SCCRefNo, SCCM.CreatedDate
									FROM SCCMaster SCCM
									LEFT JOIN Approval..ApprovalProcess InAP ON InAP.ReferenceID = SCCM.SCCMID AND InAP.APTypeID =  {(int)Util.ApprovalType.SCC}
                                    LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = SCCM.ApprovalStatusID
									WHERE POMasterID = PO.POMasterID
									FOR JSON PATH
		                        ) AS SCCDetails
                            ,CASE WHEN ISNULL(PO.IsClosed, 0) = 1 THEN 'Yes' ELSE 'No' END AS CloseStatus
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            ,ISNULL(AP1.ApprovalProcessID, 0) PRApprovalProcessID
	                            
                            FROM PurchaseOrderMaster PO
                           
                            LEFT JOIN (
		                                    SELECT 
			                                    COUNT(SCCMID) CountSCC,POMasterID
		                                    FROM 
			                                    SCCMaster
                                            
                                            WHERE ApprovalStatusID <> {(int)Util.ApprovalStatus.Rejected}
		                                    GROUP BY POMasterID
	                                    ) QC ON QC.POMasterID = PO.POMasterID
                            LEFT JOIN(
                                select count(InventoryTypeID) ServiceTypeCount, POC.POMasterID
                                from PurchaseOrderChild poc 
                                left join item i on i.ItemID = poc.ItemID
                                left join Security..SystemVariable sv on sv.SystemVariableID = i.InventoryTypeID
                                where i.InventoryTypeID={(int)Util.InventoryType.Services}
                                group by i.InventoryTypeID, POC.POMasterID
                            ) POC ON POC.POMasterID = PO.POMasterID
                            LEFT JOIN PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID
							LEFT JOIN Supplier S ON S.SupplierID = PO.SupplierID
                            LEFT JOIN Security..Users U ON U.UserID = PO.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PO.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PO.POMasterID AND AP.APTypeID = {(int)Util.ApprovalType.PO}
                            LEFT JOIN Approval..ApprovalProcess AP1 ON AP1.ReferenceID = PO.PRMasterID AND AP1.APTypeID = {(int)Util.ApprovalType.PR}
                            
							WHERE ISNULL(POC.ServiceTypeCount, 0) > 0 AND PO.ApprovalStatusID={(int)Util.ApprovalStatus.Approved} {fromDateToDateCondition}";

            var result = PurchaseOrderMasterRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }


        public async Task ClosePurchaseOrder(PurchaseOrderDto PO)
        {
            var createdQC = QCMasterRepo.Entities.Where(x => x.POMasterID == PO.POMasterID).SingleOrDefault();
            var existingPO = PurchaseOrderMasterRepo.Entities.Where(x => x.POMasterID == PO.POMasterID).SingleOrDefault();
            if (createdQC.IsNull())
            {
                int isClosed = PO.IsClosed ? 1 : 0;
                string sql = $@"UPDATE SCM..PurchaseOrderMaster SET IsClosed={isClosed}, CloseRemarks='{PO.CloseRemarks}' WHERE POMasterID={PO.POMasterID}";
                PurchaseOrderMasterRepo.ExecuteSqlCommand(sql);
                await Task.CompletedTask;
            }
        }
        public async Task UpdatePurchaseOrderMasterAfterReset(PurchaseOrderDto PO)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existingPO = PurchaseOrderMasterRepo.Entities.Where(x => x.POMasterID == PO.POMasterID).SingleOrDefault();
                if (existingPO.IsNotNull())
                {
                    existingPO.CreatedBy = existingPO.CreatedBy;
                    existingPO.CreatedDate = existingPO.CreatedDate;
                    existingPO.CreatedIP = existingPO.CreatedIP;
                    existingPO.RowVersion = existingPO.RowVersion;
                    existingPO.POMasterID = PO.POMasterID;
                    existingPO.ReferenceNo = existingPO.ReferenceNo;
                    existingPO.ApprovalStatusID = 22;
                    existingPO.SetModified();
                }

                
                var existingPR = PurchaseRequisitionMasterRepo.Entities.Where(x => x.PRMasterID == existingPO.PRMasterID).SingleOrDefault();
                if (existingPR.IsNotNull())
                {
                    existingPR.CreatedBy = existingPR.CreatedBy;
                    existingPR.CreatedDate = existingPR.CreatedDate;
                    existingPR.CreatedIP = existingPR.CreatedIP;
                    existingPR.RowVersion = existingPR.RowVersion;
                    existingPR.PRMasterID = existingPO.PRMasterID;
                    existingPR.ReferenceNo = existingPR.ReferenceNo;
                    existingPR.ApprovalStatusID = existingPR.ApprovalStatusID;
                    existingPR.IsArchive = false;
                    existingPR.SetModified();
                }


                SetAuditFields(existingPO);
                SetAuditFields(existingPR);
                PurchaseOrderMasterRepo.Add(existingPO);
                PurchaseRequisitionMasterRepo.Add(existingPR);
                unitOfWork.CommitChangesWithAudit();
            }
            await Task.CompletedTask;
        }

        public async Task RemovePurchaseOrder(int POMasterID, int aprovalProcessId)
        {
            var poMaster = PurchaseOrderMasterRepo.Entities.Where(x => x.POMasterID == POMasterID).FirstOrDefault();
            poMaster.SetDeleted();
            var mailData = new List<Dictionary<string, object>>();
            var data = new Dictionary<string, object>
                {
                    { "ReferenceNo", poMaster.ReferenceNo },
                    { "EmployeeName", AppContexts.User.FullName }
                };
            mailData.Add(data);
            var mail = GetAPEmployeeEmailsWithProxy(aprovalProcessId).Result;
            List<string> ToEmailAddress = new List<string>() { mail.Item1 };
            List<string> CCEmailAddress = new List<string>() { mail.Item2 };

            //BasicMail((int)Util.MailGroupSetup.NFARemoveMail, ToEmailAddress, false, CCEmailAddress, null, mailData);

            using (var unitOfWork = new UnitOfWork())
            {

                var poChild = PurchaseOrderChildRepo.Entities.Where(x => x.POMasterID == POMasterID).ToList();
                poChild.ForEach(x => x.SetDeleted());

                PurchaseOrderMasterRepo.Add(poMaster);
                PurchaseOrderChildRepo.AddRange(poChild);
                DeleteAllApprovalProcessRelatedData((int)ApprovalType.PO, POMasterID);
                unitOfWork.CommitChangesWithAudit();

            }

            await Task.CompletedTask;
        }

        public IEnumerable<Dictionary<string, object>> ReportForPOApprovalFeedback(int POID)
        {
            string sql = $@" EXEC SCM..spRPTPOApprovalFeedback {POID}";
            var feedback = PurchaseOrderMasterRepo.GetDataDictCollection(sql);
            return feedback;
        }
        public Dictionary<string, object> ReportForPOApprovalFeedbackWithTerm(int POID)
        {
            string sql = $@"SELECT * FROM SCM..viewPOApprovalFeedbackWithTerms WHERE POMasterID = {POID}";
            var feedback = PurchaseOrderMasterRepo.GetData(sql);
            return feedback;
        }

        public IEnumerable<Dictionary<string, object>> GetForwardingMemberComments(int APTypeID, int ReferenceID)
        {
            string sql = $@"SELECT 
	                             FullName,
	                             APForwardEmployeeComment,
	                             CAST(CommentSubmitDate as Date) CommentSubmitDate,
	                             DesignationName,
	                             DepartmentName
                            FROM 
	                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo AEF 
	                            INNER JOIN Approval..ApprovalProcess  AP ON AEF.ApprovalProcessID = AP.ApprovalProcessID
	                            INNER JOIN HRMS..ViewALLEmployee VE ON VE.EmployeeID = AEF.EmployeeID	
                            WHERE AP.APTypeID = {APTypeID} AND ReferenceID = {ReferenceID} AND CommentSubmitDate IS NOT NULL
                            ORDER BY APForwardInfoID asc";
            var comments = PurchaseOrderMasterRepo.GetDataDictCollection(sql);
            return comments;
        }

        private List<PurchaseRequisitionChildDto> GetPurchaseRequisitionForBalance(int PRMasterID, int POMasterID)
        {
            string sql = $@"SELECT  PRC.PRCID
                        	,PRC.ItemID
                        	,PRC.Description
                        	,PRC.Qty
                        	,PRC.Price
                        	,PRC.Amount AS PRAmount
                        	,PRC.PRMasterID                        	
                        	,ISNULL(PO.Amount, 0) AS PurchasedAmount
                        	,ISNULL(PO.Qty, 0) AS PurchasedQty
                            ,PRQ.PRQID
                        FROM PurchaseRequisitionChild PRC
                        INNER JOIN PurchaseRequisitionMaster PRM ON PRM.PRMasterID = PRC.PRMasterID
                        INNER JOIN PurchaseRequisitionQuotation PRQ ON PRQ.PRCID = PRC.PRCID
						LEFT JOIN (	SELECT POC.ItemID,SUM(POC.Qty) Qty,SUM(POC.TotalAmountIncludingVat) Amount, PRMasterID, PRQID FROM PurchaseOrderChild POC
									JOIN PurchaseOrderMaster POM ON POC.POMasterID=POM.POMasterID 
									WHERE POM.ApprovalStatusID	<> 24 AND POM.POMasterID != {POMasterID}
									GROUP BY POC.ItemID,PRMasterID,PRQID) PO ON PRC.ItemID=PO.ItemID AND PO.PRMasterID = PRM.PRMasterID AND PRQ.PRQID = PO.PRQID
                        
                        WHERE PRC.PRMasterID={PRMasterID}";
            var childs = PurchaseOrderChildRepo.GetDataModelCollection<PurchaseRequisitionChildDto>(sql);
            return childs;
        }


        public async Task<Dictionary<string, object>> GetSupplierByID(int POMasterID)
        {
            string sql = $@"SELECT * FROM Supplier WHERE SupplierID IN(SELECT SupplierID FROM PurchaseOrderMaster WHERE POMasterID={POMasterID})";
            var supplier = PurchaseOrderMasterRepo.GetData(sql);

            return await Task.FromResult(supplier);
        }

        public async Task<Dictionary<string, object>> GetCompanyInfo()
        {
            string sql = $@"SELECT * FROM Security..Company WHERE CompanyID='{AppContexts.User.CompanyID}'";

            var data = PurchaseOrderMasterRepo.GetData(sql);

            return await Task.FromResult(data);
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetTerms()
        {
            string sql = $@"SELECT * FROM Security..CompanyTerms WHERE TermsType='PO'";

            var listDict = PurchaseOrderMasterRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }


        public async Task<List<PurchaseOrderChildDto>> GetPurchaseOrderChildForQC(int POMasterID)
        {
            string sql = $@"SELECT POC.*
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
                        	,ISNULL(MR.QCSuppliedQty, 0) AS QCSuppliedQty
                        	,ISNULL(MR.QCAcceptedQty, 0) AS QCAcceptedQty
                        	,ISNULL(POC.Qty, 0) - ISNULL(MR.QCSuppliedQty, 0) AS BalanceQty
                        FROM PurchaseOrderChild POC
                        LEFT JOIN PurchaseOrderMaster POM ON POM.POMasterID = POC.POMasterID
                        LEFT JOIN Item I ON I.ItemID = POC.ItemID
                        LEFT JOIN (
                        	SELECT ItemID
                        		,SUM(MRC.SuppliedQty) QCSuppliedQty
                        		,SUM(MRC.AcceptedQty) QCAcceptedQty
                        		,MR.POMasterID
                        		,POCID
                        	FROM QCChild MRC
                        	LEFT JOIN QCMaster MR ON MRC.QCMID = MR.QCMID
                        		AND MR.ApprovalStatusID <> 24
                        	GROUP BY POMasterID
                        		,ItemID
                        		,POCID
                        	) MR ON POC.ItemID = MR.ItemID
                        	AND POC.POCID = MR.POCID
                        	AND MR.POMasterID = POM.POMasterID
                        LEFT JOIN Security..Unit ON Unit.UnitID = POC.UOM
                        LEFT JOIN Security..Users U ON U.UserID = POC.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = POM.ApprovalStatusID
                        WHERE POC.POMasterID = {POMasterID} AND (POC.Qty -ISNULL(MR.QCAcceptedQty, 0) > 0 )";
            var childs = PurchaseOrderChildRepo.GetDataModelCollection<PurchaseOrderChildDto>(sql);
            return childs;
        }

        public async Task<List<PurchaseOrderChildDto>> GetPurchaseOrderChildForSCC(int POMasterID)
        {
            string sql = $@"
                        
                        SELECT POC.*
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
							,SCCC.SCCReceivedQty ReceivedQty
	                        ,ISNULL(POC.Qty, 0) - ISNULL(SCCC.SCCReceivedQty, 0) AS BalanceQty
                            ,ISNULL(SCCC.InvoiceAmount, 0) InvoiceAmount
                        FROM PurchaseOrderChild POC
                        LEFT JOIN PurchaseOrderMaster POM ON POM.POMasterID = POC.POMasterID
                        LEFT JOIN Item I ON I.ItemID = POC.ItemID
						LEFT JOIN (
	                        SELECT ItemID
		                        ,SUM(MRC.ReceivedQty) SCCReceivedQty
		                        ,MR.POMasterID
                                ,SUM(MRC.InvoiceAmount) InvoiceAmount
	                        FROM SCCChild MRC
	                        LEFT JOIN SCCMaster MR ON MRC.SCCMID = MR.SCCMID AND MR.ApprovalStatusID <> 24
	                        GROUP BY POMasterID
		                        ,ItemID
	                        ) SCCC ON POC.ItemID = SCCC.ItemID
	                        AND SCCC.POMasterID = POM.POMasterID
                        LEFT JOIN Security..Unit ON Unit.UnitID = POC.UOM
                        LEFT JOIN Security..Users U ON U.UserID = POC.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = POM.ApprovalStatusID
                        WHERE POC.POMasterID = {POMasterID} AND (POC.Qty -ISNULL(SCCC.SCCReceivedQty, 0) > 0 )";
            var childs = PurchaseOrderChildRepo.GetDataModelCollection<PurchaseOrderChildDto>(sql);
            return childs;
        }


        public async Task<GridModel> GetPOListForGrid(GridParameter parameters)
        {
            string filter = "";
            string where = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "My Pending":
                    filter = $@" CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@" PO.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" PO.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
                    break;
                case "My Approved":
                    filter = $@" AP.ApprovalProcessID IN (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Approved}))";
                    break;
                case "MyRejectReturnForwarded":
                    filter = $@" AP.ApprovalProcessID IN 
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
            string sql = $@"SELECT 
                            DISTINCT
	                             PO.POMasterID
								,PO.ReferenceNo
								,PO.ApprovalStatusID
								,PO.CreatedDate
								,PO.PODate
								,PO.IsClosed
								,PO.CreditDay
                                ,PR.Subject
	                            ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
                                ,S.SupplierName
	                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
	                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable
                                ,ISNULL(AEF.IsSCM,0) IsSCM
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                ,ISNULL(CountQC, 0) CountQC
                                ,(
		                            SELECT ApprovalProcessID, QCMID,ReferenceNo QCRefNo, ReceiptDate
									FROM QCMaster QCM
									LEFT JOIN Approval..ApprovalProcess InAP ON InAP.ReferenceID = QCM.QCMID AND InAP.APTypeID =  {(int)Util.ApprovalType.QC}
                                    LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = QCM.ApprovalStatusID
									WHERE POMasterID = PO.POMasterID
									FOR JSON PATH
		                        ) AS QCDetails
                            ,CASE WHEN ISNULL(PO.IsClosed, 0) = 1 THEN 'Yes' ELSE 'No' END AS CloseStatus
                            ,ISNULL(VA.FullName+VA.EmployeeCode+VA.DepartmentName,'') EmployeeWithDepartment
                            --,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                            ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                            FROM PurchaseOrderMaster PO
                            LEFT JOIN (
		                                    SELECT 
			                                    COUNT(QCMID) CountQC,POMasterID
		                                    FROM 
			                                    QCMaster
                                            
                                            WHERE ApprovalStatusID <> {(int)Util.ApprovalStatus.Rejected}
		                                    GROUP BY POMasterID
	                                    ) QC ON QC.POMasterID = PO.POMasterID

                            LEFT JOIN Security..Users U ON U.UserID = PO.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
	                        LEFT JOIN SCM..Supplier S ON S.SupplierID = PO.SupplierID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PO.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PO.POMasterID AND AP.APTypeID = {(int)Util.ApprovalType.PO}
                            LEFT JOIN SCM..PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID
                            LEFT JOIN (
                                        SELECT 
                                              APEmployeeFeedbackID,ApprovalProcessID,IsEditable ,IsSCM,IsMultiProxy  
                                        FROM 
                                            Approval.dbo.functionJoinListAEF({AppContexts.User.EmployeeID})
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
									SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF({AppContexts.User.EmployeeID}) 
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PO} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PO} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PO.POMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = PO.POMasterID
                                    LEFT JOIN (
                                            SELECT * FROM Approval.dbo.functionJoinListProxyEmployeeAPSubmitDate({AppContexts.User.EmployeeID})                                 
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
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.PO} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = PO.POMasterID
							{where} {filter}
                            ";
            //WHERE(VA.EmployeeID = { AppContexts.User.EmployeeID} OR F.EmployeeID = { AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = { AppContexts.User.EmployeeID}) { filter}
            var result = PurchaseOrderMasterRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }

        public List<ManualApprovalPanelEmployeeDto> GetQCApprovalPanelDefault(int POMasterID)
        {
            string sql = $@"SELECT Emp.FullName AS EmployeeName, Emp.EmployeeCode,Emp.EmployeeID 
                        FROM PurchaseOrderMaster PO 
                        LEFT JOIN PurchaseRequisitionMaster PR ON PO.PRMasterID = PR.PRMasterID
                        LEFT JOIN Security..Users U ON PR.CreatedBy = U.UserID
                        LEFT JOIN HRMS..ViewALLEmployee Emp ON U.PersonID=Emp.PersonID
                        WHERE PO.POMasterID={POMasterID}";

            var pr = PurchaseOrderMasterRepo.GetModelData<PurchaseOrderMasterDto>(sql);

            List<ManualApprovalPanelEmployeeDto> apList = new List<ManualApprovalPanelEmployeeDto>();
            ManualApprovalPanelEmployeeDto seq1 = new ManualApprovalPanelEmployeeDto { APPanelID = (int)Util.ApprovalPanel.QCBelowTheLimit, EmployeeID = AppContexts.User.EmployeeID.Value, IsProxyEmployeeEnabled = false, ProxyEmployeeID = 0, APTypeID = (int)Util.ApprovalType.GRN, SequenceNo = 1, EmployeeCode = AppContexts.User.EmployeeCode, EmployeeName = AppContexts.User.EmployeeCode + "-" + AppContexts.User.FullName, NFAApprovalSequenceTypeName = "Proposed", NFAApprovalSequenceType = 62, IsSystemGenerated = true };
            apList.Add(seq1);
            if (pr.EmployeeID != AppContexts.User.EmployeeID)
            {
                ManualApprovalPanelEmployeeDto seq2 = new ManualApprovalPanelEmployeeDto { APPanelID = (int)Util.ApprovalPanel.QCBelowTheLimit, EmployeeID = pr.EmployeeID, IsProxyEmployeeEnabled = false, ProxyEmployeeID = 0, APTypeID = (int)Util.ApprovalType.GRN, SequenceNo = 2, EmployeeName = pr.EmployeeCode + "-" + pr.EmployeeName, EmployeeCode = pr.EmployeeCode, NFAApprovalSequenceTypeName = "Prepared", NFAApprovalSequenceType = 55, IsSystemGenerated = true };
                apList.Add(seq2);
            }
            return apList;
        }
        public List<ManualApprovalPanelEmployeeDto> GetSCCApprovalPanelDefault(int POMasterID)
        {
            string sql = $@"SELECT Emp.FullName AS EmployeeName, Emp.EmployeeCode,Emp.EmployeeID 
                        FROM PurchaseOrderMaster PO 
                        LEFT JOIN PurchaseRequisitionMaster PR ON PO.PRMasterID = PR.PRMasterID
                        LEFT JOIN Security..Users U ON PR.CreatedBy = U.UserID
                        LEFT JOIN HRMS..ViewALLEmployee Emp ON U.PersonID=Emp.PersonID
                        WHERE PO.POMasterID={POMasterID}";

            var pr = PurchaseOrderMasterRepo.GetModelData<PurchaseOrderMasterDto>(sql);

            List<ManualApprovalPanelEmployeeDto> apList = new List<ManualApprovalPanelEmployeeDto>();

            //if (pr.EmployeeID != AppContexts.User.EmployeeID)
            //{
            ManualApprovalPanelEmployeeDto seq1 = new ManualApprovalPanelEmployeeDto { APPanelID = (int)Util.ApprovalPanel.SCC, IsEditable = true, EmployeeID = pr.EmployeeID, IsProxyEmployeeEnabled = false, ProxyEmployeeID = 0, APTypeID = (int)Util.ApprovalType.SCC, SequenceNo = 1, EmployeeName = pr.EmployeeCode + "-" + pr.EmployeeName, EmployeeCode = pr.EmployeeCode, NFAApprovalSequenceTypeName = "Proposed", NFAApprovalSequenceType = 62, IsSystemGenerated = true };
            apList.Add(seq1);
            //}
            return apList;
        }




        public async Task<GridModel> GetPOListForSCCGrid(GridParameter parameters)
        {
            string filter = "";
            string where = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "My Pending":
                    filter = $@" CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@" PO.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" PO.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
                    break;
                case "My Approved":
                    filter = $@" AP.ApprovalProcessID IN (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Approved}))";
                    break;
                case "MyRejectReturnForwarded":
                    filter = $@" AP.ApprovalProcessID IN 
                                (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Rejected})
                                UNION  
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Returned})
                                UNION 
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Forwarded}))";
                    break;
                default:
                    break;
            }
            where = filter.IsNotNullOrEmpty() ? "AND " : "";
            string sql = $@"SELECT 
                            DISTINCT
	                             PO.POMasterID
								,PO.ReferenceNo
								,PO.ApprovalStatusID
								,PO.CreatedDate
								,PO.PODate
								,PO.IsClosed
								,PO.CreditDay
	                            ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
                                ,S.SupplierName
	                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
	                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable
                                ,ISNULL(AEF.IsSCM,0) IsSCM
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                ,ISNULL(CountQC, 0) CountQC
                                ,(
		                            SELECT ApprovalProcessID, QCMID,ReferenceNo QCRefNo, ReceiptDate
									FROM QCMaster QCM
									LEFT JOIN Approval..ApprovalProcess InAP ON InAP.ReferenceID = QCM.QCMID AND InAP.APTypeID =  {(int)Util.ApprovalType.QC}
                                    LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = QCM.ApprovalStatusID
									WHERE POMasterID = PO.POMasterID
									FOR JSON PATH
		                        ) AS QCDetails
                            ,CASE WHEN ISNULL(PO.IsClosed, 0) = 1 THEN 'Yes' ELSE 'No' END AS CloseStatus
                            ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                            ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                            FROM PurchaseOrderMaster PO
                            LEFT JOIN (
		                                    SELECT 
			                                    COUNT(QCMID) CountQC,POMasterID
		                                    FROM 
			                                    QCMaster
                                            
                                            WHERE ApprovalStatusID <> {(int)Util.ApprovalStatus.Rejected}
		                                    GROUP BY POMasterID
	                                    ) QC ON QC.POMasterID = PO.POMasterID

                            LEFT JOIN Security..Users U ON U.UserID = PO.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
	                        LEFT JOIN SCM..Supplier S ON S.SupplierID = PO.SupplierID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PO.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PO.POMasterID AND AP.APTypeID = {(int)Util.ApprovalType.PO}

                            LEFT JOIN(
                                select count(InventoryTypeID) ServiceTypeCount, POC.POMasterID
                                from PurchaseOrderChild poc 
                                left join item i on i.ItemID = poc.ItemID
                                left join Security..SystemVariable sv on sv.SystemVariableID = i.InventoryTypeID
                                where i.InventoryTypeID={(int)Util.InventoryType.Services}
                                group by i.InventoryTypeID, POC.POMasterID
                            ) POC ON POC.POMasterID = PO.POMasterID


                            LEFT JOIN (
                                        SELECT 
                                              APEmployeeFeedbackID,ApprovalProcessID,IsEditable ,IsSCM,IsMultiProxy  
                                        FROM 
                                            Approval.dbo.functionJoinListAEF({AppContexts.User.EmployeeID})
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
									SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF({AppContexts.User.EmployeeID}) 
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PO} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PO} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PO.POMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = PO.POMasterID
                                    LEFT JOIN (
                                            SELECT * FROM Approval.dbo.functionJoinListProxyEmployeeAPSubmitDate({AppContexts.User.EmployeeID})                                 
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
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.PO} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = PO.POMasterID
                                WHERE ISNULL(POC.ServiceTypeCount, 0) > 0 AND PO.ApprovalStatusID={(int)Util.ApprovalStatus.Approved}
							{where} {filter}
                            ";
            //WHERE(VA.EmployeeID = { AppContexts.User.EmployeeID} OR F.EmployeeID = { AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = { AppContexts.User.EmployeeID}) { filter}
            var result = PurchaseOrderMasterRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }





    }
}
