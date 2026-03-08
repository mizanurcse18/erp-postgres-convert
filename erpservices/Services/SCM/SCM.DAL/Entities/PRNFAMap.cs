using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("PRNFAMap"), Serializable]
    public class PRNFAMap : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PRNFAMapID { get; set; }
        [Loggable]
        [Required]
        public long PRMID { get; set; }
        [Loggable]
        public int? NFAID { get; set; }
        [Loggable]
        public string NFAReferenceNo { get; set; }

        [Loggable]
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal NFAAmount { get; set; }
        [Loggable]
        [Required]
        public bool IsFromSystem { get; set; }
    }
}
