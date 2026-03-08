using Security.DAL;
using Newtonsoft.Json;
using System;
using Core;
using DAL.Core;
using Security.DAL.Entities;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System.Collections.Generic;

namespace Security.Manager
{
    [AutoMap(typeof(SecurityGroupMaster)), Serializable]
    public class SecurityGroupMasterDto : Auditable
    {
        public int SecurityGroupID { get; set; }
        public string SecurityGroupName { get; set; }
        public string SecGroupDescription { get; set; }
        public List<SecurityGroupUserChild> SecurityGroupUserChildList { get; set; }
    }
}