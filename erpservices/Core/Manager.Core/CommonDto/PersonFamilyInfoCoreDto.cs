using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;
namespace Manager.Core.CommonDto
{
    public class PersonFamilyInfoCoreDto
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
