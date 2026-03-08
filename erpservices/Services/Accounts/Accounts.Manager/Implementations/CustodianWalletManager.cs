using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using Accounts.Manager.Interfaces;
using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Util;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Accounts.Manager.Implementations
{
    public class CustodianWalletManager : ManagerBase, ICustodianWalletManager
    {
        private readonly IRepository<CustodianWallet> CustodianWalletRepo;
        public CustodianWalletManager(IRepository<CustodianWallet> custodianWalletRepo)
        {
            CustodianWalletRepo = custodianWalletRepo;
        }

        public async Task<CustodianWalletDto> Get(int id)
        {
            string sql = @$"SELECT CWID, WalletName, AL.EmployeeID, ReimbursementThreshold, OpeningBalance, CurrentBalance,Limit,AL.CreatedDate,AL.CreatedBy, VA.FullName EmployeeName, VA.EmployeeCode, IsActive
                            ,(SELECT DivisionName label, DivisionID value from {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..Division
	                            WHERE DivisionID IN (Select * from {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..fnReturnStringArray(AL.DivisionIDs,','))
                            FOR JSON PATH) DivisionIDsStr
                            , (SELECT DepartmentName label, DepartmentID value, CONVERT(INT, DivisionID) AS DivisionID from {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..Department
	                            WHERE DepartmentID IN (Select * from {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..fnReturnStringArray(AL.DepartmentIDs,','))
                            FOR JSON PATH) DepartmentIDsStr
                            FROM {AppContexts.GetDatabaseName(ConnectionName.AccountsContext)}..CustodianWallet AL
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.EmployeeID = AL.EmployeeID
                            WHERE CWID={id}";

            var wallet = Task.Run(() => CustodianWalletRepo.GetModelData<CustodianWalletDto>(sql));

            return await wallet;
        }
        public async Task<List<PettyCashTransactionHistory>> TransactionDetailsByCWID(int id)
        {
            string sql = $@"SELECT * FROM PettyCashTransactionHistory
                        WHERE CustodianID={id}";
            var transactionDetails = CustodianWalletRepo.GetDataModelCollection<PettyCashTransactionHistory>(sql);
            return transactionDetails;
        }
        public List<Attachments> GetAttachments(int CWID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='CustodianWallet' AND ReferenceID={CWID}";
            var attachment = CustodianWalletRepo.GetDataDictCollection(attachmentSql);
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

        private void SetMasterNewId(CustodianWallet wallet)
        {
            if (!wallet.IsAdded) return;
            var code = GenerateSystemCode("CustodianWallet", AppContexts.User.CompanyID);
            wallet.CWID = code.MaxNumber;
        }
        private void SetAttachmentNewId(Attachments attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
        }

        public async Task<(bool, string)> Save(CustodianWalletDto wallets)
        {

            string sql = $@"SELECT *
                                    FROM CustodianWallet
                                    WHERE CWID <> {wallets.CWID}
                                    AND (
                                        EXISTS (SELECT 1 FROM STRING_SPLIT('{wallets.DivisionIDs}', ',') WHERE CHARINDEX(value, DivisionIDs) > 0)
                                        AND EXISTS (SELECT 1 FROM STRING_SPLIT('{wallets.DepartmentIDs}', ',') WHERE CHARINDEX(value, DepartmentIDs) > 0))";
            var isExist = CustodianWalletRepo.GetModelData<CustodianWalletDto>(sql);
            if (isExist.CWID.IsNotZero())
            {
                return (false, $"Sorry, Already Exist!");
            }

            var existingCustodianWalle = CustodianWalletRepo.Entities.SingleOrDefault(x => x.CWID == wallets.CWID);

            //string updatedNewDivisionIDs = wallets.DivisionIDs;
            //if (wallets.CWID > 0)
            //{
            //    int[] existingValues = existingCustodianWalle.DivisionIDs.Split(',').Select(int.Parse).ToArray();
            //    int[] newValues = wallets.DivisionIDs.Split(',').Select(int.Parse).ToArray();
            //    newValues = newValues.Except(existingValues).ToArray();
            //    updatedNewDivisionIDs = string.Join(",", newValues);
            //}



            //var sql = $@"WITH DivisionsCTE AS (
            //                SELECT
            //                    DivisionIDs
            //                FROM Accounts..CustodianWallet WHERE CWID <> {wallets.CWID}
            //            )
            //            SELECT 
            //                CASE WHEN EXISTS (
            //                    SELECT 1
            //                    FROM (
            //                        SELECT STRING_AGG(value, ',') AS CommonDivisions
            //                        FROM (
            //                            SELECT DISTINCT value
            //                            FROM DivisionsCTE
            //                            CROSS APPLY STRING_SPLIT(DivisionIDs, ',')
            //                        ) AS SplitValues
            //                        WHERE value IS NOT NULL
            //                    ) AS AggregatedValues
            //                    WHERE EXISTS (
            //                        SELECT value
            //                        FROM STRING_SPLIT('{updatedNewDivisionIDs}', ',')
            //                        WHERE CHARINDEX(value, AggregatedValues.CommonDivisions) > 0
            //                    )
            //                ) THEN 1 ELSE 0 END AS IsExist";

            //var data = CustodianWalletRepo.GetData(sql).FirstOrDefault();
            //bool isExist = Convert.ToBoolean(data.Value);
            //if (isExist)
            //{
            //    return (false, "You can't add same division in multiple wallet.");
            //}




            foreach (var item in wallets.Attachments)
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

            var removeList = RemoveAttachments(wallets, wallets.CWID);




            var walletModel = new CustodianWallet
            {
                WalletName = wallets.WalletName,
                EmployeeID = wallets.EmployeeID,
                ReimbursementThreshold = wallets.ReimbursementThreshold,
                OpeningBalance = wallets.OpeningBalance,
                Limit = wallets.Limit,
                DivisionIDs = wallets.DivisionIDs,
                DepartmentIDs = wallets.DepartmentIDs,
                IsActive = wallets.IsActive,
            };


            using (var unitOfWork = new UnitOfWork())
            {
                if (wallets.CWID.IsZero() && existingCustodianWalle.IsNull())
                {
                    walletModel.SetAdded();
                    SetMasterNewId(walletModel);
                    wallets.CWID = (int)walletModel.CWID;
                    walletModel.CurrentBalance = wallets.OpeningBalance;
                }
                else
                {
                    walletModel.CWID = existingCustodianWalle.CWID;
                    walletModel.CurrentBalance = existingCustodianWalle.CurrentBalance;
                    walletModel.CreatedBy = existingCustodianWalle.CreatedBy;
                    walletModel.CreatedDate = existingCustodianWalle.CreatedDate;
                    walletModel.CreatedIP = existingCustodianWalle.CreatedIP;
                    walletModel.RowVersion = existingCustodianWalle.RowVersion;
                    walletModel.SetModified();
                }

                SetAuditFields(walletModel);


                if (wallets.Attachments.IsNotNull() && wallets.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(wallets.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, Convert.ToInt32(wallets.CWID), "CustodianWallet", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, Convert.ToInt32(wallets.CWID), "CustodianWallet", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }



                CustodianWalletRepo.Add(walletModel);

                unitOfWork.CommitChangesWithAudit();

                //BasicMail((int)Util.MailGroupSetup.LeaveEncashmentWindowMail, toMail, false, null, null, mailData);


            }

            await Task.CompletedTask;

            return (true, $"Successfully");

        }

        public GridModel GetAll(GridParameter parameters)
        {
            string sql = @$"SELECT AL.CWID, WalletName, AL.EmployeeID, ReimbursementThreshold, OpeningBalance, CurrentBalance,Limit,AL.CreatedDate,AL.CreatedBy, VA.FullName EmployeeName, VA.EmployeeCode, IsActive
                            ,(SELECT DivisionName label, DivisionID value from {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..Division
	                            WHERE DivisionID IN (Select * from {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..fnReturnStringArray(AL.DivisionIDs,','))
                            FOR JSON PATH) DivisionIDsStr
                            , (SELECT DepartmentName label, DepartmentID value from {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..Department
	                            WHERE DepartmentID IN (Select * from {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..fnReturnStringArray(AL.DepartmentIDs,','))
                            FOR JSON PATH) DepartmentIDsStr
                            ,(CASE WHEN A.CWID> 0 THEN 0 ELSE 1 END) AS IsRemovable
                            FROM {AppContexts.GetDatabaseName(ConnectionName.AccountsContext)}..CustodianWallet AL
                            LEFT JOIN { AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VA ON VA.EmployeeID = AL.EmployeeID
                            LEFT JOIN (
							select CWID from Accounts..PettyCashExpenseMaster
							UNION
							select CWID from Accounts..PettyCashAdvanceMaster) A ON A.CWID=AL.CWID";

            var result = CustodianWalletRepo.LoadGridModel(parameters, sql);
            return result;
        }

        public bool DeleteWallet(int CWID)
        {
            
            var sql = @$"SELECT COUNT(*) COUNT FROM(
                        SELECT CWID FROM Accounts..PettyCashExpenseMaster
                        UNION
                        SELECT CWID FROM Accounts..PettyCashAdvanceMaster) A
                        WHERE A.CWID= {CWID}";

            var exist = CustodianWalletRepo.GetData(sql);
            int canDelete = Convert.ToInt32(exist["COUNT"]);
            if (canDelete > 0 || CWID == 0) { return false; }

            using var unitOfWork = new UnitOfWork();
            var custodian = CustodianWalletRepo.Entities.SingleOrDefault(x => x.CWID == CWID);
            custodian.SetDeleted();
            CustodianWalletRepo.Add(custodian);

            unitOfWork.CommitChangesWithAudit();
            return true;
        }


        private List<Attachments> AddAttachments(List<Attachments> list)
        {
            if (list.Count > 0)
            {
                var attachemntList = new List<Attachments>();
                int sl = 0;
                foreach (var attachment in list)
                {
                    if (attachment.AttachedFile.IsNotNull())
                    {
                        string filename = $"CW-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "CustodianWallet\\" + AppContexts.User.PersonID + " - " + (AppContexts.User.FullName).Replace(" ", ""));

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

        private List<Attachments> RemoveAttachments(CustodianWalletDto wallet, long referenceId)
        {
            if (wallet.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachments>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='CustodianWallet' AND ReferenceID={referenceId}";
                var prevAttachment = CustodianWalletRepo.GetDataDictCollection(attachmentSql);

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
                var removeList = attachemntList.Where(x => !wallet.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "CustodianWallet";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName + "\\" + AppContexts.User.PersonID + " - " + (AppContexts.User.FullName).Replace(" ", ""));
                        System.IO.File.Delete(str + "\\" + data.FileName);

                    }

                }
                return removeList;
            }
            return null;
        }


    }
}
