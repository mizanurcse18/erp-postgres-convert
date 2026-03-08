using Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;
using DAL.Core.JsonConverter;
using System;
using DAL.Core.EntityBase;
using DAL.Core.Attribute;

namespace Security.DAL.Entities
{
    [Table("CommonInterfaceFields")]
    public class CommonInterfaceFields : EntityBase
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RowID { get; set; }
        [Required]
        [Loggable]
        public int MenuID { get; set; }
        [Required]
        [Loggable]
        public string DataMember { get; set; }
        [Required]
        [Loggable]
        public string Label { get; set; }
        [Required]
        [Loggable]
        public string DataType{ get; set; }
        [Loggable]
        public string Placeholder { get; set; }
        [Required]
        [Loggable]
        public decimal SeqNo { get; set; }
        [Loggable]
        public string TabName { get; set; }
        [Loggable]
        public bool Required { get; set; }
        [Loggable]
        public bool IsVisible { get; set; }
        [Loggable]
        public bool IsReadOnly { get; set; }
        [Loggable]
        public string AutocompleteProps { get; set; }
        [Loggable]
        public string AutocompleteSource { get; set; }
        [Loggable]
        public bool IsPrimaryKey { get; set; }
        [Loggable]
        public bool IsAutoIncrement { get; set; }
        [Loggable]
        public int Rows { get; set; }
        [Loggable]
        public string ValidationRules { get; set; }
        [Loggable]
        public bool IsChild { get; set; }
        [Loggable]
        public string ChildTableName { get; set; }
        [Loggable]
        public string ChildTableStyle { get; set; }
        [Loggable]
        public string ChildTableTitle{ get; set; }
    }
}
