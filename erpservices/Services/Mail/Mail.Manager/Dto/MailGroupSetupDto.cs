
using DAL.Core.EntityBase;
using Mail.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mail.Manager.Dto
{
    [AutoMap(typeof(MailGroupSetup)), Serializable]
    public class MailGroupSetupDto : Auditable
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
        public List<MailSetupDto> MailSetups { get; set; }

    }
    [AutoMap(typeof(MailSetup)), Serializable]
    public class MailSetupDto : Auditable
    {
        public int MailId { get; set; }
        public int GroupId { get; set; }
        public string To_CC_BCC { get; set; }
        public string Email { get; set; }
    }
}
