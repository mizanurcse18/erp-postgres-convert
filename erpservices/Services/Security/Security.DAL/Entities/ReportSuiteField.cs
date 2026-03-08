using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("ReportSuiteField")]
    public class ReportSuiteField : EntityBase
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ReportFieldId { get; set; }
        [Loggable]
        public int ReportId { get; set; }
        [Required]
        [Loggable]
        public string ValueField { get; set; }
        [Loggable]
        public string LabelField { get; set; }
        [Loggable]
        [Required]
        public string Label { get; set; }
        [Loggable]
        public string DefaultValue { get; set; }
        [Loggable]
        public string FieldType { get; set; }
        [Loggable]
        public string MapField { get; set; }
        [Loggable]
        public string ReferenceSource { get; set; }
        [Loggable]
        public string Operators { get; set; }
        [Loggable]
        public bool FilterOnly { get; set; }
        [Loggable]
        public decimal SeqNo { get; set; }
        [Loggable]
        public bool IsSysParameter { get; set; }
        [Loggable]
        public bool MultiSelect { get; set; }

    }
}
