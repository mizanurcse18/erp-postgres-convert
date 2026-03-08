using Core;
using HRMS.Manager.Dto;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IJobGradeManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetJobGradeListDic();
        Task<JobGradeDto> GetJobGrade(int jobGradeId);

        void SaveChanges(JobGradeDto jobGradeDto);
        Task Delete(int JobGradeID );

    }
}
