using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Approval.DAL.Entities
{
    [Table("DocumentApprovalMaster"), Serializable]
    public class DocumentApprovalMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long DAMID { get; set; }
        [Loggable]
        public DateTime? RequestDate { get; set; }
        [Loggable]
        public string ReferenceNo { get; set; }
        [Loggable]
        public string ReferenceKeyword { get; set; }
        [Loggable]
        public int ApprovalStatusID { get; set; }
        [Loggable]
        public int TemplateID { get; set; }
        [Loggable]
        public string TemplateBody { get; set; }
        [Loggable]
        public string ExternalID { get; set; }
        [Loggable]
        public bool IsDraft { get; set; }
    }
}
