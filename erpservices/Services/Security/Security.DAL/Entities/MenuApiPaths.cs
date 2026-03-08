using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("MenuApiPaths")]
    class MenuApiPaths : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MAPID { get; set; }
        [Required]
        [Loggable]
        public int MenuID { get; set; }
        [Required]
        [Loggable]
        public string Module { get; set; }
        [Required]
        [Loggable]
        public string Controller { get; set; }
        [Required]
        [Loggable]
        public string ApiPath { get; set; }
        [Required]
        [Loggable]
        public string ActionType { get; set; }
    }
}
