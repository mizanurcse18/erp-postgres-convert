using Core;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;

namespace Approval.Manager.Dto
{
    public class DOADto
    {
        public long DOAMasterID { get; set; }
        public long EmployeeID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StatusID { get; set; }
        public string Remarks { get; set; }
        public ComboModel Status { get; set; }
        public ComboModel Employee { get; set; }
        public int IsHR { get; set; }
        public List<DOAItemDetails> DOAItemDetails { get; set; }
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();

    }

    public class DOAItemDetails
    {
        public long DOAApprovalPanelEmployeeID { get; set; }
        public long DOAMasterID { get; set; }
        public long AssigneeEmployeeID { get; set; }
        public int TypeID { get; set; }
        public int APPanelID { get; set; }
        public long GroupID { get; set; }
        public ComboModel DOAType { get; set; }
        public List<ComboModel> MultipleAPPanelDetails { get; set; }
        public List<ComboModel> MultipleEmployeeDetails { get; set; }

    }
}
