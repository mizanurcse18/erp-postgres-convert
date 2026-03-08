using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Security.Manager.Dto
{
    [AutoMap(typeof(Location)), Serializable]
    public class LocationDto : Auditable
    {
        public int LocationID { get; set; }
        public string LocationName { get; set; }
        public bool? IsActive { get; set; }
        public string IsActiveLocation { get; set; }
    }
}
