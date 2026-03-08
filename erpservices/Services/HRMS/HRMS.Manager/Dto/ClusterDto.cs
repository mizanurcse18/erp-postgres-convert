
using DAL.Core.EntityBase;
using HRMS.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    [AutoMap(typeof(Cluster)), Serializable]
    public class ClusterDto : Auditable
    {
        public int ClusterID { get; set; }
        public string ClusterName { get; set; }
        public string ClusterCode { get; set; }

    }
}
