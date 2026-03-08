using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    [AutoMap(typeof(SystemVariable)), Serializable]
    public class SystemVariableDto : Auditable
    {
        public int SystemVariableID { get; set; }
        public int EntityTypeID { get; set; }
        public string EntityTypeName { get; set; }
        public string SystemVariableCode { get; set; }
        public string SystemVariableDescription { get; set; }
        public int? NumericValue { get; set; } = 1;
        public int Sequence { get; set; }
        public bool IsSystemGenerated { get; set; }
        public bool IsInactive { get; set; }
        public bool IsRemovable { get; set; }
        public string Status { get; set; }

    }
}
