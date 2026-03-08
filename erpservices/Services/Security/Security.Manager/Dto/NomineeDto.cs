using Manager.Core.Mapper;
using DAL.Core.EntityBase;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    [AutoMap(typeof(NomineeInformation)), Serializable]
    public class NomineeDto : Auditable
    {
        public int NIID { get; set; }
        public string NomineeName { get; set; }
        public string NomineeAddress { get; set; }
        public string RelationShip { get; set; }
        public DateTime DateOfBirth { get; set; }
        public decimal Percentage { get; set; }
        public string NomineeBehalf { get; set; }
        public int PersonID { get; set; }
        public bool isDelete { get; set; }
    }
}
