using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("ReportSuite")]
    public class ReportSuite : EntityBase
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ReportId { get; set; }
        [Loggable]
        public int? ApplicationId { get; set; } 
        [Loggable]
        public int? ParentId { get; set; }
        [Required]
        [Loggable]
        public string DisplayName { get; set; }
        [Loggable]
        public string ReportName { get; set; }
        [Loggable]
        public string ReportPath { get; set; }
        [Loggable]
        public string ConName { get; set; }
        [Loggable]
        public decimal SeqNo { get; set; }
        [Loggable]
        public bool IsVisible { get; set; }
    }
}
