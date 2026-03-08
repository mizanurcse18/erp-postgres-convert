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
    public interface IDivisionManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetDivisionListDic();
        Task<DivisionDto> GetDivision(int divisionId);

        void SaveChanges(DivisionDto divisionDto);
        Task Delete(int DivisionID );

        Task<List<Dictionary<string, object>>> GetExportDivisions(string whereCondition);
        Task<UploadGenericResponseDto<DivisionDto>> SaveChangesUploadDivision(IFormFile file);
    }
}
