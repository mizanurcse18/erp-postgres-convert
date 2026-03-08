using API.Core;
using Core;
using HRMS.API.Models;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class PayrollController : BaseController
    {
        private readonly IPayrollManager Manager;

        public PayrollController(IPayrollManager manager)
        {
            Manager = manager;
        }

        [HttpPost("UploadPayrollFile")]
        public async Task<IActionResult> UploadPayrollFile([FromForm] PayrollFileUploadDto fileDto)
        {
            // Upload PaySlip
           if(fileDto.activityTypeID ==(int)Util.UploadPayslipCategory.Payslip)
            {
                var result = await Manager.UploadPaySlip(fileDto.patID, fileDto.activityTypeID,
                                    fileDto.monthId,
                                   fileDto.yearID, fileDto.file);

                return OkResult(result);
            }

           // Upload Regular Increment
            else if (fileDto.activityTypeID == (int)Util.UploadPayslipCategory.RegularIncentive)
            {
                var result = await Manager.UploadRegularIncentive(fileDto.patID, fileDto.activityTypeID,
                                   fileDto.periodID,fileDto.yearID ,fileDto.file);

                return OkResult(result);
            }

           // Upload Monthly Increment
            else if (fileDto.activityTypeID == (int)Util.UploadPayslipCategory.MonthlyIncentive)
            {
                var result = await Manager.UploadMonthlyIncentive(fileDto.patID, fileDto.activityTypeID,
                                    fileDto.monthId,
                                   fileDto.yearID, fileDto.IncentiveType, fileDto.file);

                return OkResult(result);
            }

            // Upload Festival Bonus Payslip
            else if (fileDto.activityTypeID == (int)Util.UploadPayslipCategory.FestivalBonus)
            {
                var result = await Manager.UploadFestivalBonus(fileDto.patID, fileDto.activityTypeID,
                                   fileDto.yearID, fileDto.BonusType, fileDto.file);

                return OkResult(result);
            }
            else
            {
                return OkResult();

            }
        }

        [HttpPost("DownloadPayslip")]
        public async Task<ActionResult> DownloadPayslip(PaySlipModelDto model)
        {

            var fileName = "";
            if(model.CategoryType == "246")
            {
                fileName = "Monthly Incentive Payslip";
                var result = await Manager.DownloadMonthlyIncentive(model);
                if (result.EmployeeID <= 0)
                {
                    return NotFoundResult();
                }
                var pdfBytes = await Manager.GenerateMonthlyIncentiveAsync(result);
                return OkResult(new { pdfBytes, fileName });
            }
            else if (model.CategoryType == "162")
            {
                fileName = "PaySlip";
                var result = await Manager.DownloadPayslip(model);
                //result.ArrearBasicSalary = "-789";
                //result.ArrearHouseRent = "1.00000";
                //result.ArrearMedicalAllowance = "-0.7899";
                //result.ArrearConveyanceAllowance = "0";
                //result.IncomeTax = "-0436";
                //result.MobileAllowance = "0";
                //result.TotalArrears = "-12340";
                //result.TotalDeductions = "-9078834";
                //result.LaptopRepairingCostDeducted = "-3456";
                //result.MobileAllowance = "0.00";
                //result.ProvidentFund = "2344";
                if (result.EmployeeID <= 0)
                {
                    return NotFoundResult();
                }
                var pdfBytes = await Manager.GeneratePaySlipAsync(result);
               return OkResult(new { pdfBytes, fileName });
            }
            else if (model.CategoryType == "247")
            {
                fileName = "Regular Incentive Payslip";
                var result = await Manager.DownloadRegularIncentive(model);
                if (result.EmployeeID <= 0)
                {
                    return NotFoundResult();
                }
                var pdfBytes = await Manager.GenerateRegularIncentiveAsync(result);
                return OkResult(new { pdfBytes, fileName });
            }
            else if (model.CategoryType == "207")
            {
                fileName = "Festival Bonus Payslip";
                var result = await Manager.DownloadFestivalBonus(model);
                if (result.EmployeeID <= 0)
                {
                    return NotFoundResult();
                }
                var pdfBytes = await Manager.GenerateFestivalBonusAsync(result);
                return OkResult(new { pdfBytes, fileName });
            }

            //var pdfBytes = await Manager.GeneratePaySlipAsync(result);
            return OkResult();
            

            //return File(pdfBytes, "application/pdf", $"payslip_{DateTime.Now:yyyyMMdd}.pdf");
        }
    }
}
