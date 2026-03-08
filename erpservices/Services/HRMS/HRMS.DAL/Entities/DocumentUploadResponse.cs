using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("DocumentUploadResponse")]
    public class DocumentUploadResponse : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DURID { get; set; }
        [Required]
        [Loggable]
        public int DUID { get; set; }
        [Required]
        [Loggable]
        public string ApiResponse { get; set; } 
        [Loggable]
        public string ApiStatus { get; set; } 
        
    }
}
