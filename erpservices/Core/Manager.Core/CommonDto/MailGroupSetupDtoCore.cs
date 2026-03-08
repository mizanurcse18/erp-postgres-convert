using System;

namespace Manager.Core.CommonDto
{
    public class MailGroupSetupDtoCore
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int ConfigId { get; set; }
        public string ConfigName { get; set; }
        public string AttachmentPath { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public int Priority { get; set; }
        public string PriorityName { get; set; }
        public int Sensitivity { get; set; }
        public string SensitivityName { get; set; }
        public DateTime ReportGenTime { get; set; }
        public DateTime MailGenTime { get; set; }
        public string IntervalOn { get; set; }
        public decimal IntervalValue { get; set; }
        public bool IsFromInterface { get; set; }

    }
}
