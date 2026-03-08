using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    [AutoMap(typeof(Unit)), Serializable]
    public class UnitDto:Auditable
    {

        public int UnitID { get; set; }
        public string UnitCode { get; set; }
        public string UnitShortCode { get; set; }
        public decimal? LelativeFactor { get; set; }
        public bool IsRemovable { get; set; }

    }
}
