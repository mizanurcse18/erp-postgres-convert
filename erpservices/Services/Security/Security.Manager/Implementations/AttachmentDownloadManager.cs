using Core;
using DAL.Core.Repository;
using Manager.Core;
using Security.DAL.Entities;
using Security.Manager.Dto;
using Security.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Security.Manager.Implementations
{
    public class AttachmentDownloadManager : ManagerBase, IAttachmentDownloadManager
    {
        
        private readonly IRepository<FileUpload> FileUploadRepo;

        public AttachmentDownloadManager(IRepository<FileUpload> fileUploadRepo)
        {
            FileUploadRepo = fileUploadRepo;
        }

        public byte[] GetAttachmentForDownload(DownloadAttachments parameters)
        {
            var fileUploadResult = FileUploadRepo.Entities.Where(x => x.FUID == parameters.FUID && x.TableName == parameters.TableName && x.ReferenceID == parameters.ReferenceId).Select(y => 
            new DownloadAttachments
            {
                FUID = (int)y.FUID,
                TableName = y.TableName,
                FilePath = y.FilePath,
                ReferenceId = y.ReferenceID,
                Description = y.Description,
                BaseUrl = parameters.BaseUrl,
                Module = parameters.Module
            }).FirstOrDefault();

            if (fileUploadResult != null) {
                HttpContent content = new StringContent("");
                Stream stream = new MemoryStream();
                using (var client = new WebClient())
                {
                    string fullPath = fileUploadResult.BaseUrl + fileUploadResult.Module + fileUploadResult.FilePath;
                    var content1 = client.DownloadData(fullPath);
                    stream = new MemoryStream(content1);
                }

                var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                byte[] byteArray = memoryStream.ToArray();

                string ds = Convert.ToBase64String(byteArray);

                return byteArray;
            }


            return null;
            
        }
    }
}
