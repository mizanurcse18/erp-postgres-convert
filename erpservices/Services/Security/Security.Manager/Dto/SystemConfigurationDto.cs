using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;


namespace Security.Manager
{
    [AutoMap(typeof(SystemConfiguration)), Serializable]
    public class SystemConfigurationDto : Auditable
    {
        public int SystemConfigurationID { get; set; }
        public int UserAccountLockedDurationInMin { get; set; }
        public int UserPasswordChangedDurationInDays { get; set; }
        public int AccessFailedCountMax { get; set; }
        public bool IsActive { get; set; }

    }
}
