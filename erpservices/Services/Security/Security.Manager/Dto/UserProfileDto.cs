using Core;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Newtonsoft.Json;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Security.Manager.Dto
{
    [AutoMap(typeof(UserProfile)), Serializable]
    public class UserProfileDto : Auditable
    {
        public int UserID { get; set; }       
        public string UserFullName { get; set; } 
        public string UserName { get; set; }
        public int DistributionHouseID { get; set; }        
        public int RegionID { get; set; }       
        public int ClusterID { get; set; }        
        public int PositionID { get; set; }        
        public string ContactNumber { get; set; }        
        public string Email { get; set; }        
        public DateTime? JoiningDate { get; set; }        
        public string Longitude { get; set; }        
        public string Latitude { get; set; }        
        public int VisitTypeID { get; set; }

        public string DuplicateUserError { get; set; }
        public string ParentCode { get; set; }
    }
}
