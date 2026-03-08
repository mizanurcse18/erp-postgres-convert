using Core;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class ExternalAuditQuestionDeptPOSMDto
    {
        public int QuestionID { get; set; }
        public string Question { get; set; }
        public List<ComboModel> Departments { get; set; }
        public bool IsRequried { get; set; }
        public bool IsPOSMRequired { get; set; }
        public List<ComboModel> Uddokta_POSM { get; set; }
        public List<ComboModel> Merchant_POSM { get; set; }

    }
}
