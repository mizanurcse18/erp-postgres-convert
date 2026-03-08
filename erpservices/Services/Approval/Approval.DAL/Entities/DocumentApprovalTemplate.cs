using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Approval.DAL.Entities
{
    [Table("DocumentApprovalTemplate"), Serializable]
    public class DocumentApprovalTemplate : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long DATID { get; set; }
        [Loggable]
        public string DATName { get; set; }
        [Loggable]
        public string TemplateBody { get; set; }
        [Loggable]
        public int CategoryType { get; set; }
        [Loggable]
        public string Keywords { get; set; }
    }
}
