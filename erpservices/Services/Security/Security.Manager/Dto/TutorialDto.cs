using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;


namespace Security.Manager
{
    [AutoMap(typeof(TutorialMaster)), Serializable]
    public class TutorialDto : Auditable
    {

        public int TMID { get; set; }
        public string TutorialTypeID { get; set; }
        public string TutorialType { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string OriginalName { get; set; }
        public string URL { get; set; }
        public string TableName { get; set; }
        public string VideoID { get; set; }
        public string Color { get; set; }
        public string Title { get; set; }
        public string DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public List<FileUpload> TutorialImage { get; set; }
        public List<Attachments> Attachments { get; set; }
        


    }
}
