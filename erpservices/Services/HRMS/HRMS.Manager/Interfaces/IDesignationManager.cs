using Core;
using HRMS.Manager.Dto;
using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IDesignationManager
    {
        Task<List<DesignationDto>> GetDesignationList();
        void SaveChanges(DesignationDto designationDto);
        void DeleteDesignation(int designationId);
        Task<DesignationDto> GetDesignation(int designationId);
        Task<UploadGenericResponseDto<DesignationDto>> SaveChangesUploadDesignation(FileRequestDto request);


    }
}
