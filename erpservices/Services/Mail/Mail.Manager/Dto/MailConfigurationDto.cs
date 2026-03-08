
using DAL.Core.EntityBase;
using Mail.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mail.Manager.Dto
{
    [AutoMap(typeof(MailConfiguration)), Serializable]
    public class MailConfigurationDto : Auditable
    {
        public int ConfigId { get; set; }
        public string ConfigName { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public decimal SeqNo { get; set; }
        public bool EnableSsl { get; set; }
        public int Timeout { get; set; }
        public int SleepTime { get; set; }

    }
}
