using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class AuditApprovalDto
    {
        public int MapID { get; set; }
        public List<MultiQuestionListItem> MultiQuestionList { get; set; }
        public List<MultiDepartmentListItem> MultiDepartmentList { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public bool IsRequired {  get; set; }
        public bool IsPOSMRequired { get; set; }
        public string Remarks { get; set; }
        public string DepartmentIDsStr { get; set; }
        public string QuestionIDsStr { get; set; }

    }
    public class MultiQuestionListItem
    {
        public int Value { get; set; }
        public string Label { get; set; }
    }

    public class MultiDepartmentListItem
    {
        public int Value { get; set; }
        public string Label { get; set; }
    }
}
