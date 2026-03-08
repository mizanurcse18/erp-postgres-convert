using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Security.DAL.Entities
{
    [Table("TutorialMaster")]
    public class TutorialMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TMID { get; set; }
        [Loggable]
        [Required]
        public string TutorialTypeID { get; set; }
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
        public string URL { get; set; }
        [Loggable]
        [Required]
        public string TableName { get; set; }
        [Loggable]
        public string VideoID { get; set; }
        public string Color { get; set; }
        [Loggable]
        [Required]
        public string Title { get; set; }
        public string DepartmentId { get; set; }
    }
}
