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
    public class MaterialRequisitionManager : ManagerBase, IMaterialRequisitionManager
    {

        private readonly IRepository<MaterialRequisitionMaster> MaterialRequisitionMasterRepo;
        private readonly IRepository<MaterialRequisitionChild> MaterialRequisitionChildRepo;
        private readonly IRepository<Item> ItemRepo;
        private readonly IRepository<VendorAssessmentMembers> VendorAssessmentMambersRepo;
        public MaterialRequisitionManager(IRepository<MaterialRequisitionMaster> purchaseRequisitionMasterRepo, IRepository<MaterialRequisitionChild> purchaseRequisitionChildRepo, IRepository<Item> itemRepo, IRepository<VendorAssessmentMembers> vendorAssessmentMambersRepo)
        {
            MaterialRequisitionMasterRepo = purchaseRequisitionMasterRepo;
            MaterialRequisitionChildRepo = purchaseRequisitionChildRepo;
            ItemRepo = itemRepo;
            VendorAssessmentMambersRepo = vendorAssessmentMambersRepo;
        }

        public async Task<(bool, string)> SaveChanges(MaterialRequisitionDto MR)
        {
            var existingMR = MaterialRequisitionMasterRepo.Entities.Where(x => x.MRMasterID == MR.MRMasterID).SingleOrDefault();
            var removeList = RemoveAttachments(MR);

            if (MR.MRMasterID > 0 && (existingMR.IsNullOrDbNull() || existingMR.CreatedBy != AppContexts.User.UserID))
            {
                return (false, "You don't have permission to save this MR.");
            }

            foreach (var item in MR.Attachments)
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



            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (MR.MRMasterID > 0 && MR.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                                
                            FROM MaterialRequisitionMaster MaterialRequisition
                            LEFT JOIN Security..Users U ON U.UserID = MaterialRequisition.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MaterialRequisition.MRMasterID AND AP.APTypeID =  {(int)Util.ApprovalType.MR}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID =  {(int)Util.ApprovalType.MR} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.MR} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MaterialRequisition.MRMasterID                                       
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {MR.ApprovalProcessID}";
                var canReassesstment = MaterialRequisitionMasterRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, $"Sorry, You can not edit Material Requisition once it processed from approval panel");
                    }
                }
                else
                {
                    return (false, $"Sorry, You can not edit Material Requisition once it processed from approval panel");
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)MR.MRMasterID, MR.ApprovalProcessID, (int)Util.ApprovalType.MR);
            }

            string ApprovalProcessID = "0";
            var masterModel = new MaterialRequisitionMaster
            {
                Subject = MR.Subject,
                Preamble = MR.Preamble,
                Description = MR.Description,
                RequiredByDate = MR.RequiredByDate,
                GrandTotal = (decimal)MR.ItemDetails.Select(x => x.Amount).DefaultIfEmpty(0).Sum(),
                ReferenceKeyword = MR.ReferenceKeyword,
                DeliveryLocation = MR.DeliveryLocation,
                IsDraft = MR.IsDraft,
                ApprovalStatusID = MR.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
            };

            using (var unitOfWork = new UnitOfWork())
            {
                if (MR.MRMasterID.IsZero() && existingMR.IsNull())
                {
                    masterModel.MRDate = DateTime.Now;
                    masterModel.ReferenceNo = GenerateMaterialRequisitionReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");
                    //masterModel.ApprovalStatusID = (int)ApprovalStatus.Pending;
                    masterModel.SetAdded();
                    SetMaterialRequisitionMasterNewId(masterModel);
                    MR.MRMasterID = (int)masterModel.MRMasterID;
                }
                else
                {
                    masterModel.CreatedBy = existingMR.CreatedBy;
                    masterModel.CreatedDate = existingMR.CreatedDate;
                    masterModel.CreatedIP = existingMR.CreatedIP;
                    masterModel.RowVersion = existingMR.RowVersion;
                    masterModel.MRMasterID = MR.MRMasterID;
                    masterModel.ReferenceNo = existingMR.ReferenceNo;
                    masterModel.MRDate = existingMR.MRDate;
                    masterModel.SCMRemarks = existingMR.SCMRemarks;
                    masterModel.SetModified();
                }
                var childModel = GenerateMaterialRequisitionChild(MR);

                SetAuditFields(masterModel);
                SetAuditFields(childModel);



                if (MR.Attachments.IsNotNull() && MR.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(MR.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)MR.MRMasterID, "MaterialRequisitionMaster", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)MR.MRMasterID, "MaterialRequisitionMaster", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }

                MaterialRequisitionMasterRepo.Add(masterModel);
                MaterialRequisitionChildRepo.AddRange(childModel);

                //if (!masterModel.IsDraft)
                //{
                    //if (masterModel.IsAdded || (masterModel.IsDraft && masterModel.IsModified) || (existingMR.IsDraft && masterModel.IsModified))
                    if(masterModel.IsAdded || (masterModel.IsDraft && masterModel.IsModified))
                    {
                        if (MR.MRApprovalPanelList.IsNotNull() && MR.MRApprovalPanelList.Count > 0)
                        {
                            DeleteManualApprovalPanel((int)masterModel.MRMasterID, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext),(int)Util.ApprovalType.MR);
                            foreach (var item in MR.MRApprovalPanelList)
                            {
                                string MAPPanelEmployeeID = SaveManualApprovalPanel(item.EmployeeID, (int)Util.ApprovalPanel.MRBelowTheLimit, item.SequenceNo, item.ProxyEmployeeID.Value, item.IsProxyEmployeeEnabled, item.NFAApprovalSequenceType.Value, item.IsEditable, item.IsSCM, item.IsMultiProxy, (int)Util.ApprovalType.MR, (int)masterModel.MRMasterID, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext));
                                if (item.ManualMultipleProxyDetails.IsNotNull() && item.ManualMultipleProxyDetails.Count>0)
                                foreach (var subItem in item.ManualMultipleProxyDetails)
                                {
                                        SaveManualApprovalPanelMultiProxy(Convert.ToInt32(MAPPanelEmployeeID), (int)Util.ApprovalPanel.MRBelowTheLimit, subItem.DivisionID, subItem.DepartmentID, subItem.EmployeeID, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext));
                                    
                                }
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
                //}
                unitOfWork.CommitChangesWithAudit();




                if (!masterModel.IsDraft && MR.ApprovalProcessID == 0)
                {
                    string approvalTitle = $"{Util.MRApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, MR Reference No:{masterModel.ReferenceNo}";
                    var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
                    var obj = CreateManualApprovalProcess((int)masterModel.MRMasterID, Util.AutoMRAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.MR, (int)Util.ApprovalPanel.MRBelowTheLimit, context, AppContexts.GetDatabaseName(ConnectionName.ApprovalContext));
                    ApprovalProcessID = obj.ApprovalProcessID;
                    
                }

                if (!masterModel.IsDraft)
                {
                    if (ApprovalProcessID.ToInt() > 0)
                        // await Extension.Post<string>($"/SendMail/SendEMailToRecipients", "Test API Call");
                        await SendMailFromManagerBase(ApprovalProcessID, false, masterModel.MRMasterID, (int)Util.MailGroupSetup.MRInitiatedMail, (int)Util.ApprovalType.MR);
                }
            }
            await Task.CompletedTask;

            return (true, $"Material Requisition Submitted Successfully"); ;
        }

        private void SetMaterialRequisitionMasterNewId(MaterialRequisitionMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("MaterialRequisitionMaster", AppContexts.User.CompanyID);
            master.MRMasterID = code.MaxNumber;
        }

        private List<MaterialRequisitionChild> GenerateMaterialRequisitionChild(MaterialRequisitionDto MR)
        {
            var existingMaterialRequisitionChild = MaterialRequisitionChildRepo.Entities.Where(x => x.MRMasterID == MR.MRMasterID).ToList();
            var childModel = new List<MaterialRequisitionChild>();
            if (MR.ItemDetails.IsNotNull())
            {
                MR.ItemDetails.ForEach(x =>
                {
                    if (x.ItemID > 0)
                    {
                        childModel.Add(new MaterialRequisitionChild
                        {
                            MRCID = x.MRCID,
                            MRMasterID = MR.MRMasterID,
                            ItemID = x.ItemID,
                            Description = x.Description,
                            Qty = x.Qty,
                            UOM = x.UOM,
                            Price = x.Price,
                            Amount = x.Amount
                        });
                    }

                });

                childModel.ForEach(x =>
                {
                    if (existingMaterialRequisitionChild.Count > 0 && x.MRCID > 0)
                    {
                        var existingModelData = existingMaterialRequisitionChild.FirstOrDefault(y => y.MRCID == x.MRCID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.MRMasterID = MR.MRMasterID;
                        x.SetAdded();
                        SetMaterialRequisitionChildNewId(x);
                    }
                });

                var willDeleted = existingMaterialRequisitionChild.Where(x => !childModel.Select(y => y.MRCID).Contains(x.MRCID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    childModel.Add(x);
                });
            }

            return childModel;
        }

        private void SetMaterialRequisitionChildNewId(MaterialRequisitionChild child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("MaterialRequisitionChild", AppContexts.User.CompanyID);
            child.MRCID = code.MaxNumber;
        }

        private string GenerateMaterialRequisitionReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/MR/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{ GenerateSystemCode("MaterialRequisitionRefNo", AppContexts.User.CompanyID).MaxNumber}";
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
                        string filename = $"MaterialRequisition-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "MaterialRequisition\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

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
        private List<Attachment> RemoveAttachments(MaterialRequisitionDto MR)
        {
            if (MR.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachment>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='MaterialRequisitionMaster' AND ReferenceID={MR.MRMasterID}";
                var prevAttachment = MaterialRequisitionMasterRepo.GetDataDictCollection(attachmentSql);

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
                var removeList = attachemntList.Where(x => !MR.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "MaterialRequisition";
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
        public async Task<List<MaterialRequisitionMasterDto>> GetMaterialRequisitionList(string filterData)
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
                    filter = $@" AND MR.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND MR.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
	                            MR.*,
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
		                                FROM MaterialOrderMaster PO
		                                LEFT JOIN Approval..ApprovalProcess InAP ON InAP.ReferenceID = PO.POMasterID AND InAP.APTypeID =  {(int)Util.ApprovalType.PO}
                                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PO.ApprovalStatusID
		                                WHERE MRMasterID = MR.MRMasterID
		                                FOR JSON PATH
		                                ) AS PODetails
                            FROM MaterialRequisitionMaster MR
                            LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = MR.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MR.MRMasterID AND AP.APTypeID = {(int)Util.ApprovalType.MR}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.MR} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.MR} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MR.MRMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = MR.MRMasterID
                                    LEFT JOIN (
		                                    SELECT 
			                                    COUNT(MRMasterID) CountPO,MRMasterID 
		                                    FROM 
			                                    MaterialOrderMaster
                                            
                                            WHERE ApprovalStatusID <> {(int)Util.ApprovalStatus.Rejected}
		                                    GROUP BY MRMasterID
	                                    ) PO ON PO.MRMasterID = MR.MRMasterID
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
            var prList = MaterialRequisitionMasterRepo.GetDataModelCollection<MaterialRequisitionMasterDto>(sql);
            return prList;
        }

        public async Task<MaterialRequisitionMasterDto> GetMaterialRequisitionMaster(int MRMasterID)
        {
            string sql = $@"SELECT DISTINCT MR.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,W.WarehouseName DeliveryLocationName,WorkMobile
                            ,(SELECT Security.dbo.NumericToBDT(MR.GrandTotal)) AmountInWords
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                        from MaterialRequisitionMaster MR
                        LEFT JOIN Warehouse W ON MR.DeliveryLocation=W.WarehouseID
                        LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = MR.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MR.MRMasterID AND AP.APTypeID = {(int)Util.ApprovalType.MR} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.MR}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.MR} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MR.MRMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = MR.MRMasterID
                                LEFT JOIN 
								(
								    SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                         
                        WHERE MR.MRMasterID={MRMasterID}  AND (VA.EmployeeID =  {AppContexts.User.EmployeeID} OR F.EmployeeID =  {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID})";
            var master = MaterialRequisitionMasterRepo.GetModelData<MaterialRequisitionMasterDto>(sql);
            return master;
        }

        public async Task<MaterialRequisitionMasterDto> GetMaterialRequisitionMasterByID(int MRMasterID)
        {
            string sql = $@"SELECT DISTINCT MR.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,W.WarehouseName DeliveryLocationName,WorkMobile
                            ,(SELECT Security.dbo.NumericToBDT(MR.GrandTotal)) AmountInWords
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
                        from MaterialRequisitionMaster MR
                        LEFT JOIN Warehouse W ON MR.DeliveryLocation=W.WarehouseID
                        LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = MR.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MR.MRMasterID AND AP.APTypeID = {(int)Util.ApprovalType.MR} 
                        LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.MR}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.MR} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MR.MRMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = MR.MRMasterID
                                LEFT JOIN 
								(
								    SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                         
                        WHERE MR.MRMasterID={MRMasterID}";
            var master = MaterialRequisitionMasterRepo.GetModelData<MaterialRequisitionMasterDto>(sql);
            return master;
        }
        public async Task<List<MaterialRequisitionChildDto>> GetMaterialRequisitionChild(int MRMasterID)
        {
            string sql = $@"SELECT  MRC.*,
					        VA.DepartmentID,
					        VA.DepartmentName,
					        VA.FullName AS EmployeeName,
					        SV.SystemVariableCode AS ApprovalStatus,
					        VA.ImagePath,
					        VA.EmployeeCode,
					        VA.DivisionID,
					        VA.DivisionName,
					        I.ItemCode+'-'+I.ItemName AS ItemName,
					        Unit.UnitCode,
							I.InventoryTypeID
                    FROM MaterialRequisitionChild MRC
                        LEFT JOIN MaterialRequisitionMaster MRM ON MRM.MRMasterID = MRC.MRMasterID
                        LEFT JOIN Item I ON I.ItemID = MRC.ItemID
                        LEFT JOIN Security..Unit  ON Unit.UnitID=MRC.UOM
                        LEFT JOIN Security..Users U ON U.UserID = MRC.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = MRM.ApprovalStatusID
                        WHERE MRC.MRMasterID={MRMasterID}";
            var childs = MaterialRequisitionChildRepo.GetDataModelCollection<MaterialRequisitionChildDto>(sql);
            return childs;
        }

        public async Task<List<ManualApprovalPanelEmployeeDto>> GetMRApprovalPanelDefault(int MRMasterID)
        {
            string sql = $@"SELECT APE.*, Emp.EmployeeCode, Emp.FullName AS EmployeeName, EmpPr.FullName AS ProxyEmployeeName, AP.Name AS PanelName, SV.SystemVariableCode AS NFAApprovalSequenceTypeName                     
                            FROM Approval..ManualApprovalPanelEmployee APE
                            LEFT JOIN HRMS..ViewALLEmployee Emp ON APE.EmployeeID = Emp.EmployeeID							
                            LEFT JOIN HRMS..Employee EmpPr ON APE.ProxyEmployeeID = EmpPr.EmployeeID					
                            LEFT JOIN Approval..ApprovalPanel AP ON APE.APPanelID = AP.APPanelID
							LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = APE.NFAApprovalSequenceType
                            WHERE APE.ReferenceID={MRMasterID} AND APE.APTypeID ={(int)Util.ApprovalType.MR}";

            var maps = MaterialRequisitionMasterRepo.GetDataModelCollection<ManualApprovalPanelEmployeeDto>(sql);
            maps.ForEach(x =>
            {
                if (x.IsMultiProxy)
                {
                    string proxySql = @$"select mpp.EmployeeID,mpp.APPanelID, ve.DivisionID,ve.DepartmentID,ve.EmployeeCode, ve.FullName EmployeeName 
                                    from Approval..ManualApprovalPanelProxyEmployee mpp
                                    left join HRMS..ViewALLEmployee ve on ve.EmployeeID = mpp.EmployeeID
                                    where mpp.MAPPanelEmployeeID={x.MAPPanelEmployeeID}";
                    x.ManualMultipleProxyDetails = MaterialRequisitionMasterRepo.GetDataModelCollection<ManualMultipleProxyDetailsDto>(proxySql);
                }
            });
            return maps;
        }
        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.MR);
            return comments.Result;
        }
        public List<Attachments> GetAttachments(int MRMasterID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='MaterialRequisitionMaster' AND ReferenceID={MRMasterID}";
            var attachment = MaterialRequisitionMasterRepo.GetDataDictCollection(attachmentSql);
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

        public async Task<IEnumerable<Dictionary<string, object>>> GetsSupplierFromMRQuotation(int MRID)
        {
            string sql = $@"select prq.SupplierID value, ISNULL(s.SupplierName, '') label, ISNULL(s.CorrespondingAddress, '') CorrespondingAddress, ISNULL(s.RegisteredAddress, '') RegisteredAddress
                            ,'' SelectedSupplier, s.PhoneNumber
                            from MaterialRequisitionQuotation prq
                            left join Supplier s on prq.SupplierID = s.SupplierID
                            where prq.MRMasterID={MRID} 
                            ORDER BY prq.MRQID asc";

            var listDict = ItemRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }
        public async Task<List<MaterialRequisitionMasterDto>> GetAllApproved()
        {
            string sql = $@"SELECT 
	                            MR.*,
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
		                                FROM MaterialOrderMaster PO
		                                LEFT JOIN Approval..ApprovalProcess InAP ON InAP.ReferenceID = PO.POMasterID AND InAP.APTypeID =  {(int)Util.ApprovalType.PO}
                                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = PO.ApprovalStatusID
		                                WHERE MRMasterID = MR.MRMasterID
		                                FOR JSON PATH
		                                ) AS PODetails 
                            FROM MaterialRequisitionMaster MR
                            LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = MR.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MR.MRMasterID AND AP.APTypeID = {(int)Util.ApprovalType.MR}
                            LEFT JOIN (
		                                    SELECT 
			                                    COUNT(MRMasterID) CountPO,MRMasterID 
		                                    FROM 
			                                    MaterialOrderMaster
                                            WHERE ApprovalStatusID <> {(int)Util.ApprovalStatus.Rejected}
		                                    GROUP BY MRMasterID
	                                    ) PO ON PO.MRMasterID = MR.MRMasterID
							WHERE MR.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved} 
                            ORDER BY CreatedDate desc";
            var prList = MaterialRequisitionMasterRepo.GetDataModelCollection<MaterialRequisitionMasterDto>(sql);
            return prList;
        }

        public List<QuotationDto> GetQuotations(int MRMasterID)
        {
            string quotationSql = $@"SELECT MRQ.*,
                                        S.SupplierName,
                                        I.ItemName,
                                        I.ItemCode,
                                        Unit.UnitCode,
										IIF(MRQ.TaxTypeID=1,'YES','NO') TaxTypeString,
										ISNULL(PO.Amount,0) AS MaterialdAmount,
										ISNULL(PO.Qty,0) AS MaterialdQty
                                            FROM MaterialRequisitionQuotation MRQ
                                            LEFT JOIN Supplier S ON MRQ.SupplierID=S.SupplierID 
                                            LEFT JOIN Item I ON MRQ.ItemID=I.ItemID
                                            LEFT JOIN Security..Unit  ON Unit.UnitID=I.UnitID
											LEFT JOIN (	SELECT POC.ItemID,SUM(POC.Qty) Qty,SUM(POC.TotalAmountIncludingVat) Amount,MRQID,SupplierID FROM MaterialOrderChild POC
											JOIN MaterialOrderMaster POM ON POC.POMasterID=POM.POMasterID 
											WHERE POM.ApprovalStatusID	<> 24
											GROUP BY POC.ItemID,MRQID,SupplierID) PO ON MRQ.ItemID=PO.ItemID AND PO.MRQID = MRQ.MRQID
                                        WHERE MRQ.MRMasterID={MRMasterID}";
            var quotations = MaterialRequisitionMasterRepo.GetDataModelCollection<QuotationDto>(quotationSql);
            return quotations;


        }
        public List<Attachments> GetAssesments(int MRMasterID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='MaterialRequisitionQuotation' AND ReferenceID={MRMasterID}";
            var attachment = MaterialRequisitionMasterRepo.GetDataDictCollection(attachmentSql);
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
        public async Task<List<MaterialRequisitionChildDto>> GetMaterialRequisitionChildForPOOld(int MRMasterID)
        {
            string sql = $@"SELECT  MRC.MRCID
                        	,MRC.ItemID
                        	,MRC.Description
                        	,MRC.ForID
                        	,MRC.UOM
                        	,MRC.Qty
                        	,MRC.Price
                        	,MRC.Amount AS MRAmount
                        	,MRC.MRMasterID
                        	,MRC.Remarks
                        	,MRC.CompanyID
                        	,MRC.CreatedBy
                        	,MRC.CreatedDate
                        	,MRC.CreatedIP
                        	,MRC.UpdatedBy
                        	,MRC.UpdatedDate
                        	,MRC.UpdatedIP
                        	,MRC.ROWVERSION
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
                        	,ISNULL(PO.Amount, 0) AS MaterialdAmount
                        	,ISNULL(PO.Qty, 0) AS MaterialdQty
                            ,I.InventoryTypeID
                        FROM MaterialRequisitionChild MRC
                        LEFT JOIN MaterialRequisitionMaster MRM ON MRM.MRMasterID = MRC.MRMasterID
						LEFT JOIN (	SELECT POC.ItemID,SUM(POC.Qty) Qty,SUM(POC.TotalAmountIncludingVat) Amount FROM MaterialOrderChild POC
									JOIN MaterialOrderMaster POM ON POC.POMasterID=POM.POMasterID 
									WHERE POM.ApprovalStatusID	<> 24 AND POM.MRMasterID={MRMasterID}
									GROUP BY POC.ItemID) PO ON MRC.ItemID=PO.ItemID
                        LEFT JOIN Item I ON I.ItemID=MRC.ItemID
                        LEFT JOIN Security..Unit  ON Unit.UnitID=MRC.UOM
						LEFT JOIN Accounts..CostCenter C  ON C.CostCenterID=MRC.ForID
                        LEFT JOIN Security..Users U ON U.UserID = MRC.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = MRM.ApprovalStatusID
                        WHERE MRC.MRMasterID={MRMasterID}";
            var childs = MaterialRequisitionChildRepo.GetDataModelCollection<MaterialRequisitionChildDto>(sql);
            return childs;
        }


        public async Task<List<MaterialRequisitionChildDto>> GetMaterialRequisitionChildForPO(int MRMasterID)
        {
            string sql = $@"SELECT  MRC.MRQID MRCID
                        	,MRC.ItemID
                        	,(SELECT Description FROM MaterialRequisitionChild WHERE MRMasterID = MRC.MRMasterID AND ItemID = MRC.ItemID) Description
                        	,I.UnitID UOM
                        	,MRC.QuotedQty Qty
                        	,MRC.QuotedUnitPrice Price
                        	,MRC.Amount AS MRAmount
                        	,MRC.MRMasterID
                        	,MRC.Description Remarks
                        	,MRC.CompanyID
                        	,MRC.CreatedBy
                        	,MRC.CreatedDate
                        	,MRC.CreatedIP
                        	,MRC.UpdatedBy
                        	,MRC.UpdatedDate
                        	,MRC.UpdatedIP
                        	,MRC.ROWVERSION
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
                        	,ISNULL(PO.Amount, 0) AS MaterialdAmount
                        	,ISNULL(PO.Qty, 0) AS MaterialdQty
                            ,MRC.SupplierID
                        FROM MaterialRequisitionQuotation MRC
                        LEFT JOIN MaterialRequisitionMaster MRM ON MRM.MRMasterID = MRC.MRMasterID
						LEFT JOIN (	SELECT POC.ItemID,MRQID,SUM(POC.Qty) Qty,SUM(POC.TotalAmountIncludingVat) Amount FROM MaterialOrderChild POC
									JOIN MaterialOrderMaster POM ON POC.POMasterID=POM.POMasterID 
									WHERE POM.ApprovalStatusID	<> 24 AND POM.MRMasterID={MRMasterID}
									GROUP BY POC.ItemID,POC.MRQID) PO ON MRC.ItemID=PO.ItemID AND PO.MRQID = MRC.MRQID
                        LEFT JOIN Item I ON I.ItemID=MRC.ItemID
                        LEFT JOIN Security..Unit  ON Unit.UnitID=I.UnitID
                        LEFT JOIN Security..Users U ON U.UserID = MRC.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = MRM.ApprovalStatusID
                        WHERE MRC.MRMasterID={MRMasterID}";
            var childs = MaterialRequisitionChildRepo.GetDataModelCollection<MaterialRequisitionChildDto>(sql);
            return childs;
        }

        public IEnumerable<Dictionary<string, object>> ReportForMRApprovalFeedback(int MRID)
        {
            string sql = $@" EXEC SCM..spRPTMRApprovalFeedback {MRID}";
            var feedback = MaterialRequisitionMasterRepo.GetDataDictCollection(sql);
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
            var comments = MaterialRequisitionMasterRepo.GetDataDictCollection(sql);
            return comments;
        }

        public async Task<GridModel> GetMRListForGrid(GridParameter parameters)
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
                    filter = $@" AND MR.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND MR.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
	                             MR.MRMasterID
	                            ,MR.CreatedDate
								,MR.ReferenceNo
								,MR.MRDate
								,MR.ApprovalStatusID
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
                                
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
                                
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                            FROM MaterialRequisitionMaster MR
                            LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = MR.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MR.MRMasterID AND AP.APTypeID = {(int)Util.ApprovalType.MR}
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
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.MR} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.MR} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MR.MRMasterID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = MR.MRMasterID
                                   
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
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.MR} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = MR.MRMasterID
							WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}                            
                            ) {filter}";
            var result = MaterialRequisitionMasterRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }

        public async Task<GridModel> GetApproveMRListForGrid(GridParameter parameters)
        {
            string sql = $@"SELECT 
	                            DISTINCT
	                             Mr.MRMasterID
								,MR.ReferenceNo
								,MR.CreatedDate
								,MR.MRDate
								,MR.ApprovalStatusID
	                            ,VA.DepartmentID
	                            ,VA.DepartmentName
	                            ,VA.FullName AS EmployeeName, VA.DivisionID, VA.DivisionName
	                            ,SV.SystemVariableCode AS ApprovalStatus
	                            ,VA.ImagePath
	                            ,VA.EmployeeCode
	                            ,WorkMobile
                                ,AP.ApprovalProcessID
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PR.PRMasterID IS NULL THEN 0 ELSE 1 END AS Purchased
                                ,CASE WHEN MR.CreatedBy = {AppContexts.User.UserID} THEN 1 ELSE 0 END CanCreatePR
                                ,ISNULL(PR.PRMasterID,0) PRMasterID
                                ,ISNULL(PR.ReferenceNo,'Not Yet Created') PRNo
                                ,ISNULL(AP1.ApprovalProcessID,0) PRApprovalProcessID
                            FROM MaterialRequisitionMaster MR
                            LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployeeRegularJoin VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = MR.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MR.MRMasterID AND AP.APTypeID = {(int)ApprovalType.MR}
                            LEFT JOIN PurchaseRequisitionMaster PR ON PR.MRMasterID=MR.MRMasterID
                            LEFT JOIN Approval..ApprovalProcess AP1 ON AP1.ReferenceID = PR.PRMasterID AND AP1.APTypeID = {(int)ApprovalType.PR}
							WHERE MR.ApprovalStatusID = 23 AND MR.CreatedBy = {AppContexts.User.UserID}";
            var result = MaterialRequisitionMasterRepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }


        public async Task<List<ManualApprovalPanelEmployeeDto>> GetSCMMembersForPanel(int DivisionID)
        {

            string sql = $@"SELECT EmployeeID,21 APPanelID, 1 SequenceNo,0 ProxyEmployeeID, 1 IsProxyEmployeeEnabled,
                            62 NFAApprovalSequenceType, SystemVariableCode NFAApprovalSequenceTypeName,1 IsEditable,1 IsSCM, 1 IsMultiProxy, 14 APTypeID, 0 ReferenceID
			                ,EmployeeCode, EmployeeCode + '-' + FullName EmployeeName
			                FROM HRMS..ViewAllEmployee 
			                LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = 62
                            WHERE DivisionID={DivisionID} And DesignationID= {(int)Util.Desination.HeadOfSourcing}";

            var list = MaterialRequisitionMasterRepo.GetDataModelCollection<ManualApprovalPanelEmployeeDto>(sql);
            list.ForEach(x =>
            {
                if (x.IsMultiProxy)
                {
                    string proxySql = @$"select EmployeeID,21 APPanelID, DivisionID,DepartmentID,EmployeeCode, EmployeeCode + '-' + FullName EmployeeName from hrms..ViewALLEmployee where DivisionID={DivisionID} and DesignationID <> {(int)Util.Desination.HeadOfSourcing}";
                    x.ManualMultipleProxyDetails = MaterialRequisitionMasterRepo.GetDataModelCollection<ManualMultipleProxyDetailsDto>(proxySql);
                }
            });

            await Task.CompletedTask;
            return list;
        }
        public List<ManualApprovalPanelEmployeeDto> GetDefaultMRApprovalPanel()
        {
           
            List<ManualApprovalPanelEmployeeDto> apList = new List<ManualApprovalPanelEmployeeDto>();
            ManualApprovalPanelEmployeeDto seq1 = new ManualApprovalPanelEmployeeDto { APPanelID = (int)Util.ApprovalPanel.MRBelowTheLimit, EmployeeID = AppContexts.User.EmployeeID.Value, IsProxyEmployeeEnabled = false, ProxyEmployeeID = 0, APTypeID = (int)Util.ApprovalType.GRN, SequenceNo = 1, EmployeeCode = AppContexts.User.EmployeeCode, EmployeeName = AppContexts.User.EmployeeCode + "-" + AppContexts.User.FullName, NFAApprovalSequenceTypeName = "Proposed", NFAApprovalSequenceType = 62, IsSystemGenerated = true };
            apList.Add(seq1);
            
            return apList;
        }
        
    }
}
