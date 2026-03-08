using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    [AutoMap(typeof(PersonFamilyInfo)), Serializable]
    public class PersonFamilyInfoDto : Auditable
    {
        public int PFIID { get; set; }
        public int PersonID { get; set; }
        public int RelationshipTypeID { get; set; }
        public string Name { get; set; }
        public int? GenderID { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool isDelete { get; set; }
    }
}
