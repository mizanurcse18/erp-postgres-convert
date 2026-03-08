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
using Security.DAL;
using Security.DAL.Entities;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Security.Manager
{
    public class TutorialManager : ManagerBase, ITutorialManager
    {
        private readonly IRepository<TutorialMaster> TutorialRepo;
        private readonly IRepository<FileUpload> FileUploadRepo;
        //readonly IModelAdapter Adapter;
        public TutorialManager(IRepository<TutorialMaster> tutorialRepo, IRepository<FileUpload> fileUploadRepo
            )
        {
            TutorialRepo = tutorialRepo;
            FileUploadRepo = fileUploadRepo;
        }

        public async Task Delete(int tutorialid)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var master = TutorialRepo.Entities.Where(x => x.TMID == tutorialid).FirstOrDefault();
                var files = FileUploadRepo.Entities.Where(x => x.ReferenceID == tutorialid).ToList();
                files.ForEach(x => x.SetDeleted());

                master.SetDeleted();
                TutorialRepo.Add(master);
                FileUploadRepo.AddRange(files);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }

        public async Task<List<TutorialDto>> GetTutorialsForList()
        {

            string sqlMaster = $@"SELECT TM.*, hr.DepartmentName,
                        CASE WHEN TM.TutorialTypeID=1 THEN 'video' WHEN TM.TutorialTypeID=2 THEN 'image' WHEN TM.TutorialTypeID=3 THEN 'pdf' ELSE 'excel' END AS TutorialType
                        FROM TutorialMaster TM
						LEFT JOIN HRMS..Department hr ON TM.DepartmentID = hr.DepartmentID  
                        ORDER BY TM.TMID DESC";
            //INNER JOIN FileUpload FU ON TM.TMID = FU.ReferenceID AND FU.TableName='TutorialMaster'";
            string sqlFile = $@"SELECT FU.* 
                            FROM TutorialMaster TM
                            INNER JOIN FileUpload FU ON TM.TMID = FU.ReferenceID AND FU.TableName='TutorialMaster'";

            List<TutorialDto> master = TutorialRepo.GetDataModelCollection<TutorialDto>(sqlMaster);
            List<FileUpload> files = FileUploadRepo.GetDataModelCollection<FileUpload>(sqlFile);
            var attachemntList = new List<Attachments>();

            foreach (var data in files)
            {
                attachemntList.Add(new Attachments
                {
                    FUID = data.FUID,
                    AID = data.FUID.ToString(),
                    FilePath = data.FilePath.ToString(),
                    OriginalName = data.OriginalName.ToString() + data.FileType.ToString(),
                    FileName = data.FileName.ToString(),
                    Type = data.FileType.ToString(),
                    Size = Convert.ToDecimal(data.SizeInKB),
                    Description = data.Description.ToString(),
                    ReferenceId = data.ReferenceID
                });
            }
            var customList = master.Select(chld => new TutorialDto()
            {
                TMID = chld.TMID,
                URL = chld.URL,
                VideoID = chld.VideoID,
                Title = chld.Title,
                Color = chld.Color,
                TutorialType = chld.TutorialType,
                TutorialTypeID = chld.TutorialTypeID,
                Attachments = attachemntList.Where(x => x.ReferenceId == chld.TMID).ToList(),
                DepartmentId = chld.DepartmentId,
                DepartmentName = chld.DepartmentName
            }).ToList();
            //var customList = master.GroupBy(c => new { c.TMID, c.URL, c.VideoID, c.Title, c.Color, c.TutorialTypeID, c.TutorialType })
            //  .Select(chld => new TutorialDto()
            //  {
            //      TMID = chld.Key.TMID,
            //      URL = chld.Key.URL,
            //      VideoID = chld.Key.VideoID,
            //      Title = chld.Key.Title,
            //      Color = chld.Key.Color,
            //      TutorialType = chld.Key.TutorialType,
            //      TutorialTypeID = chld.Key.TutorialTypeID,
            //      Attachments = attachemntList.Where(x=>x.ReferenceId == chld.Key.TMID).ToList()
            //  }).ToList();

            await Task.CompletedTask;
            return customList;
        }


        public List<object> GetTutorial(int tutorialid)
        {

            string sqlMaster = $@"SELECT TM.*, hr.DepartmentName ,
                            CASE WHEN TM.TutorialTypeID=1 THEN 'video' WHEN TM.TutorialTypeID=2 THEN 'image' WHEN TM.TutorialTypeID=4 THEN 'excel' ELSE 'pdf' END AS TutorialType
                            FROM TutorialMaster 
                            TM
							LEFT JOIN HRMS..Department hr 
							ON TM.DepartmentID =hr.DepartmentID
							WHERE TM.TMID={tutorialid} ";
            string sqlFile = $@"SELECT FU.* 
                            FROM TutorialMaster TM
                            INNER JOIN FileUpload FU ON TM.TMID = FU.ReferenceID WHERE TM.TMID={tutorialid}";

            var master = TutorialRepo.GetData(sqlMaster);
            var files = TutorialRepo.GetDataDictCollection(sqlFile);
            var attachemntList = new List<Attachments>();

            foreach (var data in files)
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
            var obj = new List<object>{

                master,
                attachemntList

            };
            return obj;
        }
        private void SetNewId(TutorialDto tut)
        {
            if (!tut.IsAdded) return;
            var code = GenerateSystemCode("TutorialMaster", AppContexts.User.CompanyID);
            tut.TMID = code.MaxNumber;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetTutorials()
        {
            string sql = $@"SELECT TM.*, hr.DepartmentName,
                        CASE WHEN TM.TutorialTypeID=1 THEN 'video' WHEN TM.TutorialTypeID=2 THEN 'image' WHEN TM.TutorialTypeID=3 THEN 'pdf' ELSE 'excel' END AS TutorialType
                        FROM TutorialMaster TM
						LEFT JOIN HRMS..Department hr ON TM.DepartmentID = hr.DepartmentID 
                        ORDER BY TM.TMID DESC";
            var listDict = TutorialRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }
        private List<Attachments> RemoveAttachments(TutorialDto master)
        {
            if (master.Attachments.Count > 0)
            {
                var attachemntList = new List<Attachments>();
                string attachmentSql = $@"SELECT * FROM  {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..FileUpload where TableName='TutorialMaster' AND ReferenceID={master.TMID}";
                var prevAttachment = TutorialRepo.GetDataDictCollection(attachmentSql);

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
                var removeList = attachemntList.Where(x => !master.Attachments.Where(z => z.FUID != 0).Select(y => y.FUID).Contains(x.FUID)).ToList();

                if (removeList.Count > 0)
                {
                    foreach (var data in removeList)
                    {
                        string attachmentFolder = "upload\\attachments";
                        string folderName = "Tutorial";
                        IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                        string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + attachmentFolder + "\\" + folderName);
                        System.IO.File.Delete(str + "\\" + data.FileName);

                    }

                }
                return removeList;
            }
            return null;
        }
        public async Task<TutorialDto> SaveChanges(TutorialDto master)
        {
            var existMaster = TutorialRepo.Entities.SingleOrDefault(x => x.TMID == master.TMID).MapTo<TutorialDto>();
            var removeList = RemoveAttachments(master);
            //var masterModel = new TutorialMaster
            //{
            //    TutorialTypeID = master.TutorialTypeID,
            //    FileName = master.FileName,
            //    FileType = master.FileType,
            //    OriginalName = master.OriginalName,
            //    URL = master.URL,
            //    TableName = master.TableName,
            //    VideoID = master.VideoID,
            //    Color = master.Color,
            //};
            using (var unitOfWork = new UnitOfWork())
            {
                if (
                    master.TMID.IsZero() || master.IsAdded)
                {
                    master.SetAdded();
                    SetNewId(master);
                }
                else
                {
                    master.CreatedBy = existMaster.CreatedBy;
                    master.CreatedDate = existMaster.CreatedDate;
                    master.CreatedIP = existMaster.CreatedIP;
                    master.RowVersion = existMaster.RowVersion;
                    master.SetModified();
                }

                var masterEnt = master.MapTo<TutorialMaster>();

                //Set Audti Fields Data
                SetAuditFields(masterEnt);

                if (master.Attachments.IsNotNull() && master.Attachments.Count > 0)
                {

                    var attachmentList = AddAttachments(master.Attachments.Where(x => x.ID == 0).ToList());

                    //For Add new File
                    if (attachmentList.IsNotNull() && attachmentList.Count > 0)
                    {
                        foreach (var attachemnt in attachmentList)
                        {
                            SetAttachmentNewId(attachemnt);
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, master.TMID, "TutorialMaster", false, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                    //For Remove Attachment                    
                    if (removeList.IsNotNull() && removeList.Count > 0)
                    {
                        foreach (var attachemnt in removeList)
                        {
                            SaveSingleAttachment(attachemnt.FUID, attachemnt.FilePath, attachemnt.FileName, attachemnt.Type, attachemnt.OriginalName, master.TMID, "TutorialMaster", true, attachemnt.Size, 0, false, attachemnt.Description ?? "");
                        }
                    }
                }

                TutorialRepo.Add(masterEnt);


                unitOfWork.CommitChangesWithAudit();

                //master = masterEnt.MapTo<TutorialDto>();
                //masterEnt.MapToAuditFields(master);
            }
            await Task.CompletedTask;

            return master;
        }

        private void SetAttachmentNewId(Attachments attachment)
        {
            var code = GenerateSystemCode("FileUpload", AppContexts.User.CompanyID);
            attachment.FUID = code.MaxNumber;
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
                        string filename = $"Tutorial-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(attachment.OriginalName)}";
                        var fileByte = UploadUtil.Base64ToByteArray(attachment.AttachedFile);
                        var filePath = UploadUtil.SaveAttachmentInDisk(fileByte, filename, "Tutorial");

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

        //private List<PersonImage> AddImages(TutorialDto dto)
        //{
        //    var personImageList = new List<PersonImage>();
        //    var willAdded = dto.TutorialImage.Where(x => x.PIID == 0).ToList();
        //    if (willAdded.Count > 0)
        //    {
        //        int sl = 1;
        //        foreach (var img in willAdded)
        //        {
        //            if (img.ImageFile.IsNotNull())
        //            {
        //                string imagename = $"{(img.IsSignature ? "Signature" : "Profile")}-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(img.ImageName)}";
        //                var imgByte = UploadUtil.Base64ToByteArray(img.ImageFile);
        //                var imagePath = UploadUtil.SaveImageInDisk(imgByte, imagename, "Person\\" + personSaveModel.MasterModel.PersonID + " - " + (personSaveModel.MasterModel.FirstName + personSaveModel.MasterModel.LastName).Replace(" ", ""));
        //                personImageList.Add(new PersonImage
        //                {
        //                    PersonID = personSaveModel.MasterModel.PersonID,
        //                    ImagePath = imagePath,
        //                    ImageOriginalName = img.ImageName,
        //                    IsSignature = img.IsSignature,
        //                    ImageName = imagename,
        //                    IsFavorite = img.IsFavorite,
        //                    ImageType = Path.GetExtension(img.ImageName),
        //                });

        //                sl++;
        //            }
        //        }
        //        return personImageList;
        //    }
        //    return personImageList;
        //}


        public async Task<IEnumerable<Dictionary<string, object>>> GetRevenuesForList()
        {
            string sql = $@"SELECT * FROM  FileUpload WHERE TableName='RevenueReport'";
            var listDict = TutorialRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }
    }
}
