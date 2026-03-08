using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Security.DAL.Entities
{
    [Table("FileUpload")]
    public class FileUpload : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FUID { get; set; }
        [Loggable]
        [Required]
        public string FilePath { get; set; }
        [Required]
        [Loggable]
        public string FileName { get; set; }
        [Required]
        [Loggable]
        public string FileType { get; set; }
        [Required]
        [Loggable]
        public string OriginalName { get; set; }
        [Loggable]
        [Required]
        public int ReferenceID { get; set; }
        [Loggable]
        [Required]
        public string TableName { get; set; }
        [Loggable]
        public int? ParentFUID { get; set; }
        [Loggable]
        public bool IsFolder { get; set; }
        public decimal SizeInKB { get; set; }
        [Loggable]
        public string Description { get; set; }
    }
}
