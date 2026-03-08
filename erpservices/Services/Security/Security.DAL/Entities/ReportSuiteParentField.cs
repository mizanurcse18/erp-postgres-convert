using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("ReportSuiteParentField")]
    public class ReportSuiteParentField : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ReportParentId { get; set; }
        [Loggable]
        [Required]
        public int ReportFieldId { get; set; }
        [Required]
        [Loggable]
        public string ParentField { get; set; }
        [Loggable]
        public string Condition { get; set; }   
        [Loggable]
        public bool BlankAllow { get; set; }        
    }
}
