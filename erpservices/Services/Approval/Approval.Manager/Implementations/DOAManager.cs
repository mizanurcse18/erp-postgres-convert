using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Approval.DAL.Entities;
using Approval.Manager.Dto;
using Approval.Manager.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Core.Util;

namespace Approval.Manager.Implementations
{
    public class DOAManager : ManagerBase, IDOAManager
    {

        private readonly IRepository<DOAMaster> DOARepo;
        private readonly IRepository<DOAApprovalPanelEmployee> DOAApprovalPanelEmployeeRepo;
        public DOAManager(IRepository<DOAMaster> _DOAMasterRepo, IRepository<DOAApprovalPanelEmployee> _DOAApprovalPanelEmployeeRepo)
        {
            DOARepo = _DOAMasterRepo;
            DOAApprovalPanelEmployeeRepo = _DOAApprovalPanelEmployeeRepo;
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
                        string filename = $"DOA-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "DOA\\" + AppContexts.User.DivisionName.Trim() + "-" + (AppContexts.User.DepartmentName.Trim()).Replace(" ", ""));

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
        private List<Attachment> RemoveAttachments(DOADto DOA)
        {
            if (DOA.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachment>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='DOAMaster' AND ReferenceID={DOA.DOAMasterID}";
                var prevAttachment = DOARepo.GetDataDictCollection(attachmentSql);

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
                var removeList = attachemntList.Where(x => !DOA.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "DOA";
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

        

        private List<DOAApprovalPanelEmployee> GenerateDOAApprovalPanelEmployee(DOADto DOA)
        {
            var existingDOAChild =DOAApprovalPanelEmployeeRepo.Entities.Where(x => x.DOAMasterID == DOA.DOAMasterID).ToList();
            var childModel = new List<DOAApprovalPanelEmployee>();
            if (DOA.DOAMasterID == 0)
            {
                if (DOA.DOAItemDetails.IsNotNull())
                {
                    int count = 0;
                    DOA.DOAItemDetails.ForEach(x =>
                    {
                        count += 1;
                        if (x.MultipleAPPanelDetails.IsNotNull())
                        {
                            x.MultipleAPPanelDetails.ForEach(ap =>
                            {
                                if (x.MultipleEmployeeDetails.IsNotNull())
                                {
                                    x.MultipleEmployeeDetails.ForEach(em =>
                                    {
                                        childModel.Add(new DOAApprovalPanelEmployee
                                        {
                                            DOAApprovalPanelEmployeeID = x.DOAApprovalPanelEmployeeID,
                                            DOAMasterID = DOA.DOAMasterID,
                                            AssigneeEmployeeID = em.value,
                                            TypeID = x.DOAType.value,
                                            APPanelID = ap.value,
                                            GroupID = count
                                        });
                                    });
                                }
                            });
                        }


                    });

                    childModel.ForEach(x =>
                    {
                        if (existingDOAChild.Count > 0 && x.DOAApprovalPanelEmployeeID > 0)
                        {
                            var existingModelData = existingDOAChild.FirstOrDefault(y => y.DOAApprovalPanelEmployeeID == x.DOAApprovalPanelEmployeeID);
                            x.CreatedBy = existingModelData.CreatedBy;
                            x.CreatedDate = existingModelData.CreatedDate;
                            x.CreatedIP = existingModelData.CreatedIP;
                            x.RowVersion = existingModelData.RowVersion;
                            x.SetModified();
                        }
                        else
                        {
                            x.DOAMasterID = DOA.DOAMasterID;
                            x.SetAdded();
                            SetDOAApprovalPanelEmployeeNewId(x);
                        }
                    });

                    var willDeleted = existingDOAChild.Where(x => !childModel.Select(y => y.DOAApprovalPanelEmployeeID).Contains(x.DOAApprovalPanelEmployeeID)).ToList();
                    willDeleted.ForEach(x =>
                    {
                        x.SetDeleted();
                        childModel.Add(x);
                    });
                }
            }

            return childModel;
        }

        public async Task<(bool, string)> SaveChanges(DOADto DOA)
        {
            var existingDOA = DOARepo.Entities.Where(x => x.DOAMasterID == DOA.DOAMasterID).SingleOrDefault();


            foreach (var item in DOA.Attachments)
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


            var removeList = RemoveAttachments(DOA);


            var masterModel = new DOAMaster
            {
                DOAMasterID = DOA.DOAMasterID,
                EmployeeID = DOA.IsHR == 1 ? DOA.Employee.value : AppContexts.User.EmployeeID.Value,
                StartDate = DOA.StartDate,
                EndDate = DOA.EndDate,
                StatusID = DOA.Status.value,
                Remarks = DOA.Remarks
            };
            

            using (var unitOfWork = new UnitOfWork())
            {
                if (DOA.DOAMasterID.IsZero() && existingDOA.IsNull())
                {
                    masterModel.SetAdded();
                    SetDOANewId(masterModel);
                    DOA.DOAMasterID = (int)masterModel.DOAMasterID;

                   
                }
                else
                {
                    masterModel.CreatedBy = existingDOA.CreatedBy;
                    masterModel.CreatedDate = existingDOA.CreatedDate;
                    masterModel.CreatedIP = existingDOA.CreatedIP;
                    masterModel.RowVersion = existingDOA.RowVersion;
                    masterModel.DOAMasterID = DOA.DOAMasterID;
                    masterModel.StatusID = DOA.Status.value;
                    masterModel.SetModified();
                }
                var childModel = GenerateDOAApprovalPanelEmployee(DOA);
               


                SetAuditFields(masterModel);
                SetAuditFields(childModel);

                if (DOA.Attachments.IsNotNull() && DOA.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(DOA.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)DOA.DOAMasterID, "DOAMaster", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, (int)DOA.DOAMasterID, "DOAMaster", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }

               

                DOARepo.Add(masterModel);
                DOAApprovalPanelEmployeeRepo.AddRange(childModel);


                unitOfWork.CommitChangesWithAudit();

                
            }
            await Task.CompletedTask;

            return (true, $"DOA Submitted Successfully"); ;
        }

        private void SetDOANewId(DOAMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("DOAMaster", AppContexts.User.CompanyID);
            master.DOAMasterID = code.MaxNumber;
        }


        private void SetDOAApprovalPanelEmployeeNewId(DOAApprovalPanelEmployee child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("DOAApprovalPanelEmployee", AppContexts.User.CompanyID);
            child.DOAApprovalPanelEmployeeID = code.MaxNumber;
        }

        private string GenerateDOAReference()
        {
            string format = @$"{AppContexts.User.CompanyShortCode}/DOA/{AppContexts.User.DivisionName.Substring(0, 3).ToUpper()}/{ GenerateSystemCode("DOAMasterRefNo", AppContexts.User.CompanyID).SystemCode}";
            return format;
        }
        
        
        //DOA Approved List

        public GridModel GetDOAList(GridParameter parameters)
        {
            string filter = parameters.AdditionalFilterData == "hr" ? "" : $@" WHERE M.EmployeeID={AppContexts.User.EmployeeID}";

            string sql = $@"SELECT M.DOAMasterID
                                ,M.EmployeeID
                                ,M.StartDate
                                ,M.EndDate
                                ,M.StatusID
                                ,M.Remarks
								,M.CreatedDate
								,SV.SystemVariableCode DOAStatusName
								,VE.FullName EmployeeName

                            FROM DOAMaster M
							LEFT JOIN HRMS..ViewALLEmployee VE ON M.EmployeeID = VE.EmployeeID
							LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = M.StatusID {filter}
                            ";
            //WHERE (VA.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
            var result = DOARepo.LoadGridModelOptimized(parameters, sql);
            return result;
        }

        public async Task<DOAMasterDto> GetDOAMaster(int DOAMasterID)
        {
            string sql = $@"SELECT M.DOAMasterID
                                ,M.EmployeeID
                                ,M.StartDate
                                ,M.EndDate
                                ,M.StatusID
                                ,M.Remarks
								,M.CreatedDate
								,SV.SystemVariableCode DOAStatusName
								,VE.FullName EmployeeName

                            FROM DOAMaster M
							LEFT JOIN HRMS..ViewALLEmployee VE ON M.EmployeeID = VE.EmployeeID
							LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = M.StatusID
                        WHERE M.DOAMasterID={DOAMasterID}";
            var master = DOARepo.GetModelData<DOAMasterDto>(sql);
            return master;
        }

        public async Task<List<DOAApprovalPanelEmployeeDto>> GetDOAApprovalPanelEmployee(int DOAMasterID)
        {
            string sql = $@"SELECT  DOAC.*,
                    M.DOAMasterID,
                    M.EmployeeID,
                    M.StartDate,
                    M.EndDate,
                    M.StatusID,
                    M.Remarks,
					AP.Name APPanelName,
					VE.EmployeeCode + ' - ' + VE.FullName EmployeeName,
					SV.SystemVariableCode DOATypeName
					        
                    FROM DOAApprovalPanelEmployee DOAC
                        LEFT JOIN DOAMaster M ON M.DOAMasterID = DOAC.DOAMasterID
						LEFT JOIN Approval..ApprovalPanel AP ON DOAC.APPanelID = AP.APPanelID
						LEFT JOIN HRMS..ViewALLEmployee VE ON DOAC.AssigneeEmployeeID = VE.EmployeeID
						LEFT JOIN Security..SystemVariable SV ON DOAC.TypeID=SV.SystemVariableID
                        
                        WHERE DOAC.DOAMasterID={DOAMasterID}";
            var childs = DOAApprovalPanelEmployeeRepo.GetDataModelCollection<DOAApprovalPanelEmployeeDto>(sql);

            List<DOAApprovalPanelEmployeeDto> list = new List<DOAApprovalPanelEmployeeDto>();
            DOAApprovalPanelEmployeeDto doaObj = new DOAApprovalPanelEmployeeDto();

            list = childs
             .GroupBy(e => e.GroupID)
             .Select(g1 => new DOAApprovalPanelEmployeeDto
             {
                 GroupID = g1.Key,
                 //List = g1.ToList(),
                 DOAType = g1.ToList().Select(x => new ComboModel { value = x.TypeID, label = x.DOATypeName }).FirstOrDefault(),
                 MultipleAPPanelDetails = g1.Select(e => new ComboModel { value = e.APPanelID, label = e.APPanelName }).GroupBy(x => new { x.value, x.label })
                    .Select(y => y.FirstOrDefault()).ToList().ToList(),
                 MultipleEmployeeDetails = g1.Select(e => new ComboModel { value = (int)e.AssigneeEmployeeID, label = e.EmployeeName }).Distinct().ToList()
             })
             .ToList();


            return list;
        }


        public List<Attachments> GetAttachments(int DOAMasterID)
        {
            string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='DOAMaster' AND ReferenceID={DOAMasterID}";
            var attachment = DOARepo.GetDataDictCollection(attachmentSql);
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

    }



}
