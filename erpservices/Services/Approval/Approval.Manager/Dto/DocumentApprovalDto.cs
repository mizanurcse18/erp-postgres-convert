using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;

namespace Approval.Manager.Dto
{
    public class DocumentApprovalDto
    {
        public long DAMID { get; set; }
        public DateTime? RequestDate { get; set; }
        public string ReferenceNo { get; set; }
        public string ReferenceKeyword { get; set; }
        public int ApprovalStatusID { get; set; }
        public int TemplateID { get; set; }
        public string TemplateBody { get; set; }
        public string ExternalID { get; set; }
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
        public List<ManualApprovalPanelEmployeeDto> DocumentApprovalApprovalPanelList { get; set; }
        public int ApprovalProcessID { get; set; } = 0;
        public bool IsDraft { get; set; }
        public bool IsFromHR { get; set; }

    }
    public class DocumentApprovalTemplateDto
    {
        public long DATID { get; set; }
        public string DATName { get; set; }
        public string TemplateBody { get; set; }
        public int CategoryType { get; set; }
        public string CategoryTypeName { get; set; }
        public string Keywords { get; set; }
        public List<string> KeywordList { get; set; }

    }

    public class Attachments
    {
        public string AID { get; set; }
        public int FUID { get; set; }
        public int ID
        {
            get
            {
                int fuid;
                if (int.TryParse(AID, out fuid))
                {
                    return fuid;
                }
                else
                {
                    return 0;
                }
            }
        }
        public string AttachedFile { get; set; }
        public string Type { get; set; }
        public string OriginalName { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int ReferenceId { get; set; }
        public decimal Size { get; set; }
        public string Description { get; set; }
    }
}
