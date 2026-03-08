using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("PersonImage")]
    public class PersonImage : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PIID { get; set; }
        [Loggable]
        public string ImagePath { get; set; }
        [Loggable]
        public string ImageName { get; set; }
        [Loggable]
        public string ImageType { get; set; }
        [Loggable]
        public string ImageOriginalName { get; set; }
        [Loggable]
        public int PersonID { get; set; }
        [Loggable]
        public int? ImageCategory { get; set; }
        [Loggable]
        public bool IsFavorite { get; set; } = false;
        [Loggable]
        public bool IsSignature { get; set; } = false;
        [Loggable]
        public int? GalleryID { get; set; }
    }
}
