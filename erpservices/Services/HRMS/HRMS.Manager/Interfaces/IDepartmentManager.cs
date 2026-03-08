using Core;
using HRMS.Manager.Dto;
using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IDepartmentManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetDepartmentListDic();
        Task<Dictionary<string, object>> GetDepartment(int departmentId);

        Task<DepartmentDto> SaveChanges(DepartmentDto departmentDto);
        Task<UploadGenericResponseDto<DepartmentDto>> SaveChangesUploadDepartment(IFormFile file);
        Task Delete(int DepartmentID );
        Task<List<Dictionary<string, object>>> GetExportDepartments(string whereCondition);
    }
}
