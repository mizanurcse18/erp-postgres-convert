
using DAL.Core.EntityBase;
using HRMS.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    [AutoMap(typeof(JobGrade)), Serializable]
    public class JobGradeDto : Auditable
    {

        public int JobGradeID { get; set; }
        public string JobGradeName { get; set; }
        public decimal SequenceNo { get; set; }

    }
}
