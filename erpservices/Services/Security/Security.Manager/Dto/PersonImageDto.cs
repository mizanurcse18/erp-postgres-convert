
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Http;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    [AutoMap(typeof(PersonImage)), Serializable]
    public class PersonImageDto : Auditable
    {
        public string AID { get; set; }
        public int PIID { get; set; }
        public int ID
        {
            get
            {
                int puid;
                if (int.TryParse(AID, out puid))
                {
                    return puid;
                }
                else
                {
                    return 0;
                }
            }
        }
        public int PersonID { get; set; }


        public string ImagePath { get; set; }

        public string ImageName { get; set; }

        public string ImageType { get; set; }

        public string ImageOriginalName { get; set; }

        public int? ImageCategory { get; set; }

        public bool IsFavorite { get; set; } = false;
        public bool IsSignature { get; set; } = false;

        public int? GalleryID { get; set; }
        public String ImageFile { get; set; }
        //public IFormFile OriginalFile { get; set; }
        public string ImageTypeValidation { get; set; }
    }
}
