using Core;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Security.Manager.Interfaces
{
    public interface IAttachmentDownloadManager
    {
        byte[] GetAttachmentForDownload(DownloadAttachments parameters);
    }
}
