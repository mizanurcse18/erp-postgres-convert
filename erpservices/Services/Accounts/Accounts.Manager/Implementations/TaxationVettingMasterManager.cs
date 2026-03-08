using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.EntityBase;
using DAL.Core.Extension;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Core.Util;

namespace Accounts.Manager
{
    public class TaxationVettingMasterManager : ManagerBase, ITaxationVettingMasterManager
    {
        private readonly IRepository<TaxationVettingMaster> TaxationVettingMasterRepo;
        private readonly IRepository<VatTaxDeductionSource> VatTaxDeductionSourceRepo; 
        //readonly IModelAdapter Adapter;
        public TaxationVettingMasterManager(IRepository<TaxationVettingMaster> taxationVettingMasterRepo, IRepository<VatTaxDeductionSource> vatTaxDeductionSourceRepo)
        {
            TaxationVettingMasterRepo = taxationVettingMasterRepo;
            VatTaxDeductionSourceRepo = vatTaxDeductionSourceRepo;
        }


        #region Save TaxationVetting & Releted Data
        public async Task<(bool, string)> SaveChanges(TaxationVettingMasterDto IV)
        {

            var existingIV = TaxationVettingMasterRepo.Entities.Where(x => x.TVMID == IV.TVMID).SingleOrDefault();
            var removeList = RemoveAttachments(IV);

            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (IV.TVMID > 0 && IV.ApprovalProcessID > 0)
            {
                string sql = $@"SELECT 
	                            
                               CASE WHEN (SELECT  {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END CanReassestment
                            FROM TaxationVettingMaster IV
                            LEFT JOIN Security..Users U ON U.UserID = IV.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = IV.TVMID AND AP.APTypeID = 15
                            LEFT JOIN (SELECT 
				                            APEmployeeFeedbackID,ApprovalProcessID,IsEditable 
			                            FROM 
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
			                            WHERE (AEF.APFeedbackID = 2 OR AEF.APFeedbackID = 9 OR AEF.APFeedbackID = 9) AND (EmployeeID = {AppContexts.User.EmployeeID} or ProxyEmployeeID = {AppContexts.User.EmployeeID})
                            )AEF ON AEF.ApprovalProcessID = AP.ApprovalProcessID
                            
                            LEFT JOIN 
									(
									SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM  {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback
									UNION ALL 
									SELECT DISTINCT ApprovalProcessID,EmployeeID,EmployeeID FROM {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo 
									)							
							F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                            LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback  AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = 15 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = 15 AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = IV.TVMID                                   
							WHERE (F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) AND Ap.ApprovalProcessID = {IV.ApprovalProcessID}";
                var canReassesstment = TaxationVettingMasterRepo.GetData(sql);
                if (canReassesstment.Count > 0)
                {
                    var canSubmit = (bool)canReassesstment["CanReassestment"];
                    if (!canSubmit)
                    {
                        return (false, $"Sorry, You can not edit Taxation Vetting once it processed from approval panel");
                    }
                }
                else
                {
                    return (false, $"Sorry, You can not edit Taxation Vetting once it processed from approval panel");
                }
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)IV.TVMID, IV.ApprovalProcessID, (int)Util.ApprovalType.TaxationVetting);
            }

            string ApprovalProcessID = "0";



            using (var unitOfWork = new UnitOfWork())
            {
                var masterModel = new TaxationVettingMaster
                {
                    TVMID = IV.TVMID,
                    InvoiceMasterID = IV.InvoiceMasterID,
                    PRMasterID = IV.PRMasterID,
                    POMasterID = IV.POMasterID,
                    VATRebatableID = IV.VATRebatableID,
                    VATRebatablePercent = IV.VATRebatablePercent,
                    VATRebatableAmount = IV.VATRebatableAmount,
                    VDSRateID = IV.VDSRateID,
                    VDSRatePercent = IV.VDSRatePercent,
                    VDSAmount = IV.VDSAmount,
                    TDSMethodID = IV.TDSMethodID,
                    TDSRateID = IV.TDSRateID,
                    TDSRate = IV.TDSRate,
                    TDSAmount = IV.TDSAmount,
                    GrandTotal = IV.GrandTotal,
                    Remarks = IV.Remarks,
                    ReferenceKeyword = IV.ReferenceKeyword,
                    IsDraft = IV.IsDraft,
                    ApprovalStatusID = IV.IsDraft ? (int)ApprovalStatus.Initiated : (int)ApprovalStatus.Pending,
                };
                if (IV.TVMID.IsZero() && existingIV.IsNull())
                {
                    masterModel.TVMDate = DateTime.Now;
                    masterModel.ReferenceNo = GenerateTaxationVettingMasterReference() + (string.IsNullOrWhiteSpace(masterModel.ReferenceKeyword) ? "" : $"/{masterModel.ReferenceKeyword.ToUpper()}");
                    masterModel.SetAdded();
                    SetTaxationVettingMasterNewId(masterModel);
                    IV.TVMID = (int)masterModel.TVMID;
                }
                else
                {
                    masterModel.CreatedBy = existingIV.CreatedBy;
                    masterModel.CreatedDate = existingIV.CreatedDate;
                    masterModel.CreatedIP = existingIV.CreatedIP;
                    masterModel.RowVersion = existingIV.RowVersion;
                    //masterModel.ApprovalStatusID = existingIV.ApprovalStatusID;
                    masterModel.ReferenceNo = existingIV.ReferenceNo;
                    masterModel.TVMDate = existingIV.TVMDate;
                    masterModel.ReferenceKeyword = existingIV.ReferenceKeyword;
                    masterModel.SetModified();
                }

                SetAuditFields(masterModel);


                if (IV.Attachments.IsNotNull() && IV.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(IV.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)IV.TVMID, "TaxationVettingMaster", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)IV.TVMID, "TaxationVettingMaster", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }


                TaxationVettingMasterRepo.Add(masterModel);
                if (!masterModel.IsDraft)
                {
                    if (masterModel.IsAdded || (existingIV.IsDraft && masterModel.IsModified))
                    {
                        string approvalTitle = $"{Util.TaxationVettingApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, TaxationVetting No:{masterModel.ReferenceNo}";
                        var obj = CreateApprovalProcessForLimit((int)masterModel.TVMID, Util.AutoTaxationVettingAppDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.TaxationVetting, masterModel.GrandTotal, "0");
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
                        await SendMailFromManagerBase(ApprovalProcessID, false, masterModel.TVMID, (int)Util.MailGroupSetup.TaxationVettingInitiatedMail, (int)Util.ApprovalType.TaxationVetting);
                }

            }
            await Task.CompletedTask;

            return (true, $"Taxation Vetting Submitted Successfully");
        }
        private void SetTaxationVettingMasterNewId(TaxationVettingMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("TaxationVettingMaster", AppContexts.User.CompanyID);
            master.TVMID = code.MaxNumber;
        }
        private void SetAttachmentNewId(Attachment attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
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
                        string filename = $"TaxationVetting-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "TaxationVetting\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

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
        private List<Attachment> RemoveAttachments(TaxationVettingMasterDto TVM)
        {
            if (TVM.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachment>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='TaxationVettingMaster' AND ReferenceID={TVM.TVMID}";
                var prevAttachment = TaxationVettingMasterRepo.GetDataDictCollection(attachmentSql);

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
                var removeList = attachemntList.Where(x => !TVM.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "TaxationVetting";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(instance1.ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.DivisionName + "-" + (AppContexts.User.DepartmentName).Replace(" ", ""));
                        System.IO.File.Delete(str + "\\" + data.FileName);

                    }

                }
                return removeList;
            }
            return null;
        }
        #endregion

        public async Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID)
        {
            return GetApprovalForwardingMembers(ApprovalProcessID).Result;
        }
        public async Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId)
        {
            return GetApprovalRejectedMembers(aprovalProcessId).Result;
        }
        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.TaxationVetting);
            return comments.Result;
        }
        public IEnumerable<Dictionary<string, object>> TaxationVettingApprovalFeedback(int TVMID)
        {
            string sql = $@" EXEC Accounts..spRPTTaxationVettingApprovalFeedback {TVMID}";
            var feedback = TaxationVettingMasterRepo.GetDataDictCollection(sql);
            return feedback;
        }
        public List<Attachments> GetAttachments(int TVMID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='TaxationVettingMaster' AND ReferenceID={TVMID}";
            var attachment = TaxationVettingMasterRepo.GetDataDictCollection(attachmentSql);
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
        public List<Attachments> GetAttachmentsInvoice(int invoiceid)
        {
            //string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='InvoiceMaster' AND ReferenceID={invoiceid}";

            string attachmentSql = $@"SELECT F.*,CASE when S.SystemVariableCode IS NULL then '' Else S.SystemVariableCode END AS DocumentType 
                                            FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload F
                                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable S ON F.ParentFUID=S.SystemVariableID
                                            where F.TableName='InvoiceMaster' AND F.ReferenceID={invoiceid}";
            var attachment = TaxationVettingMasterRepo.GetDataDictCollection(attachmentSql);
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
                    ParentFUID = (int)data["ParentFUID"],
                    DocumentType = data["DocumentType"].ToString(),
                    Description = data["Description"].ToString(),
                });
            }
            return attachemntList;
        }
        
        private string GenerateTaxationVettingMasterReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/TV/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{ GenerateSystemCode("TaxationVettingMasterRefNo", AppContexts.User.CompanyID).MaxNumber}";
            return format;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetsVDSListAsCombo()
        {

            string sql = @$"select VTDSID, SourceTypeID, SectionOrServiceCode, ServiceName, RatePercent, FinancialYearID from Accounts..VatTaxDeductionSource where SourceTypeID = 145";
            var listDict = VatTaxDeductionSourceRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);

            //var vdsList = VatTaxDeductionSourceRepo.Entities.Where(x => x.SourceTypeID == 145).ToList();
            //return vdsList.Select(x => new ComboModel { value = (int)x.VTDSID, label = x.SectionOrServiceCode }).ToList();
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetsTDSListAsCombo(int id)
        {

            string sql = @$"select VTDSID, SourceTypeID, SectionOrServiceCode, ServiceName, RatePercent, FinancialYearID from Accounts..VatTaxDeductionSource where SourceTypeID = {id}";
            var listDict = VatTaxDeductionSourceRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
            //var vdsList = VatTaxDeductionSourceRepo.Entities.Where(x => x.SourceTypeID == id).ToList();
            //return vdsList.Select(x => new ComboModel { value = (int)x.VTDSID, label = x.SectionOrServiceCode }).ToList();
        }
        public GridModel GetListForGridApproved(GridParameter parameters)
        {
            // "All","Pending Action","Action Taken"
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
                    filter = $@" AND TVM.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND TVM.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
            string sql = $@" SELECT 
                                DISTINCT
	                            TVM.*,
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
                                --,CASE WHEN ApprovalStatusID = 24 THEN CAST(1 as bit) WHEN EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( 353,ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
								,InventoryTypeID						
								,PO.ReferenceNo PONo
								,PR.ReferenceNo PRNo								
								,IT.SystemVariableCode InventoryTypeName
                                ,Concat(PO.ReferenceNo, '',PR.ReferenceNo) POPRNo
                                ,IM.IsAdvanceInvoice
                                ,IM.ReferenceNo InvoiceNo
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt
                                ,ISNULL(TVP.ReferenceNo,'') TVPNo
                          FROM 
                            SCM..InvoicePaymentMaster IPM
						    JOIN SCM..InvoicePaymentChild IPC ON IPM.IPaymentMasterID=IPC.IPaymentMasterID
							JOIN TaxationVettingMaster TVM ON IPC.TVMID=TVM.TVMID
							LEFT JOIN TaxationVettingPayment TVP ON TVM.TVMID = TVP.TVMID
                            LEFT JOIN SCM..InvoiceMaster IM ON TVM.InvoiceMasterID = IM.InvoiceMasterID
							LEFT JOIN SCM..PurchaseOrderMaster PO ON PO.POMasterID = TVM.POMasterID
							LEFT JOIN SCM..PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID
							LEFT JOIN Security..SystemVariable IT ON IT.SystemVariableID = PO.InventoryTypeID
                            LEFT JOIN Security..Users U ON U.UserID = TVM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = TVM.ApprovalStatusID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = TVM.TVMID AND AP.APTypeID = {(int)Util.ApprovalType.TaxationVetting}
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
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
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
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback  AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.TaxationVetting} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.TaxationVetting} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = TVM.TVMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = TVM.TVMID
                                    LEFT JOIN ( 
                                        SELECT * FROM Approval.dbo.functionJoinListProxyEmployeeAPSubmitDate({AppContexts.User.EmployeeID})                                   
									)APSubmitDate ON APSubmitDate.ApprovalProcessID = AP.ApprovalProcessID
									LEFT JOIN (
													SELECT 
													   MAX(CommentSubmitDate)  CommentSubmitDate,ApprovalProcessID 
													FROM 
														{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
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
									        WHERE AEF.APTypeID = {(int)Util.ApprovalType.TaxationVetting} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = TVM.TVMID
							WHERE TVM.ApprovalStatusID={(int)Util.ApprovalStatus.Approved} AND (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
                            ";
            var result = TaxationVettingMasterRepo.LoadGridModel(parameters, sql);
            return result;
        }

        public GridModel GetListForGrid(GridParameter parameters)
        {
            // "All","Pending Action","Action Taken"
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
                    filter = $@" AND TVM.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND TVM.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
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
            string sql = $@" SELECT 
                                DISTINCT
	                            TVM.*,
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
                                --,CASE WHEN ApprovalStatusID = 24 THEN CAST(1 as bit) WHEN EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( 353,ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
								,InventoryTypeID						
								,PO.ReferenceNo PONo
								,PR.ReferenceNo PRNo								
								,IT.SystemVariableCode InventoryTypeName
                                ,Concat(PO.ReferenceNo, '',PR.ReferenceNo) POPRNo
                                ,IM.IsAdvanceInvoice
                                ,VA.FullName+VA.EmployeeCode+VA.DepartmentName EmployeeWithDepartment
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt

	                            ,ISNULL(AP1.ApprovalProcessID, 0) PRApprovalProcessID
	                            ,ISNULL(AP2.ApprovalProcessID, 0) POApprovalProcessID
                          FROM TaxationVettingMaster TVM
                            LEFT JOIN SCM..InvoiceMaster IM ON TVM.InvoiceMasterID = IM.InvoiceMasterID
							LEFT JOIN SCM..PurchaseOrderMaster PO ON PO.POMasterID = TVM.POMasterID
							LEFT JOIN SCM..PurchaseRequisitionMaster PR ON PR.PRMasterID = PO.PRMasterID
							LEFT JOIN Security..SystemVariable IT ON IT.SystemVariableID = PO.InventoryTypeID
                            LEFT JOIN Security..Users U ON U.UserID = TVM.CreatedBy
                            LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = TVM.ApprovalStatusID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ReferenceID = TVM.TVMID AND AP.APTypeID = {(int)Util.ApprovalType.TaxationVetting}
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP1 ON AP1.ReferenceID =     PR.PRMasterID AND AP1.APTypeID = {(int)Util.ApprovalType.PR} 
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP2 ON AP2.ReferenceID =     PO.POMasterID AND AP2.APTypeID = {(int)Util.ApprovalType.PO} 
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
				                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
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
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback  AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.TaxationVetting} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
											LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.TaxationVetting} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = TVM.TVMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = TVM.TVMID
                                    LEFT JOIN ( 
                                        SELECT * FROM Approval.dbo.functionJoinListProxyEmployeeAPSubmitDate({AppContexts.User.EmployeeID})                                   
									)APSubmitDate ON APSubmitDate.ApprovalProcessID = AP.ApprovalProcessID
									LEFT JOIN (
													SELECT 
													   MAX(CommentSubmitDate)  CommentSubmitDate,ApprovalProcessID 
													FROM 
														{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo  
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
									        WHERE AEF.APTypeID = {(int)Util.ApprovalType.TaxationVetting} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = TVM.TVMID
							WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
                            ";
            var result = TaxationVettingMasterRepo.LoadGridModel(parameters, sql);
            return result;
        }
        public async Task<Dictionary<string, object>> GetTaxationVettingMaster(int tvmid)
        {
            string sql = $@"SELECT MR.*, VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName
                           
                            ,(SELECT Security.dbo.NumericToBDT(MR.GrandTotal)) AmountInWords
							
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID,

							--I.*,
                                I.ReferenceNo InvoiceNo,
                                I.InvoiceDate,
                                I.IsAdvanceInvoice,
                                VA.DepartmentID, 
                                VA.DepartmentName, 
                                VA.FullName AS EmployeeName, 
                                SV.SystemVariableCode AS ApprovalStatus, 
                                VA.ImagePath, 
                                VA.EmployeeCode, 
                                VA.DivisionID, VA.DivisionName,
                                W.WarehouseName DeliveryLocationName,
                                WorkMobile,
	                            PO.ReferenceNo,
	                            PO.PODate PODate,
	                            PO.DeliveryLocation,
	                            PO.SupplierID,
								S.SupplierName,
	                            PO.ReferenceNo PONo,
	                            WarehouseName Warehouse,
	                            PO.InventoryTypeID,
	                            IT.SystemVariableCode InventoryTypeName,
                                PO.CreatedDate POCreatedDate,
                                PO.POMasterID,
                                PO.GrandTotal,
                                PO.TotalVatAmount,
                                PO.TotalWithoutVatAmount,

								PR.PRMasterID,
								PR.ReferenceNo PRNo,
                                C.CurrencyName,
								ITC.SystemVariableCode InvoiceTypeName,
                                PT.SystemVariableCode PaymentTermsName,
						        PT.NumericValue NumberOfDueDay,
								TD.SystemVariableCode TDSMethodName,
								VDS.SectionOrServiceCode VDSCode,
								TDS.SectionOrServiceCode TDSCode,
								YN.SystemVariableCode YesNoName
	                            ,ISNULL(AP1.ApprovalProcessID, 0) PRApprovalProcessID
	                            ,ISNULL(AP2.ApprovalProcessID, 0) POApprovalProcessID
                        from TaxationVettingMaster MR
                        LEFT JOIN SCM..InvoiceMaster I ON I.InvoiceMasterID = MR.InvoiceMasterID
						LEFT JOIN SCM..PurchaseOrderMaster PO ON MR.POMasterID = PO.POMasterID
						LEFT JOIN SCM..PurchaseRequisitionMaster PR ON MR.PRMasterID = PR.PRMasterID
                        LEFT JOIN SCM..Warehouse W ON W.WarehouseID = PO.DeliveryLocation                           	
						LEFT JOIN SCM..Supplier S ON S.SupplierID = PO.SupplierID

						 LEFT JOIN Security..Currency C ON C.CurrencyID=I.CurrencyID
						 
							LEFT JOIN Security..SystemVariable ITC ON ITC.SystemVariableID = I.InvoiceTypeID AND ITC.EntityTypeID =29
                            LEFT JOIN  Security..SystemVariable PT ON PT.SystemVariableID=PO.PaymentTermsID AND  PT.EntityTypeID=28

						LEFT JOIN Security..SystemVariable IT ON IT.SystemVariableID = PO.InventoryTypeID AND IT.EntityTypeID =25
						LEFT JOIN Security..SystemVariable TD ON TD.SystemVariableID = MR.TDSMethodID
						LEFT JOIN Security..SystemVariable YN ON YN.SystemVariableID = MR.VATRebatableID
						LEFT JOIN VatTaxDeductionSource VDS ON VDS.VTDSID = MR.VDSRateID
						LEFT JOIN VatTaxDeductionSource TDS ON TDS.VTDSID = MR.TDSRateID

                        LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = MR.ApprovalStatusID
                        LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = MR.TVMID AND AP.APTypeID = {(int)Util.ApprovalType.TaxationVetting} 
                        LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP1 ON AP1.ReferenceID =     PR.PRMasterID AND AP1.APTypeID = {(int)Util.ApprovalType.PR} 
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP2 ON AP2.ReferenceID =     PO.POMasterID AND AP2.APTypeID = {(int)Util.ApprovalType.PO} 
                            LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.TaxationVetting}
										GROUP BY ReferenceID 

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.TaxationVetting} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID = MR.TVMID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID = MR.TVMID
                        WHERE MR.TVMID={tvmid}";
            var master = TaxationVettingMasterRepo.GetData(sql);
            return master;
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
            var comments = TaxationVettingMasterRepo.GetDataDictCollection(sql);
            return comments;
        }

        public async Task<List<Dictionary<string, object>>> GetInvoiceChildListOfDict(int InvoiceMasterID)
        {
            string sql = $@"SELECT  
                            POC.ItemID,
                            POC.Qty ReceivedQty,
                            POC.Rate,
                            POC.Amount,
					        VA.DepartmentID,
					        VA.DepartmentName,
					        VA.FullName AS EmployeeName,
					        SV.SystemVariableCode AS ApprovalStatus,
					        VA.ImagePath,
					        VA.EmployeeCode,
					        VA.DivisionID,
					        VA.DivisionName
					        ,I.ItemName, 
                            I.ItemCode, 
                            POC.Description AS ItemDescription,
                            Unit.UnitCode
                            ,POC.Qty AS POQty
							,POC.VatPercent
							,POC.VATAmount
							,POC.TotalAmountIncludingVat
                    FROM SCM..PurchaseOrderChild POC
                        LEFT JOIN SCM..PurchaseOrderMaster POM ON POM.POMasterID = POC.POMasterID
                        LEFT JOIN SCM..InvoiceMaster IM ON IM.POMasterID = POM.POMasterID
                        LEFT JOIN SCM..Item I ON I.ItemID=POC.ItemID
                        LEFT JOIN Security..Users U ON U.UserID = POC.CreatedBy
						LEFT JOIN SCM..PurchaseRequisitionChild PRC ON PRC.PRMasterID = POM.PRMasterID AND PRC.ItemID = POC.ItemID
                        LEFT JOIN Security..Unit  ON Unit.UnitID=POC.UOM
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = POM.ApprovalStatusID
                        WHERE IM.InvoiceMasterID={InvoiceMasterID}";
            var childs = TaxationVettingMasterRepo.GetDataDictCollection(sql);
            return childs.ToList();
        }

        public Task<List<MaterialReceiveDto>> MaterialReceiveMasterDetailsForReassessmentAndView(int POMasterID, int InvoiceMasterID)
        {
            //List<MaterialReceiveDto> master = new List<MaterialReceiveDto>();
            string query1 = $@"SELECT MR.MRID, MR.ReferenceNo, MR.MRDate, MR.ChalanNo, MR.ChalanDate, MR.TotalVatAmount, MR.TotalReceivedAmount, MR.TotalWithoutVatAmount
                            FROM SCM..MaterialReceive MR
                            LEFT JOIN SCM..InvoiceChild IC ON MR.MRID = IC.MRID
                            WHERE MR.ApprovalStatusID = 23 AND MR.POMasterID = {POMasterID}";

            var master = TaxationVettingMasterRepo.GetDataModelCollection<MaterialReceiveDto>(query1).ToList();
            if(master.Count > 0)
            {
                foreach(var item in master)
                {
                    string query2 = $@"SELECT MRC.MRID, MRC.MRCID, I.ItemCode + '-' + I.ItemName ItemName, MRC.ReceiveQty, Unit.UnitCode UnitName, POC.VatPercent POVatPercent, MRC.VatAmount, MRC.TotalAmount, MRC.TotalAmountIncludingVat, POC.Description
                    FROM SCM..MaterialReceiveChild MRC
						LEFT JOIN SCM..InvoiceChild IC ON IC.MRID = MRC.MRID
                        LEFT JOIN SCM..MaterialReceive MRM ON MRM.MRID = MRC.MRID
						--LEFT JOIN (	SELECT MRC1.ItemID,SUM(MRC1.ReceiveQty) ReceivedQty FROM SCM..MaterialReceiveChild MRC1
						--			JOIN SCM..MaterialReceive MR1 ON MRC1.MRID=MR1.MRID 
						--			LEFT JOIN SCM..PurchaseOrderChild POC ON POC.POMasterID = MR1.POMasterID
						--			LEFT JOIN Security..Unit U ON U.UnitID = POC.UOM
						--			WHERE MR1.ApprovalStatusID	<> 24 AND MR1.MRID!=7
						--			GROUP BY MRC1.ItemID) PO ON MRC.ItemID=PO.ItemID
                        LEFT JOIN SCM..Item I ON I.ItemID=MRC.ItemID
                        LEFT JOIN Security..Users U ON U.UserID = MRC.CreatedBy
						LEFT JOIN SCM..QCChild QCC ON QCC.QCCID=MRC.QCCID AND MRC.QCCID = QCC.QCCID
						LEFT JOIN SCM..PurchaseOrderChild POC ON POC.POMasterID = MRM.POMasterID AND POC.ItemID = MRC.ItemID AND POC.POCID = QCC.POCID
                        LEFT JOIN Security..Unit  ON Unit.UnitID=POC.UOM
                        LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = MRM.ApprovalStatusID
						where IC.InvoiceMasterID={InvoiceMasterID} AND MRM.MRID={item.MRID}
						";
                    item.MRItemDetails = TaxationVettingMasterRepo.GetDataModelCollection<MRItemDetails>(query2);
                }
            }

            return Task.Run(() =>
            {
                return master;
            });
        }


        public async Task<List<ComboModel>> GetInvoiceChildList(int InvoiceMasterID)
        {
            string sql = $@"select mr.MRID value, mr.ReferenceNo label 
                        from scm..InvoiceChild ic
                        left join scm..MaterialReceive mr on ic.MRID = mr.MRID
                        where ic.InvoiceMasterID = {InvoiceMasterID}";
            return TaxationVettingMasterRepo.GetDataModelCollection<ComboModel>(sql);
            
        }


        public Task<List<SCCMasterDto>> SccDetailsForReassessmentAndView(int POMasterID, int InvoiceMasterID)
        {
            //List<MaterialReceiveDto> master = new List<MaterialReceiveDto>();
            string query1 = $@"SELECT SCCM.SCCMID, SCCM.ReferenceNo, SCCM.InvoiceNoFromVendor, SCCM.InvoiceDateFromVendor, SCCC.VatAmount TotalVatAmount, SCCM.PaymentFixedOrPercentTotalAmount + SCCC.VatAmount TotalReceivedAmount, SCCM.PaymentFixedOrPercentTotalAmount TotalWithoutVatAmount , SCCM.CreatedDate SccDate
                            FROM SCM..SCCMaster SCCM
                            LEFT JOIN SCM..InvoiceChildSCC IC ON SCCM.SCCMID = IC.SCCMID
							LEFT JOIN SCM..SCCChild SCCC ON SCCC.SCCMID = SCCM.SCCMID
                            WHERE SCCM.ApprovalStatusID = 23 AND IC.InvoiceMasterID = {InvoiceMasterID}";

            var master = TaxationVettingMasterRepo.GetDataModelCollection<SCCMasterDto>(query1).ToList();
            if (master.Count > 0)
            {
                foreach (var item in master)
                {
                    string query2 = $@"SELECT SCC.SCCMID, SCC.SCCCID, I.ItemCode + '-' + I.ItemName ItemName, SCC.ReceivedQty, Unit.UnitCode UnitName, POC.VatPercent POVatPercent, SCC.VatAmount, SCC.TotalAmount, SCC.TotalAmountIncludingVat, POC.Description
                    FROM SCM..SCCChild SCC
						LEFT JOIN SCM..InvoiceChildSCC IC ON IC.SCCMID = SCC.SCCMID
                        LEFT JOIN SCM..SCCMaster SCCM ON SCCM.SCCMID = SCC.SCCMID
                        LEFT JOIN SCM..Item I ON I.ItemID=SCC.ItemID
                       -- LEFT JOIN Security..Users U ON U.UserID = MRC.CreatedBy
						--LEFT JOIN SCM..QCChild QCC ON QCC.QCCID=MRC.QCCID AND MRC.QCCID = QCC.QCCID
						LEFT JOIN SCM..PurchaseOrderChild POC ON POC.POMasterID = SCCM.POMasterID AND POC.ItemID = SCC.ItemID AND POC.POCID = SCC.POCID
                        LEFT JOIN Security..Unit  ON Unit.UnitID=POC.UOM
                       -- LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                        LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = SCCM.ApprovalStatusID
						where IC.InvoiceMasterID={InvoiceMasterID} AND SCCM.SCCMID={item.SCCMID}
						";
                    item.SCCItemDetails = TaxationVettingMasterRepo.GetDataModelCollection<SCCItemDetails>(query2).ToList();
                }
            }

            return Task.Run(() =>
            {
                return master;
            });
        }


    }
}
