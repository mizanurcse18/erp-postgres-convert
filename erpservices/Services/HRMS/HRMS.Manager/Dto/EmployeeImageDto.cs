
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Http;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    [AutoMap(typeof(EmployeeImage)), Serializable]
    public class EmployeeImageDto: Auditable
    {
        public int PIID { get; set; }
        public int EmployeeID { get; set; }


        public string ImagePath { get; set; }
        
        public string ImageName { get; set; }
        
        public string ImageType { get; set; }
        
        public string ImageOriginalName { get; set; }
        
        public int? ImageCategory { get; set; }
        
        public bool IsFavorite { get; set; } = false;
        
        public int? GalleryID { get; set; }
        public String ImageFile { get; set; }
        //public IFormFile OriginalFile { get; set; }
    }
}
