
using DAL.Core.EntityBase;
using HRMS.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    [AutoMap(typeof(WageCodeConfiguration)), Serializable]
    public class WageCodeConfigurationDto : Auditable
    {
        public int WageCodeConfigurationID { get; set; }
        public string WageCode { get; set; }
        public string Description { get; set; }
        public int TypeID { get; set; }
        public string TypeName { get; set; }
        public bool ExceptionFlag { get; set; }
        public List<WageCodeConfigurationDto> Configurations { get; set; }

    }
}
