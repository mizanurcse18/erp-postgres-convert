using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HRMS.Manager.Implementations
{
    public class JobGradeManager : ManagerBase, IJobGradeManager
    {

        private readonly IRepository<JobGrade> JobGradeRepo;
        public JobGradeManager(IRepository<JobGrade> jobGradeRepo)
        {
            JobGradeRepo = jobGradeRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetJobGradeListDic()
        {
            string sql = $@"SELECT
                                jg.job_grade_id AS ""JobGradeID"",
                                jg.job_grade_code AS ""JobGradeCode"",
                                jg.job_grade_name AS ""JobGradeName"",
                                CASE 
                                    WHEN emp.job_grade_id <> 0
                                        THEN FALSE
                                    ELSE TRUE
                                END AS ""IsRemovable""
                            FROM
                                job_grade jg
                            LEFT JOIN
                                employment emp ON emp.job_grade_id = jg.job_grade_id AND emp.is_current = TRUE
                            ORDER BY
                                jg.job_grade_id DESC";
            var listDict = JobGradeRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public async Task<JobGradeDto> GetJobGrade(int JobGradeID)
        {

            var dept = JobGradeRepo.SingleOrDefault(x => x.JobGradeID == JobGradeID).MapTo<JobGradeDto>();

            return await Task.FromResult(dept);
        }


        public async Task Delete(int JobGradeID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var jobGradeEnt = JobGradeRepo.Entities.Where(x => x.JobGradeID == JobGradeID).FirstOrDefault();

                jobGradeEnt.SetDeleted();
                JobGradeRepo.Add(jobGradeEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public void SaveChanges(JobGradeDto jobGradeDto)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existUser = JobGradeRepo.Entities.SingleOrDefault(x => x.JobGradeID == jobGradeDto.JobGradeID).MapTo<JobGrade>();

                if (existUser.IsNull() || jobGradeDto.JobGradeID.IsZero() )
                {
                    jobGradeDto.SetAdded();
                    SetNewUserID(jobGradeDto);
                }
                else
                {
                    jobGradeDto.SetModified();
                }

                var jobGradeEnt = jobGradeDto.MapTo<JobGrade>();
                SetAuditFields(jobGradeEnt);
                JobGradeRepo.Add(jobGradeEnt);
                unitOfWork.CommitChangesWithAudit();
            }
        }

        private void SetNewUserID(JobGradeDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("JobGrade", AppContexts.User.CompanyID);
            obj.JobGradeID = code.MaxNumber;
        }

    }
}
