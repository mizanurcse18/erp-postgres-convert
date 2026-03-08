
using DAL.Core.EntityBase;
using HRMS.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    [AutoMap(typeof(Region)), Serializable]
    public class RegionDto : Auditable
    {
        public int RegionID { get; set; }
        public string RegionName { get; set; }
        public string RegionCode { get; set; }
        public int? ClusterID { get; set; }
        public string ClusterName { get; set; }

    }
}
