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
    public class PurchaseRequisitionManager : ManagerBase, IPurchaseRequisitionManager
    {

        private readonly IRepository<PurchaseRequisitionMaster> PurchaseRequisitionMasterRepo;
        private readonly IRepository<PurchaseRequisitionChild> PurchaseRequisitionChildRepo;
        private readonly IRepository<PurchaseRequisitionQuotation> PurchaseRequisitionQuotationRepo;
        private readonly IRepository<Item> ItemRepo;
        private readonly IRepository<PRNFAMap> PRNFAMapRepo;
        private readonly IRepository<PurchaseRequisitionChildCostCenterBudget> PurchaseRequisitionChildCostCenterBudgetRepo;
        private readonly IRepository<PurchaseRequisitionQuotationItemMap> PurchaseRequisitionQuotationItemMapRepo;
        private readonly IRepository<VendorAssessmentMembers> VendorAssessmentMambersRepo;
        public PurchaseRequisitionManager(IRepository<PurchaseRequisitionMaster> purchaseRequisitionMasterRepo, IRepository<PurchaseRequisitionChild> purchaseRequisitionChildRepo, IRepository<PurchaseRequisitionQuotation> purchaseRequisitionQuotationRepo, IRepository<Item> itemRepo, IRepository<PurchaseRequisitionChildCostCenterBudget> purchaseRequisitionChildCostCenterBudgetRepo,
            IRepository<PurchaseRequisitionQuotationItemMap> purchaseRequisitionQuotationItemMapRepo, IRepository<VendorAssessmentMembers> vendorAssessmentMambersRepo
            , IRepository<PRNFAMap> prNFAMapRepo)
        {
            PurchaseRequisitionMasterRepo = purchaseRequisitionMasterRepo;
            PurchaseRequisitionChildRepo = purchaseRequisitionChildRepo;
            PurchaseRequisitionQuotationRepo = purchaseRequisitionQuotationRepo;
            ItemRepo = itemRepo;
            PRNFAMapRepo = prNFAMapRepo;
            PurchaseRequisitionChildCostCenterBudgetRepo = purchaseRequisitionChildCostCenterBudgetRepo;
            PurchaseRequisitionQuotationItemMapRepo = purchaseRequisitionQuotationItemMapRepo;
            VendorAssessmentMambersRepo = vendorAssessmentMambersRepo;
        }

        public async Task<(bool, string)> SaveChanges(PurchaseRequisitionDto PR)
        {

            //var existingPRByMR = PurchaseRequisitionMasterRepo.Entities.Where(x => PR.PRMasterID == 0 && x.MRMasterID == PR.MRMasterID).SingleOrDefault();
            //if (existingPRByMR.IsNotNull())
            //{
            //    return (false, $"Sorry, PR already exists by this MR");
            //}

            foreach (var item in PR.Attachments)
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

            var existingPR = PurchaseRequisitionMasterRepo.Entities.Where(x => x.PRMasterID == PR.PRMasterID).SingleOrDefault();
            var removeList = RemoveAttachments(PR);


            var existingPRNFAMap = PRNFAMapRepo.Entities.Where(x => x.PRNFAMapID == PR.PRNFAMap.PRNFAMapID).SingleOrDefault();


            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (PR.PRMasterID > 0 && PR.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                                
                            FROM PurchaseRequisitionMaster PurchaseRequisition
                            LEFT JOIN Security..Users U ON U.UserID = PurchaseRequisition.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PurchaseRequisition.PRMasterID AND AP.APTypeID =  {(int)Util.ApprovalType.PR}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID =  {(int)Util.ApprovalType.PR} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PR} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PurchaseRequisition.PRMasterID                                       
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {PR.ApprovalProcessID}";
                var canReassesstment = PurchaseRequisitionMasterRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, $"Sorry, You can not edit Purchase Requisition once it processed from approval panel");
                    }
                }
                else
                {
                    return (false, $"Sorry, You can not edit Purchase Requisition once it processed from approval panel");
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)PR.PRMasterID, PR.ApprovalProcessID, (int)Util.ApprovalType.PR);
            }

            string ApprovalProcessID = "0";
            var masterModel = new PurchaseRequisitionMaster
            {
                Subject = PR.Subject,
                Preamble = PR.Preamble,
                Description = PR.Description,
                RequiredByDate = PR.RequiredByDate,
                BudgetPlanRemarks = PR.BudgetPlanRemarks,
                GrandTotal = PR.ItemDetails.Select(x => x.Amount).DefaultIfEmpty(0).Sum(),
                ReferenceKeyword = PR.ReferenceKeyword,
                DeliveryLocation = PR.DeliveryLocation,
                IsDraft = PR.IsDraft,
                ApprovalStatusID = PR.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
                MRMasterID = (int)PR.MRMasterID,
                //NFAID= (int)PR.NFAID,
                //NFAAmount= PR.NFAAmount,
            };
            var map = new PRNFAMap
            {
                PRMID = PR.PRNFAMap.PRMID,
                NFAID = PR.PRNFAMap.NFAID,
                NFAReferenceNo = PR.PRNFAMap.NFAReferenceNo,
                NFAAmount = PR.PRNFAMap.NFAAmount,
                IsFromSystem = PR.PRNFAMap.IsFromSystem
            };

            using (var unitOfWork = new UnitOfWork())
            {
                if (PR.PRMasterID.IsZero() && existingPR.IsNull())
                {
                    masterModel.PRDate = DateTime.Now;
                    masterModel.ReferenceNo = GeneratePurchaseRequisitionReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");
                    //masterModel.ApprovalStatusID = (int)ApprovalStatus.Pending;
                    masterModel.SetAdded();
                    SetPurchaseRequisitionMasterNewId(masterModel);
                    PR.PRMasterID = (int)masterModel.PRMasterID;
                }
                else
                {
                    masterModel.CreatedBy = existingPR.CreatedBy;
                    masterModel.CreatedDate = existingPR.CreatedDate;
                    masterModel.CreatedIP = existingPR.CreatedIP;
                    masterModel.RowVersion = existingPR.RowVersion;
                    masterModel.PRMasterID = PR.PRMasterID;
                    masterModel.ReferenceNo = existingPR.ReferenceNo;
                    masterModel.PRDate = existingPR.PRDate;
                    masterModel.BudgetPlanRemarks = existingPR.BudgetPlanRemarks;
                    //masterModel.ApprovalStatusID = existingPR.ApprovalStatusID;
                    masterModel.IsSingleQuotation = existingPR.IsSingleQuotation;
                    masterModel.SCMRemarks = existingPR.SCMRemarks;
                    masterModel.MRMasterID = existingPR.MRMasterID;
                    masterModel.SetModified();
                }

                //Save PRNFAMap
                if (PR.PRNFAMap.PRNFAMapID.IsZero() && existingPRNFAMap.IsNull())
                {
                    map.SetAdded();
                    SetPRNFAMapNewId(map);
                    PR.PRNFAMap.PRNFAMapID = (int)map.PRNFAMapID;
                    map.PRMID = (int)masterModel.PRMasterID;
                }
                else
                {
                    map.CreatedBy = existingPRNFAMap.CreatedBy;
                    map.CreatedDate = existingPRNFAMap.CreatedDate;
                    map.CreatedIP = existingPRNFAMap.CreatedIP;
                    map.RowVersion = existingPRNFAMap.RowVersion;
                    map.PRNFAMapID = existingPRNFAMap.PRNFAMapID;

                    map.PRMID = PR.PRMasterID;
                    map.NFAID = PR.PRNFAMap.NFAID;
                    map.NFAReferenceNo = PR.PRNFAMap.NFAReferenceNo;
                    map.NFAAmount = PR.PRNFAMap.NFAAmount;
                    map.IsFromSystem = PR.PRNFAMap.IsFromSystem;

                    map.SetModified();
                }
                //End of Save PRNFAMap

                var childModel = GeneratePurchaseRequisitionChild(PR);
                var childModelQuotation = GeneratePurchaseRequisitionQuotation(childModel);

                SetAuditFields(masterModel);
                SetAuditFields(childModel);
                SetAuditFields(childModelQuotation);
                SetAuditFields(map);



                if (PR.Attachments.IsNotNull() && PR.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(PR.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)PR.PRMasterID, "PurchaseRequisitionMaster", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)PR.PRMasterID, "PurchaseRequisitionMaster", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }

                PurchaseRequisitionMasterRepo.Add(masterModel);
                PRNFAMapRepo.Add(map);
                PurchaseRequisitionChildRepo.AddRange(childModel);
                PurchaseRequisitionQuotationRepo.AddRange(childModelQuotation);

                if (!masterModel.IsDraft)
                {
                    if (masterModel.IsAdded || (existingPR.IsDraft && masterModel.IsModified))
                    {
                        string approvalTitle = $"{Util.PRApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, Purchase Requisition Reference No:{masterModel.ReferenceNo}";
                        var obj = CreateApprovalProcessForLimit((int)masterModel.PRMasterID, Util.AutoPRAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.PR, masterModel.GrandTotal, "0");
                        ApprovalProcessID = obj.ApprovalProcessID;
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
                }
                unitOfWork.CommitChangesWithAudit();
                if (!masterModel.IsDraft)
                {
                    if (ApprovalProcessID.ToInt() > 0)
                        // await Extension.Post<string>($"/SendMail/SendEMailToRecipients", "Test API Call");
                        await SendMailFromManagerBase(ApprovalProcessID, false, masterModel.PRMasterID, (int)Util.MailGroupSetup.PRInitiatedMail, (int)Util.ApprovalType.PR);

                }
            }
            await Task.CompletedTask;

            return (true, $"Purchase Requisition Submitted Successfully"); ;
        }

        private void SetPurchaseRequisitionMasterNewId(PurchaseRequisitionMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("PurchaseRequisitionMaster", AppContexts.User.CompanyID);
            master.PRMasterID = code.MaxNumber;
        }

        private void SetPRNFAMapNewId(PRNFAMap master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("PRNFAMap", AppContexts.User.CompanyID);
            master.PRNFAMapID = code.MaxNumber;
        }

        private List<PurchaseRequisitionChild> GeneratePurchaseRequisitionChild(PurchaseRequisitionDto PR)
        {
            var existingPurchaseRequisitionChild = PurchaseRequisitionChildRepo.Entities.Where(x => x.PRMasterID == PR.PRMasterID).ToList();
            var childModel = new List<PurchaseRequisitionChild>();
            if (PR.ItemDetails.IsNotNull())
            {
                PR.ItemDetails.ForEach(x =>
                {
                    if (x.ItemID > 0)
                    {
                        childModel.Add(new PurchaseRequisitionChild
                        {
                            PRCID = x.PRCID,
                            PRMasterID = PR.PRMasterID,
                            ItemID = x.ItemID,
                            Description = x.Description,
                            Qty = x.Qty,
                            UOM = x.UOM,
                            Price = x.Price,
                            Amount = x.Amount,
                            ForID = x.ForID
                        });
                    }

                });

                childModel.ForEach(x =>
                {
                    if (existingPurchaseRequisitionChild.Count > 0 && x.PRCID > 0)
                    {
                        var existingModelData = existingPurchaseRequisitionChild.FirstOrDefault(y => y.PRCID == x.PRCID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.PRMasterID = PR.PRMasterID;
                        x.SetAdded();
                        SetPurchaseRequisitionChildNewId(x);
                    }
                });

                var willDeleted = existingPurchaseRequisitionChild.Where(x => !childModel.Select(y => y.PRCID).Contains(x.PRCID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    childModel.Add(x);
                });
            }

            return childModel;
        }




        private List<PurchaseRequisitionQuotation> GeneratePurchaseRequisitionQuotation(List<PurchaseRequisitionChild> purchaseRequisitionChild)
        {
            var existingPurchaseRequisitionQuotation = PurchaseRequisitionQuotationRepo.Entities.Where(x => x.PRMasterID == purchaseRequisitionChild[0].PRMasterID).ToList();
            var quotationModel = new List<PurchaseRequisitionQuotation>();

            if (purchaseRequisitionChild.IsNotNull())
            {
                purchaseRequisitionChild.ForEach(x =>
                {
                    if (x.ItemID > 0 && !x.IsDeleted) // Check for isDeleted flag
                    {
                        // Add the item to quotationModel only if it is not deleted
                        quotationModel.Add(new PurchaseRequisitionQuotation
                        {
                            PRCID = x.PRCID,
                            PRMasterID = x.PRMasterID,
                            SupplierID = 1,
                            ItemID = x.ItemID,
                            Description = x.Description,
                            QuotedQty = x.Qty,
                            QuotedUnitPrice = x.Price,
                            Amount = x.Amount,
                            TaxTypeID = 1
                        });
                    }
                });

                quotationModel.ForEach(x =>
                {
                    if (existingPurchaseRequisitionQuotation.Count > 0 && x.PRCID > 0)
                    {
                        var existingModelData = existingPurchaseRequisitionQuotation.FirstOrDefault(y => y.PRCID == x.PRCID);
                        if(existingModelData != null && existingModelData.PRQID> 0)
                        {
                            x.PRQID = existingModelData.PRQID;
                            x.TaxTypeID = existingModelData.TaxTypeID;
                            x.SupplierID = existingModelData.SupplierID;
                            x.CreatedBy = existingModelData.CreatedBy;
                            x.CreatedDate = existingModelData.CreatedDate;
                            x.CreatedIP = existingModelData.CreatedIP;
                            x.RowVersion = existingModelData.RowVersion;
                            x.SetModified();
                        }
                        else
                        {
                            x.PRMasterID = purchaseRequisitionChild[0].PRMasterID;
                            x.SetAdded();
                            SetPurchaseRequisitionQuotationNewId(x);
                        }
                        
                    }
                    else
                    {
                        x.PRMasterID = purchaseRequisitionChild[0].PRMasterID;
                        x.SetAdded();
                        SetPurchaseRequisitionQuotationNewId(x);
                    }
                });

                var willDeleted = existingPurchaseRequisitionQuotation.Where(x => !quotationModel.Select(y => y.PRQID).Contains(x.PRQID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    quotationModel.Add(x);
                });
            }

            return quotationModel;
        }




        private void SetPurchaseRequisitionChildNewId(PurchaseRequisitionChild child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("PurchaseRequisitionChild", AppContexts.User.CompanyID);
            child.PRCID = code.MaxNumber;
        }

        private void SetPurchaseRequisitionQuotationNewId(PurchaseRequisitionQuotation child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("PurchaseRequisitionQuotation", AppContexts.User.CompanyID);
            child.PRQID = code.MaxNumber;
        }

        private string GeneratePurchaseRequisitionReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/PR/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{ GenerateSystemCode("PurchaseRequisitionRefNo", AppContexts.User.CompanyID).SystemCode}";
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
                        string filename = $"PurchaseRequisition-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "PurchaseRequisition\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

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
        private List<Attachment> RemoveAttachments(PurchaseRequisitionDto PR)
        {
            if (PR.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachment>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='PurchaseRequisitionMaster' AND ReferenceID={PR.PRMasterID}";
                var prevAttachment = PurchaseRequisitionMasterRepo.GetDataDictCollection(attachmentSql);

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
                var removeList = attachemntList.Where(x => !PR.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "PurchaseRequisition";
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
        public async Task<List<PurchaseRequisitionMasterDto>> GetPurchaseRequisitionList(string filterData)
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
                    filter = $@" AND PR.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND PR.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
	                            PR.*,
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
                                ,CountPO     
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                ,(
		                                SELECT POMasterID,ReferenceNo PORefNo,ApprovalProcessID,GrandTotal,SV.SystemVariableCode AS ApprovalStatus,PODate
		                                FROM PurchaseOrderMaster PO
		                                LEFT JOIN Approval..ApprovalProcess InAP ON InAP.ReferenceID = PO.POMasterID AND InAP.APTypeID =  {(int)Util.ApprovalType.PO}
                                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PO.ApprovalStatusID
		                                WHERE PRMasterID = PR.PRMasterID
		                                FOR JSON PATH
		                                ) AS PODetails
                            FROM PurchaseRequisitionMaster PR
                            LEFT JOIN Security..Users U ON U.UserID = PR.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PR.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PR.PRMasterID AND AP.APTypeID = {(int)Util.ApprovalType.PR}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PR} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PR} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PR.PRMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = PR.PRMasterID
                                    LEFT JOIN (
		                                    SELECT 
			                                    COUNT(PRMasterID) CountPO,PRMasterID 
		                                    FROM 
			                                    PurchaseOrderMaster
                                            
                                            WHERE ApprovalStatusID <> {(int)Util.ApprovalStatus.Rejected}
		                                    GROUP BY PRMasterID
	                                    ) PO ON PO.PRMasterID = PR.PRMasterID
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
													ApprovalProcessID,
													APPanelID
												FROM 
													Approval..ApprovalProcessPanelMap 
											)Panel ON Panel.ApprovalProcessID = AP.ApprovalProcessID
									LEFT JOIN (
												SELECT 
													APE.DepartmentID
													,APPE.EmployeeID
													,APE.APPanelID
												FROM Approval..ApprovalPanelProxyEmployee APPE
												INNER JOIN Approval..ApprovalPanelEmployee APE ON APPE.APPanelEmployeeID = APE.APPanelEmployeeID
												WHERE APPE.EmployeeID = {AppContexts.User.EmployeeID}
									) Prox ON Prox.DepartmentID = vA.DepartmentID AND Prox.APPanelID = Panel.APPanelID AND AEF.IsMultiProxy = 1
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID} OR 
                            Prox.EmployeeID = (CASE WHEN Va.EmployeeID = {AppContexts.User.EmployeeID} THEN -1 ELSE {AppContexts.User.EmployeeID} END)
                            ) {filter}
                            ORDER BY CreatedDate desc";
            var prList = PurchaseRequisitionMasterRepo.GetDataModelCollection<PurchaseRequisitionMasterDto>(sql);
            return prList;
        }

        public async Task<PurchaseRequisitionMasterDto> GetPurchaseRequisitionMaster(int PRMasterID, int isSCM = 0)
        {
            string balanceSql = "";
            if (isSCM == 0)
            {
                balanceSql = $@" AND pm.PRMasterID!={PRMasterID}";
            }

            string sql = $@"SELECT DISTINCT PR.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,W.WarehouseName DeliveryLocationName,WorkMobile
                            ,(SELECT Security.dbo.NumericToBDT(PR.GrandTotal)) AmountInWords
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            ,ISNULL(QPR.QAmount,0) TotalQuotedAmount
                            ,MR.ReferenceNo AS MRReferenceNo
                            ,MR.MRMasterID
                            ,ISNULL(AP1.ApprovalProcessID,0) MRApprovalProcessID
                            , SV1.SystemVariableCode AS BudgetPlanCategoryName
                            ,N.NFAReferenceNo NFANo
                            ,N.NFAAmount
							,CASE WHEN N.IsFromSystem = 0 THEN ISNULL(PRC1.AlreadyCreatedAmt, 0) ELSE ISNULL(PRC.AlreadyCreatedAmt, 0) END CreatedNFAAmount
                            		
		                    ,ISNULL(N.NFAAmount,0) - (CASE WHEN N.IsFromSystem = 0 THEN ISNULL(PRC1.AlreadyCreatedAmt, 0) ELSE ISNULL(PRC.AlreadyCreatedAmt, 0) END) Balance
                            ,ISNULL(AP2.ApprovalProcessID,0) NFAApprovalProcessID
							,N.IsFromSystem
							,N.NFAID
                            ,N.NFAReferenceNo
                            ,N.PRNFAMapID
                            
                        from PurchaseRequisitionMaster PR
                        LEFT JOIN PRNFAMap N ON N.PRMID=PR.PRMasterID
                        LEFT JOIN Warehouse W ON PR.DeliveryLocation=W.WarehouseID
                        LEFT JOIN Security..Users U ON U.UserID = PR.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = PR.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PR.PRMasterID AND AP.APTypeID = {(int)Util.ApprovalType.PR} 
                        LEFT JOIN MaterialRequisitionMaster MR ON MR.MRMasterID = PR.MRMasterID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV1 ON SV1.SystemVariableID = PR.BudgetPlanCategoryID
                        LEFT JOIN Approval..ApprovalProcess AP1 ON AP1.ReferenceID = MR.MRMasterID AND AP1.APTypeID = {(int)ApprovalType.MR}
                        LEFT JOIN Approval..ApprovalProcess AP2 ON AP2.ReferenceID = N.NFAID AND AP2.APTypeID = {(int)ApprovalType.NFA}
                        LEFT JOIN (SELECT SUM(ISNULL(pm.GrandTotal,0)) AlreadyCreatedAmt,M.NFAID FROM 
				                    PurchaseRequisitionMaster pm
				                    LEFT JOIN PRNFAMap M ON M.PRMID=pm.PRMasterID
				                    where pm.ApprovalStatusID<>24 AND pm.PRMasterID!={PRMasterID}--{balanceSql}
				                    Group By M.NFAID
	                    ) PRC ON PRC.NFAID=N.NFAID
						LEFT JOIN (SELECT SUM(ISNULL(pm.GrandTotal,0)) AlreadyCreatedAmt, M.NFAReferenceNo FROM 
									PurchaseRequisitionMaster pm
									LEFT JOIN PRNFAMap M ON M.PRMID=pm.PRMasterID AND M.IsFromSystem=0
									where pm.ApprovalStatusID<>24 AND pm.PRMasterID!={PRMasterID}--{balanceSql}
									Group By M.NFAReferenceNo
						) PRC1 ON PRC1.NFAReferenceNo=N.NFAReferenceNo
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PR}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PR} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PR.PRMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = PR.PRMasterID
                          LEFT JOIN (
										SELECT SUM(Amount) QAmount,PRMasterID FROM PurchaseRequisitionQuotation
										GROUP BY PRMasterID
									)QPR ON QPR.PRMasterID = PR.PRMasterID
                                LEFT JOIN 
								(
								    SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                        WHERE PR.PRMasterID={PRMasterID}  AND (VA.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID})";
            var master = PurchaseRequisitionMasterRepo.GetModelData<PurchaseRequisitionMasterDto>(sql);
            return master;
        }

        public async Task<PurchaseRequisitionMasterDto> GetApprovedPurchaseRequisitionMaster(int PRMasterID)
        {
            string sql = $@"SELECT PR.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,W.WarehouseName DeliveryLocationName,WorkMobile
                            ,(SELECT Security.dbo.NumericToBDT(PR.GrandTotal)) AmountInWords
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                            ,ISNULL(QPR.QAmount,0) TotalQuotedAmount
                            ,MR.ReferenceNo AS MRReferenceNo
                            ,MR.MRMasterID
                            ,ISNULL(AP1.ApprovalProcessID,0) MRApprovalProcessID
                            , SV1.SystemVariableCode AS BudgetPlanCategoryName
                        from PurchaseRequisitionMaster PR
                        LEFT JOIN Warehouse W ON PR.DeliveryLocation=W.WarehouseID
                        LEFT JOIN Security..Users U ON U.UserID = PR.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = PR.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PR.PRMasterID AND AP.APTypeID = {(int)Util.ApprovalType.PR} 
                        LEFT JOIN MaterialRequisitionMaster MR ON MR.MRMasterID = PR.MRMasterID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV1 ON SV1.SystemVariableID = PR.BudgetPlanCategoryID
                        LEFT JOIN Approval..ApprovalProcess AP1 ON AP1.ReferenceID = MR.MRMasterID AND AP1.APTypeID = {(int)ApprovalType.MR}

                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PR}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PR} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PR.PRMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = PR.PRMasterID
                          LEFT JOIN (
										SELECT SUM(Amount) QAmount,PRMasterID FROM PurchaseRequisitionQuotation
										GROUP BY PRMasterID
									)QPR ON QPR.PRMasterID = PR.PRMasterID
                        WHERE PR.PRMasterID={PRMasterID}";
            var master = PurchaseRequisitionMasterRepo.GetModelData<PurchaseRequisitionMasterDto>(sql);
            return master;
        }
        public async Task<PurchaseRequisitionMasterDto> GetNFABalanceByPRID(long PRMasterID, int? nfaid)
        {
            string sql = $@"SELECT DISTINCT 
                            N.ReferenceNo NFANo
                            ,N.GrandTotal NFAAmount
							,ISNULL(PRC.AlreadyCreatedAmt, 0) CreatedNFAAmount
							
							,N.NFAID
                            ,N.ReferenceNo NFAReferenceNo
                            
                        from Security..NFAMaster N
                        LEFT JOIN (SELECT SUM(ISNULL(pc.Amount,0)) AlreadyCreatedAmt,M.NFAID FROM PurchaseRequisitionChild pc 
									LEFT JOIN PurchaseRequisitionMaster pm on pm.PRMasterID=pc.PRMasterID
                                    LEFT JOIN PRNFAMap M ON M.PRMID=pm.PRMasterID
									where pm.ApprovalStatusID<>24 AND pm.PRMasterID!={PRMasterID} and M.NFAID={nfaid.Value}
									Group By M.NFAID
						) PRC ON PRC.NFAID=N.NFAID
                        
                        WHERE N.NFAID={nfaid.Value}";
            var master = PurchaseRequisitionMasterRepo.GetModelData<PurchaseRequisitionMasterDto>(sql);
            return master;
        }
        
        
        public async Task<List<PurchaseRequisitionChildDto>> GetPurchaseRequisitionChild(int PRMasterID)
        {
            string sql = $@"SELECT  PRC.*,
					        VA.DepartmentID,
					        VA.DepartmentName,
					        VA.FullName AS EmployeeName,
					        SV.SystemVariableCode AS ApprovalStatus,
					        VA.ImagePath,
					        VA.EmployeeCode,
					        VA.DivisionID,
					        VA.DivisionName,
					        I.ItemCode+'-'+I.ItemName AS ItemName,
                            I.InventoryTypeID,
					        Unit.UnitCode,
					        C.CostCenterName,
					        ISNULL(PO.Amount,0) AS PurchasedAmount,
					        ISNULL(PO.Qty,0) AS PurchasedQty
                    FROM PurchaseRequisitionChild PRC
                        LEFT JOIN PurchaseRequisitionMaster PRM ON PRM.PRMasterID = PRC.PRMasterID
                        LEFT JOIN PurchaseRequisitionQuotation PRQ ON PRQ.PRCID = PRC.PRCID
						LEFT JOIN (	SELECT POC.ItemID,SUM(POC.Qty) Qty,SUM(POC.TotalAmountIncludingVat) Amount,PRMasterID, PRQID FROM PurchaseOrderChild POC
									JOIN PurchaseOrderMaster POM ON POC.POMasterID=POM.POMasterID 
									WHERE POM.ApprovalStatusID	<> 24
									GROUP BY POC.ItemID,PRMasterID,PRQID) PO ON PRC.ItemID=PO.ItemID AND PO.PRMasterID = PRM.PRMasterID AND PRQ.PRQID = PO.PRQID
                        LEFT JOIN Item I ON I.ItemID = PRC.ItemID
                        LEFT JOIN Security..Unit  ON Unit.UnitID=PRC.UOM
						LEFT JOIN Accounts..CostCenter C  ON C.CostCenterID=PRC.ForID
                        LEFT JOIN Security..Users U ON U.UserID = PRC.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PRM.ApprovalStatusID
                        WHERE PRC.PRMasterID={PRMasterID}";
            var childs = PurchaseRequisitionChildRepo.GetDataModelCollection<PurchaseRequisitionChildDto>(sql);
            return childs;
        }

        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.PR);
            return comments.Result;
        }
        public List<Attachments> GetAttachments(int PRMasterID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='PurchaseRequisitionMaster' AND ReferenceID={PRMasterID}";
            var attachment = PurchaseRequisitionMasterRepo.GetDataDictCollection(attachmentSql);
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
        public async Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID)
        {
            return GetApprovalForwardingMembers(ApprovalProcessID).Result;
        }
        public async Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId)
        {
            return GetApprovalRejectedMembers(aprovalProcessId).Result;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetSuppliers(string param)
        {
            string sql = $@"SELECT s.SupplierID value,
                            ISNULL(s.SupplierName, '') label,
                            ISNULL(s.CorrespondingAddress, '') CorrespondingAddress,
                            ISNULL(s.RegisteredAddress, '') RegisteredAddress
                            ,'' SelectedSupplier,
                            s.PhoneNumber
                            FROM Supplier s WHERE SupplierName LIKE '%{param}%'
                            ORDER BY SupplierName ASC";

            var listDict = ItemRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }
        public async Task<List<PurchaseRequisitionMasterDto>> GetAllApproved()
        {
            string sql = $@"SELECT 
	                            PR.*,
	                            VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
	                            ,WorkMobile
                                ,AP.ApprovalProcessID
                                ,CountPO 
                                ,(
		                                SELECT POMasterID,ReferenceNo PORefNo,ApprovalProcessID,GrandTotal,SV.SystemVariableCode AS ApprovalStatus,PODate
		                                FROM PurchaseOrderMaster PO
		                                LEFT JOIN Approval..ApprovalProcess InAP ON InAP.ReferenceID = PO.POMasterID AND InAP.APTypeID =  {(int)Util.ApprovalType.PO}
                                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PO.ApprovalStatusID
		                                WHERE PRMasterID = PR.PRMasterID
		                                FOR JSON PATH
		                                ) AS PODetails 
                            FROM PurchaseRequisitionMaster PR
                            LEFT JOIN Security..Users U ON U.UserID = PR.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PR.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PR.PRMasterID AND AP.APTypeID = {(int)Util.ApprovalType.PR}
                            LEFT JOIN (
		                                    SELECT 
			                                    COUNT(PRMasterID) CountPO,PRMasterID 
		                                    FROM 
			                                    PurchaseOrderMaster
                                            WHERE ApprovalStatusID <> {(int)Util.ApprovalStatus.Rejected}
		                                    GROUP BY PRMasterID
	                                    ) PO ON PO.PRMasterID = PR.PRMasterID
							WHERE PR.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved} 
                            ORDER BY CreatedDate desc";
            var prList = PurchaseRequisitionMasterRepo.GetDataModelCollection<PurchaseRequisitionMasterDto>(sql);
            return prList;
        }

        public List<QuotationDto> GetQuotations(int PRMasterID)
        {
            string quotationSql = $@"SELECT PRQ.*,
                                        S.SupplierName,
                                        I.ItemName,
                                        I.ItemCode,
                                        Unit.UnitCode,
										IIF(PRQ.TaxTypeID=1,'YES','NO') TaxTypeString,
										ISNULL(PO.Amount,0) AS PurchasedAmount,
										ISNULL(PO.Qty,0) AS PurchasedQty
                                            FROM PurchaseRequisitionQuotation PRQ
                                            LEFT JOIN Supplier S ON PRQ.SupplierID=S.SupplierID 
                                            LEFT JOIN Item I ON PRQ.ItemID=I.ItemID
                                            LEFT JOIN Security..Unit  ON Unit.UnitID=I.UnitID
											LEFT JOIN (	SELECT POC.ItemID,SUM(POC.Qty) Qty,SUM(POC.TotalAmountIncludingVat) Amount,PRQID FROM PurchaseOrderChild POC
											JOIN PurchaseOrderMaster POM ON POC.POMasterID=POM.POMasterID 
											WHERE POM.ApprovalStatusID	<> 24
											GROUP BY POC.ItemID,PRQID) PO ON PRQ.ItemID=PO.ItemID AND PO.PRQID = PRQ.PRQID
                                        WHERE PRQ.PRMasterID={PRMasterID}";
            var quotations = PurchaseRequisitionMasterRepo.GetDataModelCollection<QuotationDto>(quotationSql);
            return quotations;


        }
        public List<Attachments> GetAssesments(int PRMasterID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='PurchaseRequisitionQuotation' AND ReferenceID={PRMasterID}";
            var attachment = PurchaseRequisitionMasterRepo.GetDataDictCollection(attachmentSql);
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
        public bool GetIsAssessmentMember()
        {
            bool isAssessmentMember = VendorAssessmentMambersRepo.Entities.Where(x => x.EmployeeID == AppContexts.User.EmployeeID).Any();
            return isAssessmentMember;
        }
        public async Task<List<PurchaseRequisitionChildDto>> GetPurchaseRequisitionChildForPOOld(int PRMasterID)
        {
            string sql = $@"SELECT  PRC.PRCID
                        	,PRC.ItemID
                        	,PRC.Description
                        	,PRC.ForID
                        	,PRC.UOM
                        	,PRC.Qty
                        	,PRC.Price
                        	,PRC.Amount AS PRAmount
                        	,PRC.PRMasterID
                        	,PRC.Remarks
                        	,PRC.CompanyID
                        	,PRC.CreatedBy
                        	,PRC.CreatedDate
                        	,PRC.CreatedIP
                        	,PRC.UpdatedBy
                        	,PRC.UpdatedDate
                        	,PRC.UpdatedIP
                        	,PRC.ROWVERSION
                        	,VA.DepartmentID
                        	,VA.DepartmentName
                        	,VA.FullName AS EmployeeName
                        	,SV.SystemVariableCode AS ApprovalStatus
                        	,VA.ImagePath
                        	,VA.EmployeeCode
                        	,VA.DivisionID
                        	,VA.DivisionName
                        	,I.ItemName
                        	,Security.dbo.Unit.UnitCode
                        	,C.CostCenterName
                        	,ISNULL(PO.Amount, 0) AS PurchasedAmount
                        	,ISNULL(PO.Qty, 0) AS PurchasedQty
                            ,I.InventoryTypeID
                        FROM PurchaseRequisitionChild PRC
                        LEFT JOIN PurchaseRequisitionMaster PRM ON PRM.PRMasterID = PRC.PRMasterID
						LEFT JOIN (	SELECT POC.ItemID,SUM(POC.Qty) Qty,SUM(POC.TotalAmountIncludingVat) Amount FROM PurchaseOrderChild POC
									JOIN PurchaseOrderMaster POM ON POC.POMasterID=POM.POMasterID 
									WHERE POM.ApprovalStatusID	<> 24 AND POM.PRMasterID={PRMasterID}
									GROUP BY POC.ItemID) PO ON PRC.ItemID=PO.ItemID
                        LEFT JOIN Item I ON I.ItemID=PRC.ItemID
                        LEFT JOIN Security..Unit  ON Unit.UnitID=PRC.UOM
						LEFT JOIN Accounts..CostCenter C  ON C.CostCenterID=PRC.ForID
                        LEFT JOIN Security..Users U ON U.UserID = PRC.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PRM.ApprovalStatusID
                        WHERE PRC.PRMasterID={PRMasterID}";
            var childs = PurchaseRequisitionChildRepo.GetDataModelCollection<PurchaseRequisitionChildDto>(sql);
            return childs;
        }


        public async Task<List<PurchaseRequisitionChildDto>> GetPurchaseRequisitionChildForPO(int PRMasterID)
        {
            string sql = $@"SELECT  
                            PRC.PRQID PRCID
                             --PRC.PRCID
                        	,PRC.ItemID
                        	,PRC.Description
                        	,I.UnitID UOM
                        	,PRC.QuotedQty Qty
                        	,PRC.QuotedUnitPrice Price
                        	,PRC.Amount AS PRAmount
                        	,PRC.PRMasterID
                        	,PRC.Description Remarks
                        	,PRC.CompanyID
                        	,PRC.CreatedBy
                        	,PRC.CreatedDate
                        	,PRC.CreatedIP
                        	,PRC.UpdatedBy
                        	,PRC.UpdatedDate
                        	,PRC.UpdatedIP
                        	,PRC.ROWVERSION
                        	,VA.DepartmentID
                        	,VA.DepartmentName
                        	,VA.FullName AS EmployeeName
                        	,SV.SystemVariableCode AS ApprovalStatus
                        	,VA.ImagePath
                        	,VA.EmployeeCode
                        	,VA.DivisionID
                        	,VA.DivisionName
                            ,I.ItemCode
                        	,I.ItemName
                        	,Security.dbo.Unit.UnitCode
                        	,ISNULL(PO.Amount, 0) AS PurchasedAmount
                        	,ISNULL(PO.Qty, 0) AS PurchasedQty
                            ,PRC.SupplierID
                        FROM PurchaseRequisitionQuotation PRC
                        LEFT JOIN PurchaseRequisitionMaster PRM ON PRM.PRMasterID = PRC.PRMasterID
						LEFT JOIN (	SELECT POC.ItemID,PRQID,SUM(POC.Qty) Qty,SUM(POC.TotalAmountIncludingVat) Amount FROM PurchaseOrderChild POC
									JOIN PurchaseOrderMaster POM ON POC.POMasterID=POM.POMasterID 
									WHERE POM.ApprovalStatusID	<> 24 AND POM.PRMasterID={PRMasterID}
									GROUP BY POC.ItemID,POC.PRQID) PO ON PRC.ItemID=PO.ItemID AND PO.PRQID = PRC.PRQID
                        LEFT JOIN Item I ON I.ItemID=PRC.ItemID
                        LEFT JOIN Security..Unit  ON Unit.UnitID=I.UnitID
                        LEFT JOIN Security..Users U ON U.UserID = PRC.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PRM.ApprovalStatusID
                        WHERE PRC.PRMasterID={PRMasterID}";
            var childs = PurchaseRequisitionChildRepo.GetDataModelCollection<PurchaseRequisitionChildDto>(sql);
            return childs;
        }

        public IEnumerable<Dictionary<string, object>> ReportForPRApprovalFeedback(int PRID)
        {
            string sql = $@" EXEC SCM..spRPTPRApprovalFeedback {PRID}";
            var feedback = PurchaseRequisitionMasterRepo.GetDataDictCollection(sql);
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
            var comments = PurchaseRequisitionMasterRepo.GetDataDictCollection(sql);
            return comments;
        }

        public async Task<List<PRChildCostCenterBudgetDto>> GetPurchaseRequisitionChildCostCenterBudget(int PRMasterID)
        {
            string sql = @$"SELECT        
                            *, C.CostCenterName
                             FROM PurchaseRequisitionChildCostCenterBudget P
                            JOIN Accounts..CostCenter C ON P.ForID = C.CostCenterID
                            WHERE P.PRMasterID = {PRMasterID}";
            var budgetDetails = PurchaseRequisitionChildCostCenterBudgetRepo.GetDataModelCollection<PRChildCostCenterBudgetDto>(sql);
            return budgetDetails;
        }

        public async Task<GridModel> GetPRListForGrid(GridParameter parameters)
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
                    filter = $@" AND PR.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND PR.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
	                            --PR.*,
                                 PR.PRMasterID
								,PR.ReferenceNo
                                ,NF.ReferenceNo NfaReferenceNo
                                ,ISNULL(NF.ReferenceNo,'') + PR.ReferenceNo COLLATE Latin1_General_CI_AS AS PRAndNfaRefNo
                                ,PR.Subject
								,PR.CreatedDate
								,PR.PRDate
                                ,PR.Subject AS Subjects
                                ,VA.DepartmentID
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
                                ,CountPO     
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                ,(
		                                SELECT POMasterID,ReferenceNo PORefNo,ApprovalProcessID,GrandTotal,SV.SystemVariableCode AS ApprovalStatus,PODate
		                                FROM PurchaseOrderMaster PO
		                                LEFT JOIN Approval..ApprovalProcess InAP ON InAP.ReferenceID = PO.POMasterID AND InAP.APTypeID =  {(int)Util.ApprovalType.PO}
                                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PO.ApprovalStatusID
		                                WHERE PRMasterID = PR.PRMasterID
		                                FOR JSON PATH
		                                ) AS PODetails
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                            FROM PurchaseRequisitionMaster PR
                           LEFT JOIN (SELECT NfaMap.PRMID,NfaMap.NFAID,Nfa.ReferenceNo from PRNFAMap NfaMap
												LEFT JOIN Security..NFAMaster Nfa ON NfaMap.NFAID=Nfa.NFAID)NF ON PR.PRMasterID=NF.PRMID
                            LEFT JOIN Security..Users U ON U.UserID = PR.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PR.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PR.PRMasterID AND AP.APTypeID = {(int)Util.ApprovalType.PR}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.PR} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.PR} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = PR.PRMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = PR.PRMasterID
                                    LEFT JOIN (
		                                    SELECT 
			                                    COUNT(PRMasterID) CountPO,PRMasterID 
		                                    FROM 
			                                    PurchaseOrderMaster
                                            
                                            WHERE ApprovalStatusID <> {(int)Util.ApprovalStatus.Rejected}
		                                    GROUP BY PRMasterID
	                                    ) PO ON PO.PRMasterID = PR.PRMasterID
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
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.PR} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = PR.PRMasterID
							WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}                            
                            ) {filter}";
            var result = PurchaseRequisitionMasterRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }

        public async Task<GridModel> GetApprovePRListForGrid(GridParameter parameters)
        {
            string filter = $@" AND IsArchive={parameters.AdditionalFilterData}"; 
         

            string sql = $@"SELECT 
                                DISTINCT
	                             PR.ReferenceNo
								,PR.PRMasterID
								,PR.PRDate
								,PR.CreatedDate
								,PR.Subject
								,PR.ApprovalStatusID
                                ,PR.IsArchive
	                            ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
	                            ,WorkMobile
                                ,AP.ApprovalProcessID
                                ,CountPO 
                                ,(
		                                SELECT POMasterID,ReferenceNo PORefNo,ISNULL(ApprovalProcessID,0) ApprovalProcessID,GrandTotal,SV.SystemVariableCode AS ApprovalStatus,PODate
		                                FROM PurchaseOrderMaster PO
		                                LEFT JOIN Approval..ApprovalProcess InAP ON InAP.ReferenceID = PO.POMasterID AND InAP.APTypeID =  {(int)Util.ApprovalType.PO}
                                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PO.ApprovalStatusID
		                                WHERE PRMasterID = PR.PRMasterID
		                                FOR JSON PATH
		                                ) AS PODetails 
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                            FROM PurchaseRequisitionMaster PR
                            LEFT JOIN Security..Users U ON U.UserID = PR.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PR.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = PR.PRMasterID AND AP.APTypeID = {(int)Util.ApprovalType.PR}
                            LEFT JOIN (
		                                    SELECT 
			                                    COUNT(PRMasterID) CountPO,PRMasterID 
		                                    FROM 
			                                    PurchaseOrderMaster
                                            WHERE ApprovalStatusID <> {(int)Util.ApprovalStatus.Rejected}
		                                    GROUP BY PRMasterID
	                                    ) PO ON PO.PRMasterID = PR.PRMasterID
							WHERE PR.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved} {filter}";
            var result = PurchaseRequisitionMasterRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }

        public async Task SaveArchiveStatus(int PRMasterID, bool IsArchive)
        {
            var existingPR = PurchaseRequisitionMasterRepo.Entities.Where(x => x.PRMasterID == PRMasterID).SingleOrDefault();
            existingPR.IsArchive = IsArchive;
            using (var unitOfWork = new UnitOfWork())
            {
                existingPR.SetModified();
                SetAuditFields(existingPR);
                PurchaseRequisitionMasterRepo.Add(existingPR);
                unitOfWork.CommitChangesWithAudit();
            }
            await Task.CompletedTask;
        }

        public async Task<List<PurchaseRequisitionChildDto>> GetPurchaseRequisitionForReassessment(int PRMasterID, int POMasterID)
        {
            string sql = $@"SELECT  PRC.PRQID PRCID
                        	,PRC.ItemID
                        	,PRC.Description
                        	,I.UnitID UOM
                        	,PRC.QuotedQty Qty
                        	,PRC.QuotedUnitPrice Price
                        	,PRC.Amount AS PRAmount
                        	,PRC.PRMasterID
                        	,PRC.Description Remarks
                        	,PRC.CompanyID
                        	,PRC.CreatedBy
                        	,PRC.CreatedDate
                        	,PRC.CreatedIP
                        	,PRC.UpdatedBy
                        	,PRC.UpdatedDate
                        	,PRC.UpdatedIP
                        	,PRC.ROWVERSION
                        	,VA.DepartmentID
                        	,VA.DepartmentName
                        	,VA.FullName AS EmployeeName
                        	,SV.SystemVariableCode AS ApprovalStatus
                        	,VA.ImagePath
                        	,VA.EmployeeCode
                        	,VA.DivisionID
                        	,VA.DivisionName
                            ,I.ItemCode
                        	,I.ItemName
                        	,Security.dbo.Unit.UnitCode
                        	,ISNULL(PO.Amount, 0) AS PurchasedAmount
                        	,ISNULL(PO.Qty, 0) AS PurchasedQty
                            ,PRC.SupplierID
                        FROM PurchaseRequisitionQuotation PRC
                        LEFT JOIN PurchaseRequisitionMaster PRM ON PRM.PRMasterID = PRC.PRMasterID
						LEFT JOIN (	SELECT POC.ItemID,PRQID,SUM(POC.Qty) Qty,SUM(POC.TotalAmountIncludingVat) Amount FROM PurchaseOrderChild POC
									JOIN PurchaseOrderMaster POM ON POC.POMasterID=POM.POMasterID 
									WHERE POM.ApprovalStatusID	<> 24 AND POM.PRMasterID={PRMasterID} AND POM.POMasterID <> {POMasterID}
									GROUP BY POC.ItemID,POC.PRQID) PO ON PRC.ItemID=PO.ItemID AND PO.PRQID = PRC.PRQID
                        LEFT JOIN Item I ON I.ItemID=PRC.ItemID
                        LEFT JOIN Security..Unit  ON Unit.UnitID=I.UnitID
                        LEFT JOIN Security..Users U ON U.UserID = PRC.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PRM.ApprovalStatusID
                        WHERE PRC.PRMasterID={PRMasterID}";
            var childs = PurchaseRequisitionChildRepo.GetDataModelCollection<PurchaseRequisitionChildDto>(sql);
            return childs;
        }
    }
}
