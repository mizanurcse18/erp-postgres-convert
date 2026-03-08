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
    [Table("CommonInterface")]
    public class CommonInterface : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int MenuID { get; set; }
		[Required]
		[Loggable]
		public string TableName { get; set; }
		[Required]
		[Loggable]
		public string KeyFields { get; set; }
		[Required]
		[Loggable]
		public string InterfaceType { get; set; }
		[Loggable]
		public string GridHeaderFilter { get; set; }
		[Loggable]
		public string GridRows { get; set; }
		[Loggable]
		public string GridDataSource { get; set; }
		[Loggable]
		public string GridDataSort { get; set; }
		[Loggable]
		public string GridDataOrder { get; set; }
		[Loggable]
		public bool IsRowSelectAction { get; set; }
		[Loggable]
		public string Tabs { get; set; }
		[Loggable]
        public int ColumnsPerRow { get; set; }
		[Loggable]
		public string AssemblyInfo { get; set; }
		[Loggable]
        public string GetDataGetSource { get; set; }
    }
}
