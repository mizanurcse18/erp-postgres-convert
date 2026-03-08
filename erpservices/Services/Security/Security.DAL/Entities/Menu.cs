using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("Menu")]
    public class Menu : EntityBase
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MenuID { get; set; }
        public int ParentID { get; set; }
        public int? ApplicationID { get; set; }
        public string ID { get; set; }
        public string Title { get; set; }
        public string Translate { get; set; }
        public string Type { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }
        public string Badge { get; set; }
        public string Target { get; set; }
        public bool Exact { get; set; }
        public string Auth { get; set; }
        public string Parameters { get; set; }
        public bool IsVisible { get; set; }
        public decimal SequenceNo { get; set; }
    }
}
