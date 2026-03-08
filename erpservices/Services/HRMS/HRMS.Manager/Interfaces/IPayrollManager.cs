using HRMS.Manager.Dto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IPayrollManager
    {
        Task<GenericResponse<EmployeePaySlipDto>> UploadPaySlip(int patID, int activityTypeID, string monthId, int year, IFormFile file);
        Task<GenericResponse<RegularIncentiveDto>> UploadRegularIncentive(int patID, int activityTypeID, string periodID, int year, IFormFile file);
        Task<GenericResponse<EmployeeMonthlyIncentiveDto>> UploadMonthlyIncentive(int patID, int activityTypeID, string monthId, int year, string incentiveType, IFormFile file);
        Task<GenericResponse<EmployeeFestivalBonusDto>> UploadFestivalBonus(int patID, int activityTypeID, int year, int BonusType, IFormFile file);

        Task<EmployeePaySlipInfoDto> DownloadPayslip(PaySlipModelDto model);
        Task<byte[]> GeneratePaySlipAsync(EmployeePaySlipInfoDto payslip);

        Task<EmployeeMonthlyIncentivePayslipDto> DownloadMonthlyIncentive(PaySlipModelDto model);
        Task<byte[]> GenerateMonthlyIncentiveAsync(EmployeeMonthlyIncentivePayslipDto payslip);

        Task<RegularIncentivePayslipDto> DownloadRegularIncentive(PaySlipModelDto model);
        Task<byte[]> GenerateRegularIncentiveAsync(RegularIncentivePayslipDto payslip);

        Task<EmployeeFestivalBonusPayslipDto> DownloadFestivalBonus(PaySlipModelDto model);
        Task<byte[]> GenerateFestivalBonusAsync(EmployeeFestivalBonusPayslipDto payslip);

    }
}
