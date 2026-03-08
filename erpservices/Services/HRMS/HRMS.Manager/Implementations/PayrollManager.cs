using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Manager.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OfficeOpenXml;
using Security.DAL.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using static Core.Util;

namespace HRMS.Manager.Implementations
{
    public class PayrollManager : ManagerBase, IPayrollManager
    {
        private readonly IRepository<Employee> EmployeeRepo;
        private readonly IRepository<PayrollAuditTrial> PayrollAuditTrail;
        private readonly IRepository<EmployeePaySlipInfo> PaySlipInfoRepo;
        private readonly IRepository<EmployeeRegularIncentiveInfo> RegularIncentiveRepo;
        private readonly IRepository<EmployeeMonthlyIncentiveInfo> MonthlyIncentiveRepo;
        private readonly IRepository<EmployeeFestivalBonusInfo> FestivalBonusRepo;
        private readonly IConfiguration Config;

        public PayrollManager(IRepository<Employee> employeeRepo,
                IRepository<PayrollAuditTrial> payrollAuditTrail,
                IRepository<EmployeePaySlipInfo> paySlipInfoRepo,
                IRepository<EmployeeRegularIncentiveInfo> regularIncentiveRepo,
                IRepository<EmployeeMonthlyIncentiveInfo> monthlyIncentiveRepo,
                IRepository<EmployeeFestivalBonusInfo> festivalBonusRepo,
                IConfiguration config)
        {
            EmployeeRepo = employeeRepo;
            PayrollAuditTrail = payrollAuditTrail;
            PaySlipInfoRepo = paySlipInfoRepo;
            RegularIncentiveRepo = regularIncentiveRepo;
            MonthlyIncentiveRepo = monthlyIncentiveRepo;
            FestivalBonusRepo = festivalBonusRepo;
            Config = config;
        }
        //Task Here   EmployeePaySlipInfo
        public async Task<GenericResponse<EmployeePaySlipDto>> UploadPaySlip(int patID, int activityTypeID, string monthId, int year, IFormFile file)
        {
            string fileType = file.FileName.Split('.')[1];
            var secretKey = string.IsNullOrEmpty(Convert.ToString(Config["AppSettings:EncSecret"])) ? string.Empty
                                : Config["AppSettings:EncSecret"];

            GenericResponse<EmployeePaySlipDto> resp = new GenericResponse<EmployeePaySlipDto>();
            PayrollAuditTrial auditData = new PayrollAuditTrial();
            PaySlipDataModel modelData = new PaySlipDataModel();
            if (patID == 0 && activityTypeID > 0 && !string.IsNullOrEmpty(monthId) && year > 0)
            {
                var auditTrail = PayrollAuditTrail.FirstOrDefault(a => a.ActivityTypeID == activityTypeID
                                                        && a.ActivityPeriod == monthId + '-' + year && a.ActivityStatusID == 1);
                if (!auditTrail.IsNullOrDbNull())
                {
                    resp.status = false;
                    //resp.message = "Data was already uploaded for this month.";
                    resp.message = "Files for this month has been already uploaded. Please use Audit Trail for re-upload.";
                    return resp;
                }

            }
            if (patID > 0)
            {
                var auditTrail = PayrollAuditTrail.FirstOrDefault(a => a.PATID == patID && a.ActivityTypeID == activityTypeID
                                                        && a.ActivityPeriod == monthId + '-' + year && a.ActivityStatusID == 1);

                if (auditTrail.IsNullOrDbNull())
                {
                    resp.status = false;
                    resp.message = "Invalid file uploaded.";
                    return resp;
                }

                auditData = auditTrail;
            }
            string[] excelValidCols = { "Salary_Month_Year", "Disbursement_Date",
                                        "Employee_ID", "Designation",
                                        "Division", "Name", "Department",
                                        "Joining_Date","Basic_Salary",
                                        "House_Rent_Allowance","Medical_Allowance",
                                        "Conveyance_Allowance","Free_Or_Concessional_Passage_For_Travel",
                                        "Payroll_Card_Part","Arrear_Basic_Salary",
                                        "Arrear_House_Rent_Allowance","Arrear_Medical_Allowance",
                                        "Arrear_Conveyance_Allowance",
                                        "Arrear_Free_Or_Concessional_Passage_For_Travel",
                                        "Total_Earnings","Total_Arrears",
                                        "Income_Tax","Deduction_Field_1",
                                        "Deduction_Field_2","Total_Deductions","Net_Payable",
                                        "Amount_In_Words","Bank_Amount_BDT",
                                        "Wallet_Amount","Cash_Out_Charge","Extra_Mobile_Bill_Deducted",
                                        "Market_Bonus","Weekend_Allowance","Festival_Holiday_Allowance",
                                        "Saturday_Allowance","Tax_Support","Festival_Bonus_Arrear",
                                        "Salary_Advance","Tax_Refund","Laptop_Repairing_Cost_Deducted","Provident_Fund",
                                        "Mobile_Bill_Adjustment","Car_Allowance"
                                      };

            if (fileType.Equals("xlsx"))
            {
                using var memoryStream = new MemoryStream();
                file.CopyTo(memoryStream);
                var bytes = memoryStream.ToArray();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(new MemoryStream(bytes)))
                {

                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;
                    List<string> invalidColName = new List<string>();
                    int columnCount = worksheet.Dimension.Columns;
                    for (int col = 1; col <= columnCount; col++)
                    {
                        var columnName = worksheet.Cells[1, col].Text;
                        var current = excelValidCols[col - 1].Trim().ToLower();
                        var selfVal = columnName.Trim().ToLower();
                        var isOK = excelValidCols[col - 1].Trim().ToLower().Equals(columnName.Trim().ToLower())
                                    ? true : false;
                        if (!isOK)
                            invalidColName.Add(columnName);


                        resp.message = "File re-uploaded successfully";
                        resp.status = true;

                    }

                    if (invalidColName.Count > 0)
                    {
                        resp.status = false;
                        resp.message = "Invalid file column header.";
                        return resp;
                    }

                    bool validationStatus = true;
                    modelData = ValidateEmployeePaySlipExcel(worksheet, rowCount, monthId.ToString(), year.ToString(), secretKey, ref validationStatus);
                    if (modelData.IsValid == false)
                    {
                        resp.status = false;
                        resp.message = modelData.message;
                        return resp;
                    }

                    if (!validationStatus)
                    {
                        resp.status = false;
                        resp.message = "Invalid file data.";
                        resp.data = modelData.paySlipDtos;
                        return resp;
                    }

                }

            }

            else if (fileType.Equals("csv"))
            {

                List<string> headrs = new List<string>();
                //var fta = new GenericParse<PaySlipCSVDto>().ConvertCsvToXlsx(file);

                var fileData = new GenericParse<PaySlipCSVDto>().ConvertCsvToList(file, ref headrs, excelValidCols);

                List<string> invalidColName = new List<string>();
                for (int col = 0; col <= headrs.Count() - 1; col++)
                {
                    var columnName = headrs[col];
                    var isOK = excelValidCols[col].Trim().ToLower().Equals(columnName.Trim().ToLower())
                                   ? true : false;
                    if (!isOK)
                        invalidColName.Add(columnName);
                }

                if (invalidColName.Count > 0)
                {
                    resp.status = false;
                    resp.message = "Invalid file column header.";
                    return resp;
                }

                bool validationStatus = true;
                modelData = ValidateEmployeePaySlipCSV(fileData, monthId.ToString(), year.ToString(), secretKey, ref validationStatus);

                if (!validationStatus)
                {
                    resp.status = false;
                    resp.message = "Invalid file data.";
                    resp.data = modelData.paySlipDtos;
                    return resp;
                }
            }

            else
            {
                resp.status = false;
                resp.message = "Invalid file format.";
                return resp;
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (patID > 0)
                {
                    PayrollAuditTrial trail = new PayrollAuditTrial
                    {
                        PATID = patID,
                        FileName = auditData.FileName,
                        UploadedDateTime = auditData.UploadedDateTime,
                        ActivityTypeID = auditData.ActivityTypeID,
                        ActivityPeriod = auditData.ActivityPeriod,
                        ActivityStatusID = 2,
                        CompanyID = auditData.CompanyID,
                        CreatedBy = auditData.CreatedBy,
                        CreatedDate = auditData.CreatedDate,
                        CreatedIP = auditData.CreatedIP,
                        RowVersion = auditData.RowVersion,
                    };
                    trail.SetModified();
                    PayrollAuditTrail.Add(trail);
                    PayrollAuditTrail.SaveChangesWithAudit();

                    PayrollAuditTrial newTrail = new PayrollAuditTrial
                    {
                        FileName = file.FileName,
                        UploadedDateTime = DateTime.Now,
                        ActivityTypeID = activityTypeID,
                        ActivityPeriod = monthId.ToString() + '-' + year.ToString(),
                        ActivityStatusID = 1
                    };

                    newTrail.SetAdded();
                    PayrollAuditTrail.Add(newTrail);
                    PayrollAuditTrail.SaveChangesWithAudit();

                    var masterEntites = PaySlipInfoRepo.Entities.Where(x => x.PATID == patID).ToList();
                    var payEntities = new List<EmployeePaySlipInfo>();

                    foreach (var masterEntite in masterEntites)
                    {
                        masterEntite.SetDeleted();
                        payEntities.Add(masterEntite);
                    }

                    PaySlipInfoRepo.AddRange(payEntities);
                    PaySlipInfoRepo.SaveChangesWithAudit();

                    List<EmployeePaySlipInfo> payRepoEntites = new List<EmployeePaySlipInfo>();

                    foreach (var payEntity in modelData.employeePaySlipInfos)
                    {
                        payEntity.PATID = newTrail.PATID;
                        payRepoEntites.Add(payEntity);
                    }
                    PaySlipInfoRepo.AddRange(payRepoEntites);
                    PaySlipInfoRepo.SaveChangesWithAudit();

                    resp.message = "File re-uploaded successfully";
                    resp.status = true;

                }

                else
                {
                    PayrollAuditTrial newTrail = new PayrollAuditTrial
                    {
                        FileName = file.FileName,
                        UploadedDateTime = DateTime.Now,
                        ActivityTypeID = activityTypeID,
                        ActivityPeriod = monthId + '-' + year,
                        ActivityStatusID = 1
                    };

                    newTrail.SetAdded();
                    PayrollAuditTrail.Add(newTrail);
                    PayrollAuditTrail.SaveChangesWithAudit();

                    List<EmployeePaySlipInfo> payRepoEntites = new List<EmployeePaySlipInfo>();

                    foreach (var payEntity in modelData.employeePaySlipInfos)
                    {
                        payEntity.PATID = newTrail.PATID;
                        payRepoEntites.Add(payEntity);
                    }
                    PaySlipInfoRepo.AddRange(payRepoEntites);
                    PaySlipInfoRepo.SaveChangesWithAudit();

                    resp.message = "File uploaded successfully";
                    resp.status = true;
                }

                unitOfWork.CommitChangesWithAudit();

            }

            return await Task.FromResult(resp);
        }

        PaySlipDataModel ValidateEmployeePaySlipExcel(ExcelWorksheet worksheet, int rowCount, string salaryMonth, string salaryYear, string secretKey, ref bool status)
        {
            PaySlipDataModel paySlipDataModel = new PaySlipDataModel();
            var employees = EmployeeRepo.GetAllList();
            status = true;
            List<EmployeePaySlipDto> paySlipDtos = new List<EmployeePaySlipDto>();
            List<EmployeePaySlipInfo> paySlipInfos = new List<EmployeePaySlipInfo>();
            for (int row = 2; row <= rowCount; row++)
            {
                string format = "MMMM-yyyy";
                string formatJoin = "dd MMMM, yyyy";

                DateTime parsedDate;
                DateTime disburseDate;
                DateTime joinDate;

                bool isValidSalaryMonthYear = DateTime.TryParseExact(worksheet.Cells[row, 1].Text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
                bool isValidDisburseDate = DateTime.TryParseExact(worksheet.Cells[row, 2].Text, formatJoin, CultureInfo.InvariantCulture, DateTimeStyles.None, out disburseDate);
                bool isValidJoinDate = DateTime.TryParseExact(worksheet.Cells[row, 8].Text, formatJoin, CultureInfo.InvariantCulture, DateTimeStyles.None, out joinDate);

                var paySlip = new EmployeePaySlipDto
                {
                    Salary_Month_Year = isValidSalaryMonthYear ? worksheet.Cells[row, 1].Text : string.Empty,
                    Disbursement_Date = isValidDisburseDate ? worksheet.Cells[row, 2].Text : string.Empty,
                    Employee_ID = !string.IsNullOrEmpty(worksheet.Cells[row, 3].Text) ? worksheet.Cells[row, 3].Text : string.Empty,
                    Designation = !string.IsNullOrEmpty(worksheet.Cells[row, 4].Text) ? worksheet.Cells[row, 4].Text : string.Empty,
                    Division = !string.IsNullOrEmpty(worksheet.Cells[row, 5].Text) ? worksheet.Cells[row, 5].Text : string.Empty,
                    Name = !string.IsNullOrEmpty(worksheet.Cells[row, 6].Text) ? worksheet.Cells[row, 6].Text : string.Empty,
                    Department = !string.IsNullOrEmpty(worksheet.Cells[row, 7].Text) ? worksheet.Cells[row, 7].Text : string.Empty,
                    Joining_Date = isValidJoinDate ? worksheet.Cells[row, 8].Text : string.Empty,
                    Basic_Salary = decimal.TryParse(worksheet.Cells[row, 9].Text, out var basic_salary) ? basic_salary : (decimal?)null,
                    House_Rent_Allowance = decimal.TryParse(worksheet.Cells[row, 10].Text, out var house_rent) ? house_rent : (decimal?)null,
                    Medical_Allowance = decimal.TryParse(worksheet.Cells[row, 11].Text, out var medical) ? medical : (decimal?)null,
                    Conveyance_Allowance = decimal.TryParse(worksheet.Cells[row, 12].Text, out var conveyance) ? conveyance : (decimal?)null,
                    Free_or_Concessional_Passage_for_Travel = decimal.TryParse(worksheet.Cells[row, 13].Text, out var travel)
                                                                        ? travel : (decimal?)null,
                    Payroll_Card_Part = decimal.TryParse(worksheet.Cells[row, 14].Text, out var payroll_card) ? payroll_card : (decimal?)null,
                    Arrear_Basic_Salary = decimal.TryParse(worksheet.Cells[row, 15].Text, out var arrear_basic) ? arrear_basic : (decimal?)null,
                    Arrear_House_Rent_Allowance = decimal.TryParse(worksheet.Cells[row, 16].Text, out var arrear_house) ? arrear_house : (decimal?)null,
                    Arrear_Medical_Allowance = decimal.TryParse(worksheet.Cells[row, 17].Text, out var arrear_medical) ? arrear_medical : (decimal?)null,
                    Arrear_Conveyance_Allowance = decimal.TryParse(worksheet.Cells[row, 18].Text, out var arrear_conveyance) ? arrear_conveyance : (decimal?)null,
                    Arrear_Free_Or_Concessional_Passage_For_Travel = decimal.TryParse(worksheet.Cells[row, 19].Text, out var arrear_travel) ? arrear_travel : (decimal?)null,
                    Total_Earnings = decimal.TryParse(worksheet.Cells[row, 20].Text, out var total_earn) ? total_earn : (decimal?)null,
                    Total_Arrears = decimal.TryParse(worksheet.Cells[row, 21].Text, out var total_arrear) ? total_arrear : (decimal?)null,
                    Income_Tax = decimal.TryParse(worksheet.Cells[row, 22].Text, out var income_tax) ? income_tax : (decimal?)null,
                    Deduction_Field_1 = decimal.TryParse(worksheet.Cells[row, 23].Text, out var deduction_1) ? deduction_1 : (decimal?)null,
                    Deduction_Field_2 = decimal.TryParse(worksheet.Cells[row, 24].Text, out var deduction_2) ? deduction_2 : (decimal?)null,
                    Total_Deductions = decimal.TryParse(worksheet.Cells[row, 25].Text, out var total_deduction) ? total_deduction : (decimal?)null,
                    Net_Payable = decimal.TryParse(worksheet.Cells[row, 26].Text, out var net_pay) ? net_pay : (decimal?)null,
                    Amount_In_Words = !string.IsNullOrEmpty(worksheet.Cells[row, 27].Text) ? worksheet.Cells[row, 27].Text : string.Empty,
                    Bank_Amount_BDT = decimal.TryParse(worksheet.Cells[row, 28].Text, out var bank_amt) ? bank_amt : (decimal?)null,
                    Wallet_Amount = decimal.TryParse(worksheet.Cells[row, 29].Text, out var wallet_amt) ? wallet_amt : (decimal?)null,
                    Cash_out_Charge = decimal.TryParse(worksheet.Cells[row, 30].Text, out var cash_out) ? cash_out : (decimal?)null,
                    Extra_Mobile_Bill_Deducted = decimal.TryParse(worksheet.Cells[row, 31].Text, out var extra_mobile_bill) ? extra_mobile_bill : (decimal?)null,

                    Market_Bonus = decimal.TryParse(worksheet.Cells[row, 32].Text, out var market_bonus) ? market_bonus : (decimal?)null,
                    Weekend_Allowance = decimal.TryParse(worksheet.Cells[row, 33].Text, out var weekend_allowance) ? weekend_allowance : (decimal?)null,
                    Festival_Holiday_Allowance = decimal.TryParse(worksheet.Cells[row, 34].Text, out var festival_holiday_allowance) ? festival_holiday_allowance : (decimal?)null,
                    Saturday_Allowance = decimal.TryParse(worksheet.Cells[row, 35].Text, out var saturday_allowance) ? saturday_allowance : (decimal?)null,
                    Tax_Support = decimal.TryParse(worksheet.Cells[row, 36].Text, out var tax_support) ? tax_support : (decimal?)null,
                    Festival_Bonus_Arrear = decimal.TryParse(worksheet.Cells[row, 37].Text, out var festival_bonus_arrear) ? festival_bonus_arrear : (decimal?)null,
                    Salary_Advance = decimal.TryParse(worksheet.Cells[row, 38].Text, out var salary_advance) ? salary_advance : (decimal?)null,
                    Tax_Refund = decimal.TryParse(worksheet.Cells[row, 39].Text, out var tax_refund) ? tax_refund : (decimal?)null,
                    Laptop_Repairing_Cost_Deducted = decimal.TryParse(worksheet.Cells[row, 40].Text, out var laptop_repairing_cost_deducted) ? laptop_repairing_cost_deducted : (decimal?)null,
                    Provident_Fund = decimal.TryParse(worksheet.Cells[row, 41].Text, out var provident_fund) ? provident_fund : (decimal?)null,

                    Mobile_Bill_Adjustment = decimal.TryParse(worksheet.Cells[row, 42].Text, out var mobile_bill_adjustment) ? mobile_bill_adjustment : (decimal?)null,
                    Car_Allowance = decimal.TryParse(worksheet.Cells[row, 43].Text, out var car_allowance) ? car_allowance : (decimal?)null,
                };

                // Check for duplicate Employee_ID in the list
                if (paySlipDtos.Any(p => p.Employee_ID == paySlip.Employee_ID && !string.IsNullOrEmpty(paySlip.Employee_ID)))
                {
                    status = false;
                    paySlipDataModel.IsValid = false;
                    paySlipDataModel.message = $"Duplicate Employee found for Employee_ID: {paySlip.Employee_ID}";
                    return paySlipDataModel;
                }

                #region Field Validation

                if (!isValidSalaryMonthYear)
                {
                    status = false;
                    paySlip.Validation_Result = "Invalid Salary Month, Year,";
                }

                if (!isValidDisburseDate)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Disbursement Date,";
                }

                if (!isValidJoinDate)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Joining Date,";
                }

                var emp = employees.Where(e => e.EmployeeCode == paySlip.Employee_ID.ToString()).ToList();
                if (emp == null || emp.Count == 0 || string.IsNullOrEmpty(paySlip.Employee_ID))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Employee ID,";
                }

                if (string.IsNullOrEmpty(paySlip.Designation))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Designation,";
                }

                if (string.IsNullOrEmpty(paySlip.Division))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Division,";
                }

                if (string.IsNullOrEmpty(paySlip.Name))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Name,";
                }

                if (string.IsNullOrEmpty(paySlip.Department))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Department,";
                }

                if (paySlip.Basic_Salary.IsNull() || paySlip.Basic_Salary < 0 || paySlip.Basic_Salary.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Basic Salary,";
                }

                if (paySlip.House_Rent_Allowance.IsNull() || paySlip.House_Rent_Allowance < 0 || paySlip.House_Rent_Allowance.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid House Rent Allowance,";
                }

                if (paySlip.Medical_Allowance.IsNull() || paySlip.Medical_Allowance < 0 || paySlip.Medical_Allowance.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Medical Allowance,";
                }

                if (paySlip.Conveyance_Allowance == null || paySlip.Conveyance_Allowance.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Conveyance Allowance,";
                }

                if (paySlip.Free_or_Concessional_Passage_for_Travel.IsNull() || paySlip.Free_or_Concessional_Passage_for_Travel < 0 || paySlip.Free_or_Concessional_Passage_for_Travel.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Free or Concessional Passage for Travel,";
                }

                if (paySlip.Payroll_Card_Part.IsNull() || paySlip.Payroll_Card_Part < 0 || paySlip.Payroll_Card_Part.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Payroll Card Part,";
                }

                if (paySlip.Mobile_Bill_Adjustment.IsNull() || paySlip.Mobile_Bill_Adjustment < 0 || paySlip.Mobile_Bill_Adjustment.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Mobile Bill Adjustment,";
                }

                if (paySlip.Car_Allowance.IsNull() || paySlip.Car_Allowance < 0 || paySlip.Car_Allowance.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Car Allowance,";
                }

                if (paySlip.Arrear_Basic_Salary.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Arrear Basic Salary,";
                }

                if (paySlip.Arrear_House_Rent_Allowance.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Arrear House Rent Allowance,";
                }
                if (paySlip.Arrear_Medical_Allowance.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Arrear Medical Allowance,";
                }
                if (paySlip.Arrear_Conveyance_Allowance.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Arrear Con Allowance,";
                }
                if (paySlip.Arrear_Free_Or_Concessional_Passage_For_Travel.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Arrear Free or Concessional,";
                }
                if (paySlip.Income_Tax.IsNull() || paySlip.Income_Tax < 0 || paySlip.Income_Tax.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Income Tax,";
                }

                if (paySlip.Deduction_Field_1.IsNull() || paySlip.Deduction_Field_1 < 0 || paySlip.Deduction_Field_1.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Deduction Field 1,";
                }
                if (paySlip.Deduction_Field_2.IsNull() || paySlip.Deduction_Field_2 < 0 || paySlip.Deduction_Field_2.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Deduction Field 2,";
                }
                if (paySlip.Total_Earnings.IsNull() || paySlip.Total_Earnings < 0 || paySlip.Total_Earnings.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Total Earnings,";
                }
                if (paySlip.Total_Arrears.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Total Arrears,";
                }
                if (paySlip.Total_Deductions.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Total Deductions,";
                }
                if (paySlip.Net_Payable.IsNull() || paySlip.Net_Payable < 0 || paySlip.Net_Payable.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Net Payable,";
                }
                if (string.IsNullOrEmpty(paySlip.Amount_In_Words))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Amount in words,";
                }
                if (paySlip.Bank_Amount_BDT.IsNull() || paySlip.Bank_Amount_BDT < 0 || paySlip.Bank_Amount_BDT.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Bank Amount (BDT),";
                }
                if (paySlip.Wallet_Amount.IsNull() || paySlip.Wallet_Amount < 0 || paySlip.Wallet_Amount.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Wallet Amount,";
                }
                if (paySlip.Cash_out_Charge.IsNull() || paySlip.Cash_out_Charge < 0 || paySlip.Cash_out_Charge.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Cash out Charge";
                }

                if (!paySlip.Salary_Month_Year.Contains(salaryMonth + '-' + salaryYear))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Month";
                }
                if ((paySlip.Extra_Mobile_Bill_Deducted.IsNotNull() && paySlip.Extra_Mobile_Bill_Deducted.ToString().IsNumeric() == false) || paySlip.Extra_Mobile_Bill_Deducted < 0)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Extra Mobile Bill Deducted";
                }
                ////////
                if ((paySlip.Market_Bonus.IsNotNull() && paySlip.Market_Bonus.ToString().IsNumeric() == false) || paySlip.Market_Bonus < 0)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Market Bonus";
                }
                if ((paySlip.Weekend_Allowance.IsNotNull() && paySlip.Weekend_Allowance.ToString().IsNumeric() == false) || paySlip.Weekend_Allowance < 0)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Weekend Allowance";
                }
                if ((paySlip.Festival_Holiday_Allowance.IsNotNull() && paySlip.Festival_Holiday_Allowance.ToString().IsNumeric() == false) || paySlip.Festival_Holiday_Allowance < 0)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Festival Holiday Allowance";
                }
                if ((paySlip.Saturday_Allowance.IsNotNull() && paySlip.Saturday_Allowance.ToString().IsNumeric() == false) || paySlip.Saturday_Allowance < 0)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Saturday Allowance";
                }
                if ((paySlip.Tax_Support.IsNotNull() && paySlip.Tax_Support.ToString().IsNumeric() == false) || paySlip.Tax_Support < 0)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Tax Support";
                }
                if ((paySlip.Festival_Bonus_Arrear.IsNotNull() && paySlip.Festival_Bonus_Arrear.ToString().IsNumeric() == false) || paySlip.Festival_Bonus_Arrear < 0)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Festival Bonus Arrear";
                }
                if ((paySlip.Salary_Advance.IsNotNull() && paySlip.Salary_Advance.ToString().IsNumeric() == false) || paySlip.Salary_Advance < 0)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Salary Advance";
                }
                if ((paySlip.Tax_Refund.IsNotNull() && paySlip.Tax_Refund.ToString().IsNumeric() == false) || paySlip.Tax_Refund < 0)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Tax Refund";
                }
                if ((paySlip.Laptop_Repairing_Cost_Deducted.IsNotNull() && paySlip.Laptop_Repairing_Cost_Deducted.ToString().IsNumeric() == false) || paySlip.Laptop_Repairing_Cost_Deducted < 0)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Laptop Repairing Cost Deducted";
                }
                if ((paySlip.Provident_Fund.IsNotNull() && paySlip.Provident_Fund.ToString().IsNumeric() == false) || paySlip.Provident_Fund < 0)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Provident Fund";
                }
                if (paySlip.Extra_Mobile_Bill_Deducted.IsNull() || paySlip.Extra_Mobile_Bill_Deducted.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Extra Mobile Bill Deducted,";
                }
                ////////



                if (paySlip.Validation_Result.IsNullOrEmpty())
                {
                    paySlip.Validation_Result = "Passed";
                }


                paySlipDtos.Add(paySlip);

                #endregion

                if (status)
                {
                    EmployeePaySlipInfo pay = new EmployeePaySlipInfo
                    {
                        SalaryMonth = paySlip.Salary_Month_Year,
                        DisbursementDate = Convert.ToDateTime(paySlip.Disbursement_Date),
                        EmployeeID = emp[0].EmployeeID,
                        EmployeeCode = emp[0].EmployeeCode,
                        Designation = paySlip.Designation,
                        Division = paySlip.Division,
                        EmployeeName = emp[0].FullName,
                        Department = paySlip.Department,
                        BasicSalary = PayrollEncrypt(Convert.ToString(paySlip.Basic_Salary), secretKey),
                        HouseRent = PayrollEncrypt(Convert.ToString(paySlip.House_Rent_Allowance), secretKey),
                        MedicalAllowance = PayrollEncrypt(Convert.ToString(paySlip.Medical_Allowance), secretKey),
                        ConveyanceAllowance = PayrollEncrypt(Convert.ToString(paySlip.Conveyance_Allowance), secretKey),
                        PassageForTravel = PayrollEncrypt(Convert.ToString(paySlip.Free_or_Concessional_Passage_for_Travel), secretKey),
                        PayrollCardPart = PayrollEncrypt(Convert.ToString(paySlip.Payroll_Card_Part), secretKey),
                        ArrearBasicSalary = PayrollEncrypt(Convert.ToString(paySlip.Arrear_Basic_Salary), secretKey),
                        ArrearHouseRent = PayrollEncrypt(Convert.ToString(paySlip.Arrear_House_Rent_Allowance), secretKey),
                        ArrearMedicalAllowance = PayrollEncrypt(Convert.ToString(paySlip.Arrear_Medical_Allowance), secretKey),
                        ArrearConveyanceAllowance = PayrollEncrypt(Convert.ToString(paySlip.Arrear_Conveyance_Allowance), secretKey),
                        ArrearPassageForTravel = PayrollEncrypt(Convert.ToString(paySlip.Arrear_Free_Or_Concessional_Passage_For_Travel), secretKey),
                        TotalEarnings = PayrollEncrypt(Convert.ToString(paySlip.Total_Earnings), secretKey),
                        TotalArrears = PayrollEncrypt(Convert.ToString(paySlip.Total_Arrears), secretKey),
                        TotalDeductions = PayrollEncrypt(Convert.ToString(paySlip.Total_Deductions), secretKey),
                        IncomeTax = PayrollEncrypt(Convert.ToString(paySlip.Income_Tax), secretKey),
                        DeductionField1 = PayrollEncrypt(Convert.ToString(paySlip.Deduction_Field_1), secretKey),
                        DeductionField2 = PayrollEncrypt(Convert.ToString(paySlip.Deduction_Field_2), secretKey),
                        NetPayable = PayrollEncrypt(Convert.ToString(paySlip.Net_Payable), secretKey),
                        AmountInWords = PayrollEncrypt(Convert.ToString(paySlip.Amount_In_Words), secretKey),
                        BankAmount = PayrollEncrypt(Convert.ToString(paySlip.Bank_Amount_BDT), secretKey),
                        WalletAmount = PayrollEncrypt(Convert.ToString(paySlip.Wallet_Amount), secretKey),
                        CashOutCharge = PayrollEncrypt(Convert.ToString(paySlip.Cash_out_Charge), secretKey),
                        MobileAllowance = PayrollEncrypt(Convert.ToString(paySlip.Extra_Mobile_Bill_Deducted), secretKey),
                        MarketBonus = PayrollEncrypt(Convert.ToString(paySlip.Market_Bonus), secretKey),
                        WeekendAllowance = PayrollEncrypt(Convert.ToString(paySlip.Weekend_Allowance), secretKey),
                        FestivalHolidayAllowance = PayrollEncrypt(Convert.ToString(paySlip.Festival_Holiday_Allowance), secretKey),
                        SaturdayAllowance = PayrollEncrypt(Convert.ToString(paySlip.Saturday_Allowance), secretKey),
                        TaxSupport = PayrollEncrypt(Convert.ToString(paySlip.Tax_Support), secretKey),
                        FestivalBonusArrear = PayrollEncrypt(Convert.ToString(paySlip.Festival_Bonus_Arrear), secretKey),
                        SalaryAdvance = PayrollEncrypt(Convert.ToString(paySlip.Salary_Advance), secretKey),
                        TaxRefund = PayrollEncrypt(Convert.ToString(paySlip.Tax_Refund), secretKey),
                        LaptopRepairingCostDeducted = PayrollEncrypt(Convert.ToString(paySlip.Laptop_Repairing_Cost_Deducted), secretKey),
                        ProvidentFund = PayrollEncrypt(Convert.ToString(paySlip.Provident_Fund), secretKey),
                        MobileBillAdjustment = PayrollEncrypt(Convert.ToString(paySlip.Mobile_Bill_Adjustment), secretKey),
                        CarAllowance = PayrollEncrypt(Convert.ToString(paySlip.Car_Allowance), secretKey)
                    };
                    pay.SetAdded();
                    paySlipInfos.Add(pay);
                }
            }

            paySlipDataModel.paySlipDtos = paySlipDtos;
            paySlipDataModel.employeePaySlipInfos = paySlipInfos;
            return paySlipDataModel;
        }
        PaySlipDataModel ValidateEmployeePaySlipCSV(List<PaySlipCSVDto> dataList, string salaryMonth, string salaryYear, string secretKey, ref bool status)
        {
            PaySlipDataModel paySlipDataModel = new PaySlipDataModel();
            var employees = EmployeeRepo.GetAllList();
            status = true;
            List<EmployeePaySlipDto> paySlipDtos = new List<EmployeePaySlipDto>();
            List<EmployeePaySlipInfo> paySlipInfos = new List<EmployeePaySlipInfo>();

            string format = "MMMM-yyyy";
            string formatJoin = "dd MMMM, yyyy";

            DateTime parsedDate;
            DateTime disburseDate;
            DateTime joinDate;


            foreach (var d in dataList)
            {
                bool isValidSalaryMonthYear = DateTime.TryParseExact(d.Salary_Month_Year, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
                bool isValidDisburseDate = DateTime.TryParseExact(d.Disbursement_Date, formatJoin, CultureInfo.InvariantCulture, DateTimeStyles.None, out disburseDate);
                bool isValidJoinDate = DateTime.TryParseExact(d.Joining_Date, formatJoin, CultureInfo.InvariantCulture, DateTimeStyles.None, out joinDate);

                var paySlip = new EmployeePaySlipDto
                {
                    Salary_Month_Year = isValidSalaryMonthYear ? d.Salary_Month_Year : string.Empty,
                    Disbursement_Date = isValidDisburseDate ? d.Disbursement_Date : string.Empty,
                    Employee_ID = !string.IsNullOrEmpty(d.Employee_ID) ? d.Employee_ID : string.Empty,
                    Designation = !string.IsNullOrEmpty(d.Designation) ? d.Designation : string.Empty,
                    Division = !string.IsNullOrEmpty(d.Division) ? d.Division : string.Empty,
                    Name = !string.IsNullOrEmpty(d.Name) ? d.Name : string.Empty,
                    Department = !string.IsNullOrEmpty(d.Department) ? d.Department : string.Empty,
                    Joining_Date = isValidJoinDate ? d.Joining_Date : string.Empty,
                    Basic_Salary = decimal.TryParse(d.Basic_Salary, out var basic_salary) ? basic_salary : (decimal?)null,
                    House_Rent_Allowance = decimal.TryParse(d.House_Rent_Allowance, out var house_rent) ? house_rent : (decimal?)null,
                    Medical_Allowance = decimal.TryParse(d.Medical_Allowance, out var medical) ? medical : (decimal?)null,
                    Conveyance_Allowance = decimal.TryParse(d.Conveyance_Allowance, out var conveyance) ? conveyance : (decimal?)null,
                    Free_or_Concessional_Passage_for_Travel = decimal.TryParse(d.Free_Or_Concessional_Passage_For_Travel, out var travel)
                                                                       ? travel : (decimal?)null,
                    Payroll_Card_Part = decimal.TryParse(d.Payroll_Card_Part, out var payroll_card) ? payroll_card : (decimal?)null,
                    Arrear_Basic_Salary = decimal.TryParse(d.Arrear_Basic_Salary, out var arrear_basic) ? arrear_basic : (decimal?)null,
                    Arrear_House_Rent_Allowance = decimal.TryParse(d.Arrear_House_Rent_Allowance, out var arrear_house) ? arrear_house : (decimal?)null,
                    Arrear_Medical_Allowance = decimal.TryParse(d.Arrear_Medical_Allowance, out var arrear_medical) ? arrear_medical : (decimal?)null,
                    Arrear_Conveyance_Allowance = decimal.TryParse(d.Arrear_Conveyance_Allowance, out var arrear_conveyance) ? arrear_conveyance : (decimal?)null,
                    Arrear_Free_Or_Concessional_Passage_For_Travel = decimal.TryParse(d.Arrear_Free_Or_Concessional_Passage_For_Travel, out var arrear_travel) ? arrear_travel : (decimal?)null,
                    Total_Earnings = decimal.TryParse(d.Total_Earnings, out var total_earn) ? total_earn : (decimal?)null,
                    Total_Arrears = decimal.TryParse(d.Total_Arrears, out var total_arrear) ? total_arrear : (decimal?)null,
                    Income_Tax = decimal.TryParse(d.Income_Tax, out var income_tax) ? income_tax : (decimal?)null,
                    Deduction_Field_1 = decimal.TryParse(d.Deduction_Field_1, out var deduction_1) ? deduction_1 : (decimal?)null,
                    Deduction_Field_2 = decimal.TryParse(d.Deduction_Field_2, out var deduction_2) ? deduction_2 : (decimal?)null,
                    Total_Deductions = decimal.TryParse(d.Total_Deductions, out var total_deduction) ? total_deduction : (decimal?)null,
                    Net_Payable = decimal.TryParse(d.Net_Payable, out var net_pay) ? net_pay : (decimal?)null,
                    Amount_In_Words = !string.IsNullOrEmpty(d.Amount_In_Words) ? d.Amount_In_Words : string.Empty,
                    Bank_Amount_BDT = decimal.TryParse(d.Bank_Amount_BDT, out var bank_amt) ? bank_amt : (decimal?)null,
                    Wallet_Amount = decimal.TryParse(d.Wallet_Amount, out var wallet_amt) ? wallet_amt : (decimal?)null,
                    Cash_out_Charge = decimal.TryParse(d.Cash_Out_Charge, out var cash_out) ? cash_out : (decimal?)null,
                    Extra_Mobile_Bill_Deducted = decimal.TryParse(d.Extra_Mobile_Bill_Deducted, out var extra_mobile_bill) ? extra_mobile_bill : (decimal?)null,

                    Market_Bonus = decimal.TryParse(d.Market_Bonus, out var market_bonus) ? market_bonus : (decimal?)null,
                    Weekend_Allowance = decimal.TryParse(d.Weekend_Allowance, out var weekend_allowance) ? weekend_allowance : (decimal?)null,
                    Festival_Holiday_Allowance = decimal.TryParse(d.Festival_Holiday_Allowance, out var festival_holiday_allowance) ? festival_holiday_allowance : (decimal?)null,
                    Saturday_Allowance = decimal.TryParse(d.Saturday_Allowance, out var saturday_allowance) ? saturday_allowance : (decimal?)null,
                    Tax_Support = decimal.TryParse(d.Tax_Support, out var tax_support) ? tax_support : (decimal?)null,
                    Festival_Bonus_Arrear = decimal.TryParse(d.Festival_Bonus_Arrear, out var festival_bonus_arrear) ? festival_bonus_arrear : (decimal?)null,
                    Salary_Advance = decimal.TryParse(d.Salary_Advance, out var salary_advance) ? salary_advance : (decimal?)null,
                    Tax_Refund = decimal.TryParse(d.Tax_Refund, out var tax_refund) ? tax_refund : (decimal?)null,
                    Laptop_Repairing_Cost_Deducted = decimal.TryParse(d.Laptop_Repairing_Cost_Deducted, out var laptop_repairing_cost_deducted) ? laptop_repairing_cost_deducted : (decimal?)null,
                    Provident_Fund = decimal.TryParse(d.Provident_Fund, out var provident_fund) ? provident_fund : (decimal?)null,

                    Mobile_Bill_Adjustment = decimal.TryParse(d.Mobile_Bill_Adjustment, out var mobile_bill_adjustment) ? mobile_bill_adjustment : (decimal?)null,
                    Car_Allowance = decimal.TryParse(d.Car_Allowance, out var car_allowance) ? car_allowance : (decimal?)null,
                };

                //if (paySlipDtos.Any(p => p.Employee_ID == paySlip.Employee_ID && !string.IsNullOrEmpty(paySlip.Employee_ID)))
                //{
                //    status = false;
                //    paySlip.Validation_Result = $"Duplicate Employee found for Employee_ID: {paySlip.Employee_ID}";
                //}

                if (paySlipDtos.Any(p => p.Employee_ID == paySlip.Employee_ID && !string.IsNullOrEmpty(paySlip.Employee_ID)))
                {
                    status = false;
                    paySlipDataModel.IsValid = false;
                    paySlipDataModel.message = $"Duplicate Employee found for Employee_ID: {paySlip.Employee_ID}";
                    return paySlipDataModel;
                }

                #region Field Validation

                if (!isValidSalaryMonthYear)
                {
                    status = false;
                    paySlip.Validation_Result = "Invalid Salary Month, Year,";
                }

                if (!isValidDisburseDate)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Disbursement Date,";
                }

                if (!isValidJoinDate)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Joining Date,";
                }

                var emp = employees.Where(e => e.EmployeeCode == paySlip.Employee_ID.ToString()).ToList();
                if (emp == null || emp.Count == 0 || string.IsNullOrEmpty(paySlip.Employee_ID))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Employee ID";
                }

                if (string.IsNullOrEmpty(paySlip.Designation))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Designation,";
                }

                if (string.IsNullOrEmpty(paySlip.Division))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Division,";
                }

                if (string.IsNullOrEmpty(paySlip.Name))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Name,";
                }

                if (string.IsNullOrEmpty(paySlip.Department))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Department,";
                }

                if (paySlip.Basic_Salary.IsNull() || paySlip.Basic_Salary < 0 || paySlip.Basic_Salary.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Basic Salary,";
                }


                if (paySlip.House_Rent_Allowance < 0 || paySlip.House_Rent_Allowance.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid House Rent Allowance,";
                }

                if (paySlip.Medical_Allowance < 0 || paySlip.Medical_Allowance.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Medical Allowance,";
                }

                if (paySlip.Conveyance_Allowance == null)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Conveyance Allowance,";
                }

                if (paySlip.Free_or_Concessional_Passage_for_Travel < 0 || paySlip.Free_or_Concessional_Passage_for_Travel.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Free or Concessional Passage for Travel,";
                }

                if (paySlip.Payroll_Card_Part < 0 || paySlip.Payroll_Card_Part.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Payroll Card Part,";
                }

                if (paySlip.Mobile_Bill_Adjustment.IsNull() || paySlip.Mobile_Bill_Adjustment < 0 || paySlip.Mobile_Bill_Adjustment.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Mobile Bill Adjustment,";
                }

                if (paySlip.Car_Allowance.IsNull() || paySlip.Car_Allowance < 0 || paySlip.Car_Allowance.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Car Allowance,";
                }

                if (paySlip.Arrear_Basic_Salary < 0 || paySlip.Arrear_Basic_Salary.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Arrear Basic Salary,";
                }

                if (paySlip.Arrear_House_Rent_Allowance < 0 || paySlip.Arrear_House_Rent_Allowance.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Arrear House Rent Allowance,";
                }
                if (paySlip.Arrear_Medical_Allowance < 0 || paySlip.Arrear_Medical_Allowance.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Arrear Medical Allowance,";
                }
                if (paySlip.Arrear_Conveyance_Allowance < 0 || paySlip.Arrear_Conveyance_Allowance.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Arrear Con Allowance,";
                }
                if (paySlip.Arrear_Free_Or_Concessional_Passage_For_Travel < 0 || paySlip.Arrear_Free_Or_Concessional_Passage_For_Travel.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Arrear Free or Concessional,";
                }
                if (paySlip.Income_Tax < 0 || paySlip.Income_Tax.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Income Tax,";
                }

                if (paySlip.Deduction_Field_1 < 0 || paySlip.Deduction_Field_1.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Deduction Field 1,";
                }
                if (paySlip.Deduction_Field_2 < 0 || paySlip.Deduction_Field_2.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Deduction Field 2,";
                }
                if (paySlip.Total_Earnings.IsNull() || paySlip.Total_Earnings < 0 || paySlip.Total_Earnings.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Total Earnings,";
                }
                if (paySlip.Total_Arrears.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Total Arrears,";
                }
                if (paySlip.Total_Deductions.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Total Deductions,";
                }
                if (paySlip.Net_Payable < 0 || paySlip.Net_Payable.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Net Payable,";
                }
                if (string.IsNullOrEmpty(paySlip.Amount_In_Words))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Amount in words,";
                }
                if (paySlip.Bank_Amount_BDT.IsNull() || paySlip.Bank_Amount_BDT < 0 || paySlip.Bank_Amount_BDT.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Bank Amount (BDT),";
                }
                if (paySlip.Wallet_Amount.IsNull() || paySlip.Wallet_Amount < 0 || paySlip.Wallet_Amount.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Wallet Amount,";
                }
                if (paySlip.Cash_out_Charge.IsNull() || paySlip.Cash_out_Charge < 0 || paySlip.Cash_out_Charge.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Cash out Charge";
                }



                if (!paySlip.Salary_Month_Year.Contains(salaryMonth + '-' + salaryYear))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Month";
                }
                if (paySlip.Extra_Mobile_Bill_Deducted < 0 || paySlip.Extra_Mobile_Bill_Deducted.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Extra Mobile Bill Deducted";
                }
                if (paySlip.Market_Bonus.IsNotNull() && paySlip.Market_Bonus.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Market Bonus";
                }
                if (paySlip.Weekend_Allowance.IsNotNull() && paySlip.Weekend_Allowance.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Weekend Allowance";
                }
                if (paySlip.Festival_Holiday_Allowance.IsNotNull() && paySlip.Festival_Holiday_Allowance.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Festival Holiday Allowance";
                }
                if (paySlip.Saturday_Allowance.IsNotNull() && paySlip.Saturday_Allowance.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Saturday Allowance";
                }
                if (paySlip.Tax_Support.IsNotNull() && paySlip.Tax_Support.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Tax Support";
                }
                if (paySlip.Festival_Bonus_Arrear.IsNotNull() && paySlip.Festival_Bonus_Arrear.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Festival Bonus Arrear";
                }
                if (paySlip.Salary_Advance.IsNotNull() && paySlip.Salary_Advance.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Salary Advance";
                }
                if (paySlip.Tax_Refund.IsNotNull() && paySlip.Tax_Refund.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Tax Refund";
                }
                if (paySlip.Laptop_Repairing_Cost_Deducted.IsNotNull() && paySlip.Laptop_Repairing_Cost_Deducted.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Laptop Repairing Cost Deducted";
                }
                if (paySlip.Provident_Fund.IsNotNull() && paySlip.Provident_Fund.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Provident Fund";
                }
                if (paySlip.Validation_Result.IsNullOrEmpty())
                {
                    paySlip.Validation_Result = "Passed";
                }


                paySlipDtos.Add(paySlip);

                #endregion

                if (status)
                {
                    EmployeePaySlipInfo pay = new EmployeePaySlipInfo
                    {
                        SalaryMonth = paySlip.Salary_Month_Year,
                        DisbursementDate = Convert.ToDateTime(paySlip.Disbursement_Date),
                        EmployeeID = emp[0].EmployeeID,
                        EmployeeCode = emp[0].EmployeeCode,
                        Designation = paySlip.Designation,
                        Division = paySlip.Division,
                        EmployeeName = emp[0].FullName,
                        Department = paySlip.Department,
                        BasicSalary = PayrollEncrypt(Convert.ToString(paySlip.Basic_Salary), secretKey),
                        HouseRent = PayrollEncrypt(Convert.ToString(paySlip.House_Rent_Allowance), secretKey),
                        MedicalAllowance = PayrollEncrypt(Convert.ToString(paySlip.Medical_Allowance), secretKey),
                        ConveyanceAllowance = PayrollEncrypt(Convert.ToString(paySlip.Conveyance_Allowance), secretKey),
                        PassageForTravel = PayrollEncrypt(Convert.ToString(paySlip.Free_or_Concessional_Passage_for_Travel), secretKey),
                        PayrollCardPart = PayrollEncrypt(Convert.ToString(paySlip.Payroll_Card_Part), secretKey),
                        ArrearBasicSalary = PayrollEncrypt(Convert.ToString(paySlip.Arrear_Basic_Salary), secretKey),
                        ArrearHouseRent = PayrollEncrypt(Convert.ToString(paySlip.Arrear_House_Rent_Allowance), secretKey),
                        ArrearMedicalAllowance = PayrollEncrypt(Convert.ToString(paySlip.Arrear_Medical_Allowance), secretKey),
                        ArrearConveyanceAllowance = PayrollEncrypt(Convert.ToString(paySlip.Arrear_Conveyance_Allowance), secretKey),
                        ArrearPassageForTravel = PayrollEncrypt(Convert.ToString(paySlip.Arrear_Free_Or_Concessional_Passage_For_Travel), secretKey),
                        TotalEarnings = PayrollEncrypt(Convert.ToString(paySlip.Total_Earnings), secretKey),
                        TotalArrears = PayrollEncrypt(Convert.ToString(paySlip.Total_Arrears), secretKey),
                        TotalDeductions = PayrollEncrypt(Convert.ToString(paySlip.Total_Deductions), secretKey),
                        IncomeTax = PayrollEncrypt(Convert.ToString(paySlip.Income_Tax), secretKey),
                        DeductionField1 = PayrollEncrypt(Convert.ToString(paySlip.Deduction_Field_1), secretKey),
                        DeductionField2 = PayrollEncrypt(Convert.ToString(paySlip.Deduction_Field_2), secretKey),
                        NetPayable = PayrollEncrypt(Convert.ToString(paySlip.Net_Payable), secretKey),
                        AmountInWords = PayrollEncrypt(Convert.ToString(paySlip.Amount_In_Words), secretKey),
                        BankAmount = PayrollEncrypt(Convert.ToString(paySlip.Bank_Amount_BDT), secretKey),
                        WalletAmount = PayrollEncrypt(Convert.ToString(paySlip.Wallet_Amount), secretKey),
                        CashOutCharge = PayrollEncrypt(Convert.ToString(paySlip.Cash_out_Charge), secretKey),
                        MobileAllowance = PayrollEncrypt(Convert.ToString(paySlip.Extra_Mobile_Bill_Deducted), secretKey),
                        MarketBonus = PayrollEncrypt(Convert.ToString(paySlip.Market_Bonus), secretKey),
                        WeekendAllowance = PayrollEncrypt(Convert.ToString(paySlip.Weekend_Allowance), secretKey),
                        FestivalHolidayAllowance = PayrollEncrypt(Convert.ToString(paySlip.Festival_Holiday_Allowance), secretKey),
                        SaturdayAllowance = PayrollEncrypt(Convert.ToString(paySlip.Saturday_Allowance), secretKey),
                        TaxSupport = PayrollEncrypt(Convert.ToString(paySlip.Tax_Support), secretKey),
                        FestivalBonusArrear = PayrollEncrypt(Convert.ToString(paySlip.Festival_Bonus_Arrear), secretKey),
                        SalaryAdvance = PayrollEncrypt(Convert.ToString(paySlip.Salary_Advance), secretKey),
                        TaxRefund = PayrollEncrypt(Convert.ToString(paySlip.Tax_Refund), secretKey),
                        LaptopRepairingCostDeducted = PayrollEncrypt(Convert.ToString(paySlip.Laptop_Repairing_Cost_Deducted), secretKey),
                        ProvidentFund = PayrollEncrypt(Convert.ToString(paySlip.Provident_Fund), secretKey),

                        MobileBillAdjustment = PayrollEncrypt(Convert.ToString(paySlip.Mobile_Bill_Adjustment), secretKey),
                        CarAllowance = PayrollEncrypt(Convert.ToString(paySlip.Car_Allowance), secretKey),
                    };
                    pay.SetAdded();
                    paySlipInfos.Add(pay);
                }
            }

            paySlipDataModel.paySlipDtos = paySlipDtos;
            paySlipDataModel.employeePaySlipInfos = paySlipInfos;
            return paySlipDataModel;
        }


        public async Task<GenericResponse<RegularIncentiveDto>> UploadRegularIncentive(int patID, int activityTypeID, string periodID, int year, IFormFile file)
        {
            string fileType = file.FileName.Split('.')[1];
            var secretKey = string.IsNullOrEmpty(Convert.ToString(Config["AppSettings:EncSecret"])) ? string.Empty
                             : Config["AppSettings:EncSecret"];

            GenericResponse<RegularIncentiveDto> resp = new GenericResponse<RegularIncentiveDto>();
            PayrollAuditTrial auditData = new PayrollAuditTrial();
            RegularIncentiveDataModel modelData = new RegularIncentiveDataModel();

            if (patID == 0 && activityTypeID > 0 && !string.IsNullOrEmpty(periodID) && year > 0)
            {
                var auditTrail = PayrollAuditTrail.FirstOrDefault(a => a.ActivityTypeID == activityTypeID
                                                        && a.ActivityPeriod == periodID + '-' + year.ToString() && a.ActivityStatusID == 1);
                if (!auditTrail.IsNullOrDbNull())
                {
                    resp.status = false;
                    //resp.message = "Data was already uploaded for this month.";
                    resp.message = "Files for this month has been already uploaded. Please use Audit Trail for re-upload.";
                    return resp;
                }

            }
            if (patID > 0)
            {
                var auditTrail = PayrollAuditTrail.FirstOrDefault(a => a.PATID == patID && a.ActivityTypeID == activityTypeID
                                                        && a.ActivityPeriod == periodID + '-' + year.ToString());

                if (auditTrail.IsNullOrDbNull())
                {
                    resp.status = false;
                    resp.message = "Invalid file uploaded.";
                    return resp;
                }

                auditData = auditTrail;
            }
            string[] excelValidCols = { "Incentive_Type_Year", "Disbursement_Date",
                                        "Employee_ID", "Designation",
                                        "Division", "Name", "Email", "Joining_Date",
                                        "Particulars_1","Basic_Entitlement_1",
                                        "Particulars_2","Basic_Entitlement_2",
                                        "Particulars_3","Basic_Entitlement_3",
                                        "Particulars_4","Basic_Entitlement_4",
                                        "Eligible_Bonus_BDT","Eligible_Bonus_Total",
                                        "Income_Tax","Total_Deduction","Net_Payable",
                                        "Amount_In_Words","Bank_Amount_BDT",
                                        "Particulars_5", "Performance_rating_1", 
                                        "Particulars_6", "Performance_rating_2"
                                      };

            if (fileType.Equals("xlsx"))
            {
                using var memoryStream = new MemoryStream();
                file.CopyTo(memoryStream);
                var bytes = memoryStream.ToArray();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(new MemoryStream(bytes)))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;
                    List<string> invalidColName = new List<string>();
                    int columnCount = worksheet.Dimension.Columns;
                    for (int col = 1; col <= columnCount; col++)
                    {
                        var columnName = worksheet.Cells[1, col].Text;
                        var isOK = excelValidCols[col - 1].Trim().ToLower().Equals(columnName.Trim().ToLower())
                                    ? true : false;
                        if (!isOK)
                            invalidColName.Add(columnName);
                    }

                    if (invalidColName.Count > 0)
                    {
                        resp.status = false;
                        resp.message = "Invalid file column header.";
                        return resp;
                    }

                    bool validationStatus = true;
                    modelData = ValidateRegularIncentiveExcel(worksheet, rowCount, year.ToString(), periodID, secretKey, ref validationStatus);

                    if (modelData.IsValid == false)
                    {
                        resp.status = false;
                        resp.message = modelData.message;
                        return resp;
                    }

                    if (!validationStatus)
                    {
                        resp.status = false;
                        resp.message = "Invalid file data.";
                        resp.data = modelData.regularIncentiveDtos;
                        return resp;
                    }
                }
            }

            else if (fileType.Equals("csv"))
            {

                List<string> headrs = new List<string>();


                var fileData = new GenericParse<RegularIncentiveDto>().ConvertCsvToList(file, ref headrs, excelValidCols);

                List<string> invalidColName = new List<string>();
                for (int col = 0; col <= headrs.Count() - 1; col++)
                {
                    var columnName = headrs[col];
                    var isOK = excelValidCols[col].Trim().ToLower().Equals(columnName.Trim().ToLower())
                                   ? true : false;
                    if (!isOK)
                        invalidColName.Add(columnName);
                }

                if (invalidColName.Count > 0)
                {
                    resp.status = false;
                    resp.message = "Invalid file column header.";
                    return resp;
                }

                bool validationStatus = true;
                modelData = ValidateRegularIncentiveCSV(fileData, year, periodID, secretKey, ref validationStatus);

                if (!validationStatus)
                {
                    resp.status = false;
                    resp.message = "Invalid file data.";
                    resp.data = modelData.regularIncentiveDtos;
                    return resp;
                }
            }

            else
            {
                resp.status = false;
                resp.message = "Invalid file format.";
                return resp;
            }

            using (var unitOfWork = new UnitOfWork())
            {
                if (patID > 0)
                {
                    PayrollAuditTrial trail = new PayrollAuditTrial
                    {
                        PATID = patID,
                        FileName = auditData.FileName,
                        UploadedDateTime = auditData.UploadedDateTime,
                        ActivityTypeID = auditData.ActivityTypeID,
                        ActivityPeriod = auditData.ActivityPeriod,
                        ActivityStatusID = 2,
                        CompanyID = auditData.CompanyID,
                        CreatedBy = auditData.CreatedBy,
                        CreatedDate = auditData.CreatedDate,
                        CreatedIP = auditData.CreatedIP,
                        RowVersion = auditData.RowVersion,
                    };
                    trail.SetModified();
                    PayrollAuditTrail.Add(trail);
                    PayrollAuditTrail.SaveChangesWithAudit();

                    PayrollAuditTrial newTrail = new PayrollAuditTrial
                    {
                        FileName = file.FileName,
                        UploadedDateTime = DateTime.Now,
                        ActivityTypeID = activityTypeID,
                        ActivityPeriod = periodID + '-' + year.ToString(),
                        ActivityStatusID = 1
                    };

                    newTrail.SetAdded();
                    PayrollAuditTrail.Add(newTrail);
                    PayrollAuditTrail.SaveChangesWithAudit();


                    var masterEntites = RegularIncentiveRepo.Entities.Where(x => x.PATID == patID).ToList();
                    var payEntities = new List<EmployeeRegularIncentiveInfo>();


                    foreach (var masterEntite in masterEntites)
                    {
                        masterEntite.SetDeleted();
                        payEntities.Add(masterEntite);
                    }

                    RegularIncentiveRepo.AddRange(payEntities);
                    RegularIncentiveRepo.SaveChangesWithAudit();

                    List<EmployeeRegularIncentiveInfo> payRepoEntites = new List<EmployeeRegularIncentiveInfo>();

                    foreach (var payEntity in modelData.employeeRegularIncentiveInfos)
                    {
                        payEntity.PATID = newTrail.PATID;
                        payRepoEntites.Add(payEntity);
                    }
                    RegularIncentiveRepo.AddRange(payRepoEntites);
                    RegularIncentiveRepo.SaveChangesWithAudit();

                    resp.message = "File re-uploaded successfully";
                    resp.status = true;
                }

                else
                {
                    PayrollAuditTrial newTrail = new PayrollAuditTrial
                    {
                        FileName = file.FileName,
                        UploadedDateTime = DateTime.Now,
                        ActivityTypeID = activityTypeID,
                        ActivityPeriod = periodID + '-' + year.ToString(),
                        ActivityStatusID = 1
                    };

                    newTrail.SetAdded();
                    PayrollAuditTrail.Add(newTrail);
                    PayrollAuditTrail.SaveChangesWithAudit();

                    List<EmployeeRegularIncentiveInfo> payRepoEntites = new List<EmployeeRegularIncentiveInfo>();

                    foreach (var payEntity in modelData.employeeRegularIncentiveInfos)
                    {
                        payEntity.PATID = newTrail.PATID;
                        payRepoEntites.Add(payEntity);
                    }
                    RegularIncentiveRepo.AddRange(payRepoEntites);
                    RegularIncentiveRepo.SaveChangesWithAudit();

                    resp.message = "File uploaded successfully";
                    resp.status = true;
                }

                unitOfWork.CommitChangesWithAudit();
            }

            return await Task.FromResult(resp);
        }

        RegularIncentiveDataModel ValidateRegularIncentiveExcel(ExcelWorksheet worksheet, int rowCount, string year, string periodID, string secretKey, ref bool status)
        {
            RegularIncentiveDataModel regIncentiveDataModel = new RegularIncentiveDataModel();
            var employees = EmployeeRepo.GetAllList();
            status = true;
            List<RegularIncentiveDto> regIncetiveDtos = new List<RegularIncentiveDto>();
            List<EmployeeRegularIncentiveInfo> regIncentiveInfos = new List<EmployeeRegularIncentiveInfo>();
            for (int row = 2; row <= rowCount; row++)
            {
                string formatJoin = "dd MMMM, yyyy";

                DateTime disburseDate;
                DateTime joinDate;

                bool isValidDisburseDate = DateTime.TryParseExact(worksheet.Cells[row, 2].Text, formatJoin, CultureInfo.InvariantCulture, DateTimeStyles.None, out disburseDate);
                bool isValidJoinDate = DateTime.TryParseExact(worksheet.Cells[row, 8].Text, formatJoin, CultureInfo.InvariantCulture, DateTimeStyles.None, out joinDate);

                var incentive = new RegularIncentiveDto
                {
                    Incentive_Type_Year = !string.IsNullOrEmpty(worksheet.Cells[row, 1].Text) ? worksheet.Cells[row, 1].Text : string.Empty,
                    Disbursement_Date = isValidDisburseDate ? worksheet.Cells[row, 2].Text : string.Empty,
                    Employee_ID = !string.IsNullOrEmpty(worksheet.Cells[row, 3].Text) ? worksheet.Cells[row, 3].Text : string.Empty,
                    Designation = !string.IsNullOrEmpty(worksheet.Cells[row, 4].Text) ? worksheet.Cells[row, 4].Text : string.Empty,
                    Division = !string.IsNullOrEmpty(worksheet.Cells[row, 5].Text) ? worksheet.Cells[row, 5].Text : string.Empty,
                    Name = !string.IsNullOrEmpty(worksheet.Cells[row, 6].Text) ? worksheet.Cells[row, 6].Text : string.Empty,
                    Email = !string.IsNullOrEmpty(worksheet.Cells[row, 7].Text) ? worksheet.Cells[row, 7].Text : string.Empty,
                    Joining_Date = isValidJoinDate ? worksheet.Cells[row, 8].Text : string.Empty,
                    Particulars_1 = !string.IsNullOrEmpty(worksheet.Cells[row, 9].Text) ? worksheet.Cells[row, 9].Text : string.Empty,
                    Basic_Entitlement_1 = decimal.TryParse(worksheet.Cells[row, 10].Text, out var basic_salary) ? basic_salary : (decimal?)null,
                    Particulars_2 = !string.IsNullOrEmpty(worksheet.Cells[row, 11].Text) ? worksheet.Cells[row, 11].Text : string.Empty,
                    Basic_Entitlement_2 = decimal.TryParse(worksheet.Cells[row, 12].Text, out var b2) ? b2 : (decimal?)null,
                    Particulars_3 = !string.IsNullOrEmpty(worksheet.Cells[row, 13].Text) ? worksheet.Cells[row, 13].Text : string.Empty,
                    Basic_Entitlement_3 = decimal.TryParse(worksheet.Cells[row, 14].Text, out var b3) ? b3 : (decimal?)null,
                    Particulars_4 = !string.IsNullOrEmpty(worksheet.Cells[row, 15].Text) ? worksheet.Cells[row, 15].Text : string.Empty,
                    Basic_Entitlement_4 = decimal.TryParse(worksheet.Cells[row, 16].Text, out var b4) ? b4 : (decimal?)null,
                    Eligible_Bonus_BDT = !string.IsNullOrEmpty(worksheet.Cells[row, 17].Text) ? worksheet.Cells[row, 17].Text : string.Empty,
                    Eligible_Bonus_Total = decimal.TryParse(worksheet.Cells[row, 18].Text, out var eB) ? eB : (decimal?)null,
                    Income_Tax = decimal.TryParse(worksheet.Cells[row, 19].Text, out var iT) ? iT : (decimal?)null,
                    Total_Deduction = decimal.TryParse(worksheet.Cells[row, 20].Text, out var tD) ? tD : (decimal?)null,
                    Net_Payable = decimal.TryParse(worksheet.Cells[row, 21].Text, out var nP) ? nP : (decimal?)null,
                    Amount_In_Words = !string.IsNullOrEmpty(worksheet.Cells[row, 22].Text) ? worksheet.Cells[row, 22].Text : string.Empty,
                    Bank_Amount_BDT = decimal.TryParse(worksheet.Cells[row, 23].Text, out var bAmt) ? bAmt : (decimal?)null,

                    Particulars_5 = !string.IsNullOrEmpty(worksheet.Cells[row, 24].Text) ? worksheet.Cells[row, 24].Text : string.Empty,
                    Performance_rating_1 = !string.IsNullOrEmpty(worksheet.Cells[row, 25].Text) ? worksheet.Cells[row, 25].Text : string.Empty,
                    Particulars_6 = !string.IsNullOrEmpty(worksheet.Cells[row, 26].Text) ? worksheet.Cells[row, 26].Text : string.Empty,
                    Performance_rating_2 = !string.IsNullOrEmpty(worksheet.Cells[row, 27].Text) ? worksheet.Cells[row, 27].Text : string.Empty,
                };

                // Check for duplicate Employee_ID in the list
                if (regIncetiveDtos.Any(p => p.Employee_ID == incentive.Employee_ID && !string.IsNullOrEmpty(incentive.Employee_ID)))
                {
                    status = false;
                    regIncentiveDataModel.IsValid = false;
                    regIncentiveDataModel.message = $"Duplicate Employee found for Employee_ID: {incentive.Employee_ID}";
                    return regIncentiveDataModel;
                }

                #region Field Validation

                if (!isValidDisburseDate)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Disbursement Date,";
                }

                if (!isValidJoinDate)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Joining Date,";
                }

                var emp = employees.Where(e => e.EmployeeCode == incentive.Employee_ID.ToString()).ToList();
                if (emp == null || emp.Count == 0 || string.IsNullOrEmpty(incentive.Employee_ID))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Employee ID,";
                }

                if (string.IsNullOrEmpty(incentive.Designation))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Designation,";
                }

                if (string.IsNullOrEmpty(incentive.Division))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Division,";
                }
                if (string.IsNullOrEmpty(incentive.Name))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Name,";
                }
                if (string.IsNullOrEmpty(incentive.Email))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Email,";
                }

                if (string.IsNullOrEmpty(incentive.Particulars_1))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Particulars 1,";
                }
                if (incentive.Basic_Entitlement_1 < 0 || incentive.Basic_Entitlement_1.ToString().IsNumeric() == false)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Basic Entitlement 1,";
                }

                if (string.IsNullOrEmpty(incentive.Particulars_2))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Particulars 2,";
                }
                if (incentive.Basic_Entitlement_2 < 0 || incentive.Basic_Entitlement_2.ToString().IsNumeric() == false)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Basic Entitlement 2,";
                }

                if (string.IsNullOrEmpty(incentive.Particulars_3))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Particulars 3,";
                }
                if (incentive.Basic_Entitlement_3 < 0 || incentive.Basic_Entitlement_3.ToString().IsNumeric() == false)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Basic Entitlement 3,";
                }

                if (string.IsNullOrEmpty(incentive.Particulars_4))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Particulars 4,";
                }
                if (incentive.Basic_Entitlement_4 < 0 || incentive.Basic_Entitlement_4.ToString().IsNumeric() == false)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Basic Entitlement 4,";
                }

                if (string.IsNullOrEmpty(incentive.Eligible_Bonus_BDT))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Eligible Bonus BDT,";
                }

                if (incentive.Eligible_Bonus_Total < 0 || incentive.Eligible_Bonus_Total.ToString().IsNumeric() == false)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Eligible Bonus Total ,";
                }

                if (incentive.Income_Tax < 0 || incentive.Income_Tax.ToString().IsNumeric() == false)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Income Tax,";
                }

                if (incentive.Total_Deduction < 0 || incentive.Total_Deduction.ToString().IsNumeric() == false)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Total Deduction,";
                }

                if (incentive.Net_Payable < 0 || incentive.Net_Payable.ToString().IsNumeric() == false)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Net Payable,";
                }

                if (string.IsNullOrEmpty(incentive.Amount_In_Words))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Amount In Words,";
                }

                if (incentive.Bank_Amount_BDT < 0 || incentive.Bank_Amount_BDT.ToString().IsNumeric() == false)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Bank Amount BDT,";
                }

                if (!incentive.Incentive_Type_Year.Contains(periodID + '-' + year))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Period/Year,";
                }


                if (string.IsNullOrEmpty(incentive.Particulars_5) || !System.Text.RegularExpressions.Regex.IsMatch(incentive.Particulars_5, @"^[a-zA-Z0-9 ]+$"))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Particulars 5,";
                }

                if (string.IsNullOrEmpty(incentive.Performance_rating_1) || !System.Text.RegularExpressions.Regex.IsMatch(incentive.Performance_rating_1, @"^[a-zA-Z ]+$"))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Performance Rating 1,";
                }

                if (string.IsNullOrEmpty(incentive.Particulars_6) || !System.Text.RegularExpressions.Regex.IsMatch(incentive.Particulars_6, @"^[a-zA-Z0-9 ]+$"))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Particulars 6,";
                }

                if (string.IsNullOrEmpty(incentive.Performance_rating_2) || !System.Text.RegularExpressions.Regex.IsMatch(incentive.Performance_rating_2, @"^[a-zA-Z ]+$"))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Performance Rating 2,";
                }




                if (incentive.Validation_Result.IsNullOrEmpty())
                {
                    incentive.Validation_Result = "Passed";
                }

                regIncetiveDtos.Add(incentive);

                #endregion

                if (status)
                {
                    EmployeeRegularIncentiveInfo pay = new EmployeeRegularIncentiveInfo
                    {
                        IncentiveType = incentive.Incentive_Type_Year,//periodID + '-' + year.ToString(),
                        DisbursementDate = Convert.ToDateTime(incentive.Disbursement_Date),
                        EmployeeID = emp[0].EmployeeID,
                        EmployeeCode = emp[0].EmployeeCode,
                        Designation = incentive.Designation,
                        Division = incentive.Division,
                        EmployeeName = emp[0].FullName,
                        Particular1 = Convert.ToString(incentive.Particulars_1),
                        BasicEntitlement1 = PayrollEncrypt(Convert.ToString(incentive.Basic_Entitlement_1), secretKey),
                        Particular2 = Convert.ToString(incentive.Particulars_2),
                        BasicEntitlement2 = PayrollEncrypt(Convert.ToString(incentive.Basic_Entitlement_2), secretKey),
                        Particular3 = Convert.ToString(incentive.Particulars_3),
                        BasicEntitlement3 = PayrollEncrypt(Convert.ToString(incentive.Basic_Entitlement_3), secretKey),
                        Particular4 = Convert.ToString(incentive.Particulars_4),
                        BasicEntitlement4 = PayrollEncrypt(Convert.ToString(incentive.Basic_Entitlement_4), secretKey),
                        EligibleBonus = PayrollEncrypt(Convert.ToString(incentive.Eligible_Bonus_BDT), secretKey),
                        EligibleBonusTotal = PayrollEncrypt(Convert.ToString(incentive.Eligible_Bonus_Total), secretKey),
                        IncomeTax = PayrollEncrypt(Convert.ToString(incentive.Income_Tax), secretKey),
                        TotalDeduction = PayrollEncrypt(Convert.ToString(incentive.Total_Deduction), secretKey),
                        NetPayable = PayrollEncrypt(Convert.ToString(incentive.Net_Payable), secretKey),
                        AmountInWords = PayrollEncrypt(Convert.ToString(incentive.Amount_In_Words), secretKey),
                        BankAmount = PayrollEncrypt(Convert.ToString(incentive.Bank_Amount_BDT), secretKey),

                        Particulars5 = Convert.ToString(incentive.Particulars_5), 
                        PerformanceRating1 = PayrollEncrypt(Convert.ToString(incentive.Performance_rating_1), secretKey),
                        Particulars6 = Convert.ToString(incentive.Particulars_6),
                        PerformanceRating2 = PayrollEncrypt(Convert.ToString(incentive.Performance_rating_2), secretKey),

                    };
                    pay.SetAdded();
                    regIncentiveInfos.Add(pay);
                }
            }
            regIncentiveDataModel.regularIncentiveDtos = regIncetiveDtos;
            regIncentiveDataModel.employeeRegularIncentiveInfos = regIncentiveInfos;
            return regIncentiveDataModel;
        }

        RegularIncentiveDataModel ValidateRegularIncentiveCSV(List<RegularIncentiveDto> dataList, int year, string periodID, string secretKey, ref bool status)
        {
            RegularIncentiveDataModel regIncentiveDataModel = new RegularIncentiveDataModel();
            var employees = EmployeeRepo.GetAllList();
            status = true;
            List<RegularIncentiveDto> regIncetiveDtos = new List<RegularIncentiveDto>();
            List<EmployeeRegularIncentiveInfo> regIncentiveInfos = new List<EmployeeRegularIncentiveInfo>();

            string formatJoin = "dd MMMM, yyyy";
            DateTime disburseDate;
            DateTime joinDate;

            foreach (var d in dataList)
            {
                bool isValidDisburseDate = DateTime.TryParseExact(d.Disbursement_Date, formatJoin, CultureInfo.InvariantCulture, DateTimeStyles.None, out disburseDate);
                bool isValidJoinDate = DateTime.TryParseExact(d.Joining_Date, formatJoin, CultureInfo.InvariantCulture, DateTimeStyles.None, out joinDate);

                var incentive = new RegularIncentiveDto
                {
                    Incentive_Type_Year = !string.IsNullOrEmpty(d.Incentive_Type_Year) ? d.Incentive_Type_Year : string.Empty,
                    Disbursement_Date = isValidDisburseDate ? d.Disbursement_Date : string.Empty,
                    Employee_ID = !string.IsNullOrEmpty(d.Employee_ID) ? d.Employee_ID : string.Empty,
                    Designation = !string.IsNullOrEmpty(d.Designation) ? d.Designation : string.Empty,
                    Division = !string.IsNullOrEmpty(d.Division) ? d.Division : string.Empty,
                    Name = !string.IsNullOrEmpty(d.Name) ? d.Name : string.Empty,
                    Email = !string.IsNullOrEmpty(d.Email) ? d.Email : string.Empty,
                    Joining_Date = isValidJoinDate ? d.Joining_Date : string.Empty,
                    Particulars_1 = !string.IsNullOrEmpty(d.Particulars_1) ? d.Particulars_1 : string.Empty,
                    Basic_Entitlement_1 = d.Basic_Entitlement_1 > 0 ? d.Basic_Entitlement_1 : -1,
                    Particulars_2 = !string.IsNullOrEmpty(d.Particulars_2) ? d.Particulars_2 : string.Empty,
                    Basic_Entitlement_2 = d.Basic_Entitlement_2 > 0 ? d.Basic_Entitlement_2 : -1,
                    Particulars_3 = !string.IsNullOrEmpty(d.Particulars_3) ? d.Particulars_3 : string.Empty,
                    Basic_Entitlement_3 = d.Basic_Entitlement_3 > 0 ? d.Basic_Entitlement_3 : -1,
                    Particulars_4 = !string.IsNullOrEmpty(d.Particulars_4) ? d.Particulars_4 : string.Empty,
                    Basic_Entitlement_4 = d.Basic_Entitlement_4 > 0 ? d.Basic_Entitlement_4 : -1,
                    Eligible_Bonus_BDT = !string.IsNullOrEmpty(d.Eligible_Bonus_BDT) ? d.Eligible_Bonus_BDT : string.Empty,
                    Eligible_Bonus_Total = d.Eligible_Bonus_Total > 0 ? d.Eligible_Bonus_Total : -1,
                    Income_Tax = d.Income_Tax > 0 ? d.Income_Tax : -1,
                    Total_Deduction = d.Total_Deduction > 0 ? d.Total_Deduction : -1,
                    Net_Payable = d.Net_Payable > 0 ? d.Net_Payable : -1,
                    Amount_In_Words = !string.IsNullOrEmpty(d.Amount_In_Words) ? d.Amount_In_Words : string.Empty,
                    Bank_Amount_BDT = d.Bank_Amount_BDT > 0 ? d.Bank_Amount_BDT : -1,

                
                    Particulars_5 = !string.IsNullOrEmpty(d.Particulars_5) ? d.Particulars_5 : string.Empty,
                    Performance_rating_1 = !string.IsNullOrEmpty(d.Performance_rating_1) ? d.Performance_rating_1 : string.Empty,
                    Particulars_6 = !string.IsNullOrEmpty(d.Particulars_6) ? d.Particulars_6 : string.Empty,
                    Performance_rating_2 = !string.IsNullOrEmpty(d.Performance_rating_2) ? d.Performance_rating_2 : string.Empty,
                };

                // Check for duplicate Employee_ID in the list
                if (regIncetiveDtos.Any(p => p.Employee_ID == incentive.Employee_ID && !string.IsNullOrEmpty(incentive.Employee_ID)))
                {
                    status = false;
                    regIncentiveDataModel.IsValid = false;
                    regIncentiveDataModel.message = $"Duplicate Employee found for Employee_ID: {incentive.Employee_ID}";
                    return regIncentiveDataModel;
                }

                #region Field Validation

                if (!isValidDisburseDate)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Disbursement Date,";
                }

                if (!isValidJoinDate)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Joining Date,";
                }

                var emp = employees.Where(e => e.EmployeeCode == incentive.Employee_ID.ToString()).ToList();
                if (emp == null || emp.Count == 0 || string.IsNullOrEmpty(incentive.Employee_ID))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Employee ID";
                }

                if (string.IsNullOrEmpty(incentive.Designation))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Designation,";
                }

                if (string.IsNullOrEmpty(incentive.Division))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Division,";
                }
                if (string.IsNullOrEmpty(incentive.Name))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Name,";
                }
                if (string.IsNullOrEmpty(incentive.Email))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Email,";
                }

                if (string.IsNullOrEmpty(incentive.Particulars_1))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Particulars 1,";
                }
                if (incentive.Basic_Entitlement_1 < 0)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Basic Entitlement 1,";
                }

                if (string.IsNullOrEmpty(incentive.Particulars_2))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Particulars 2,";
                }
                if (incentive.Basic_Entitlement_2 < 0)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Basic Entitlement 2,";
                }

                if (string.IsNullOrEmpty(incentive.Particulars_3))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Particulars 3,";
                }
                if (incentive.Basic_Entitlement_3 < 0)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Basic Entitlement 3,";
                }

                if (string.IsNullOrEmpty(incentive.Particulars_4))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Particulars 4,";
                }
                if (incentive.Basic_Entitlement_4 < 0)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Basic Entitlement 4,";
                }

                if (string.IsNullOrEmpty(incentive.Eligible_Bonus_BDT))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Eligible Bonus BDT,";
                }

                if (incentive.Eligible_Bonus_Total < 0)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Eligible Bonus Total ,";
                }

                if (incentive.Income_Tax < 0)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Income Tax,";
                }

                if (incentive.Total_Deduction < 0)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Total Deduction,";
                }

                if (incentive.Net_Payable < 0)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Net Payable,";
                }

                if (string.IsNullOrEmpty(incentive.Amount_In_Words))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Amount In Words,";
                }

                if (incentive.Bank_Amount_BDT < 0)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Bank Amount BDT,";
                }



                if (string.IsNullOrEmpty(incentive.Particulars_5) || !System.Text.RegularExpressions.Regex.IsMatch(incentive.Particulars_5, @"^[a-zA-Z0-9 ]+$"))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Particulars 5,";
                }

                if (string.IsNullOrEmpty(incentive.Performance_rating_1) || !System.Text.RegularExpressions.Regex.IsMatch(incentive.Performance_rating_1, @"^[a-zA-Z ]+$"))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Performance Rating 1,";
                }

                if (string.IsNullOrEmpty(incentive.Particulars_6) || !System.Text.RegularExpressions.Regex.IsMatch(incentive.Particulars_6, @"^[a-zA-Z0-9 ]+$"))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Particulars 6,";
                }

                if (string.IsNullOrEmpty(incentive.Performance_rating_2) || !System.Text.RegularExpressions.Regex.IsMatch(incentive.Performance_rating_2, @"^[a-zA-Z ]+$"))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Performance Rating 2,";
                }


                if (incentive.Validation_Result.IsNullOrEmpty())
                {
                    incentive.Validation_Result = "Passed";
                }

                regIncetiveDtos.Add(incentive);

                #endregion

                if (status)
                {
                    EmployeeRegularIncentiveInfo pay = new EmployeeRegularIncentiveInfo
                    {
                        IncentiveType = incentive.Incentive_Type_Year,//periodID + '-' + year.ToString(),
                        DisbursementDate = Convert.ToDateTime(incentive.Disbursement_Date),
                        EmployeeID = emp[0].EmployeeID,
                        EmployeeCode = emp[0].EmployeeCode,
                        Designation = incentive.Designation,
                        Division = incentive.Division,
                        EmployeeName = emp[0].FullName,
                        Particular1 = Convert.ToString(incentive.Particulars_1),
                        BasicEntitlement1 = PayrollEncrypt(Convert.ToString(incentive.Basic_Entitlement_1), secretKey),
                        Particular2 = Convert.ToString(incentive.Particulars_2),
                        BasicEntitlement2 = PayrollEncrypt(Convert.ToString(incentive.Basic_Entitlement_2), secretKey),
                        Particular3 = Convert.ToString(incentive.Particulars_3),
                        BasicEntitlement3 = PayrollEncrypt(Convert.ToString(incentive.Basic_Entitlement_3), secretKey),
                        Particular4 = Convert.ToString(incentive.Particulars_4),
                        BasicEntitlement4 = PayrollEncrypt(Convert.ToString(incentive.Basic_Entitlement_4), secretKey),
                        EligibleBonus = PayrollEncrypt(Convert.ToString(incentive.Eligible_Bonus_BDT), secretKey),
                        EligibleBonusTotal = PayrollEncrypt(Convert.ToString(incentive.Eligible_Bonus_Total), secretKey),
                        IncomeTax = PayrollEncrypt(Convert.ToString(incentive.Income_Tax), secretKey),
                        TotalDeduction = PayrollEncrypt(Convert.ToString(incentive.Total_Deduction), secretKey),
                        NetPayable = PayrollEncrypt(Convert.ToString(incentive.Net_Payable), secretKey),
                        AmountInWords = PayrollEncrypt(Convert.ToString(incentive.Amount_In_Words), secretKey),
                        BankAmount = PayrollEncrypt(Convert.ToString(incentive.Bank_Amount_BDT), secretKey),

                        Particulars5 = Convert.ToString(incentive.Particulars_5),
                        PerformanceRating1 = PayrollEncrypt(Convert.ToString(incentive.Performance_rating_1), secretKey),
                        Particulars6 = Convert.ToString(incentive.Particulars_6),
                        PerformanceRating2 = PayrollEncrypt(Convert.ToString(incentive.Performance_rating_2), secretKey),
                    };
                    pay.SetAdded();
                    regIncentiveInfos.Add(pay);
                }
            }
            regIncentiveDataModel.regularIncentiveDtos = regIncetiveDtos;
            regIncentiveDataModel.employeeRegularIncentiveInfos = regIncentiveInfos;
            return regIncentiveDataModel;
        }

        public async Task<GenericResponse<EmployeeMonthlyIncentiveDto>> UploadMonthlyIncentive(int patID, int activityTypeID, string monthId, int year, string incentiveType, IFormFile file)
        {
            string fileType = file.FileName.Split('.')[1];
            var secretKey = string.IsNullOrEmpty(Convert.ToString(Config["AppSettings:EncSecret"])) ? string.Empty
                             : Config["AppSettings:EncSecret"];

            GenericResponse<EmployeeMonthlyIncentiveDto> resp = new GenericResponse<EmployeeMonthlyIncentiveDto>();
            PayrollAuditTrial auditData = new PayrollAuditTrial();
            MonthlyIncentiveDataModel modelData = new MonthlyIncentiveDataModel();


            if (patID == 0 && activityTypeID > 0 && !string.IsNullOrEmpty(monthId) && year > 0)
            {
                var auditTrail = PayrollAuditTrail.FirstOrDefault(a => a.ActivityTypeID == activityTypeID
                                                        && a.ActivityPeriod == monthId + '-' + year && a.ActivityStatusID == 1);
                if (!auditTrail.IsNullOrDbNull())
                {
                    resp.status = false;
                    resp.message = "Data was already uploaded for this month.";
                    return resp;
                }

            }


            if (patID > 0)
            {
                var auditTrail = PayrollAuditTrail.FirstOrDefault(a => a.PATID == patID && a.ActivityTypeID == activityTypeID && a.ActivityPeriod == monthId + '-' + year && a.ActivityStatusID == 1);
                //&& Convert.ToString(periodID).Equals(a.ActivityPeriod));

                if (auditTrail.IsNullOrDbNull())
                {
                    resp.status = false;
                    resp.message = "Invalid file uploaded.";
                    return resp;
                }

                auditData = auditTrail;
            }
            string[] excelValidCols = { "Salary_Month_Year", "Disbursement_Date",
                                        "Employee_ID", "Designation",
                                        "Division", "Name", "Email", "Joining_Date",
                                        "Adjusted_KPI_Performance_Score_Out_Of_100",
                                        "ESSAU_Rating","Attendance_And_Adherence_Quality_Score",
                                        "Eligible_Incentive","Total_Earnings","Adjustment",
                                        "Total_Adjustment","Income_Tax","Total_Deduction",
                                        "Net_Payable","Amount_In_Words","Wallet_Amount"
                                      };
            if (fileType.Equals("xlsx"))
            {
                using var memoryStream = new MemoryStream();
                file.CopyTo(memoryStream);
                var bytes = memoryStream.ToArray();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(new MemoryStream(bytes)))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;
                    List<string> invalidColName = new List<string>();
                    int columnCount = worksheet.Dimension.Columns;
                    for (int col = 1; col <= columnCount; col++)
                    {
                        var columnName = worksheet.Cells[1, col].Text;
                        var isOK = excelValidCols[col - 1].Trim().ToLower().Equals(columnName.Trim().ToLower())
                                    ? true : false;
                        if (!isOK)
                            invalidColName.Add(columnName);
                    }

                    if (invalidColName.Count > 0)
                    {
                        resp.status = false;
                        resp.message = "Invalid file column header.";
                        return resp;
                    }

                    bool validationStatus = true;
                    modelData = ValidateMonthlyIncentiveExcel(worksheet, rowCount, incentiveType, monthId, year.ToString(), secretKey, ref validationStatus);

                    if (modelData.IsValid == false)
                    {
                        resp.status = false;
                        resp.message = modelData.message;
                        return resp;
                    }

                    if (!validationStatus)
                    {
                        resp.status = false;
                        resp.message = "Invalid file data.";
                        resp.data = modelData.monthlyIncentiveDtos;
                        return resp;
                    }



                }
            }
            else if (fileType.Equals("csv"))
            {

                //List<string> headrs = new List<string>();

                //var fileData = new GenericParse<EmployeeMonthlyIncentiveDto>().ConvertCsvToList(file, ref headrs, excelValidCols);

                //List<string> invalidColName = new List<string>();
                //for (int col = 0; col <= headrs.Count() - 1; col++)
                //{
                //    var columnName = headrs[col];
                //    var isOK = excelValidCols[col].Trim().ToLower().Equals(columnName.Trim().ToLower())
                //                   ? true : false;
                //    if (!isOK)
                //        invalidColName.Add(columnName);
                //}

                //if (invalidColName.Count > 0)
                //{
                //    resp.status = false;
                //    resp.message = "Invalid file column header.";
                //    return resp;
                //}

                //bool validationStatus = true;
                //modelData = ValidateMonthlyIncentiveCSV(fileData, monthId.ToString(), year.ToString(), secretKey, ref validationStatus);

                //if (!validationStatus)
                //{
                //    resp.status = false;
                //    resp.message = "Invalid file data.";
                //    resp.data = modelData.monthlyIncentiveDtos;
                //    return resp;
                //}
            }
            else
            {
                resp.status = false;
                resp.message = "Invalid file format.";
                return resp;
            }
            using (var unitOfWork = new UnitOfWork())
            {
                if (patID > 0)
                {
                    PayrollAuditTrial trail = new PayrollAuditTrial
                    {
                        PATID = patID,
                        FileName = auditData.FileName,
                        UploadedDateTime = auditData.UploadedDateTime,
                        ActivityTypeID = auditData.ActivityTypeID,
                        ActivityPeriod = monthId + '-' + year,
                        ActivityStatusID = 2,
                        CompanyID = auditData.CompanyID,
                        CreatedBy = auditData.CreatedBy,
                        CreatedDate = auditData.CreatedDate,
                        CreatedIP = auditData.CreatedIP,
                        RowVersion = auditData.RowVersion,
                    };
                    trail.SetModified();
                    PayrollAuditTrail.Add(trail);
                    PayrollAuditTrail.SaveChangesWithAudit();

                    PayrollAuditTrial newTrail = new PayrollAuditTrial
                    {
                        FileName = file.FileName,
                        UploadedDateTime = DateTime.Now,
                        ActivityTypeID = activityTypeID,
                        ActivityPeriod = monthId + '-' + year,
                        ActivityStatusID = 1
                    };

                    newTrail.SetAdded();
                    PayrollAuditTrail.Add(newTrail);
                    PayrollAuditTrail.SaveChangesWithAudit();

                    var masterEntites = MonthlyIncentiveRepo.Entities.Where(x => x.PATID == patID).ToList();
                    var payEntities = new List<EmployeeMonthlyIncentiveInfo>();

                    foreach (var masterEntite in masterEntites)
                    {
                        masterEntite.SetDeleted();
                        payEntities.Add(masterEntite);
                    }

                    MonthlyIncentiveRepo.AddRange(payEntities);
                    MonthlyIncentiveRepo.SaveChangesWithAudit();

                    List<EmployeeMonthlyIncentiveInfo> payRepoEntites = new List<EmployeeMonthlyIncentiveInfo>();

                    foreach (var payEntity in modelData.monthlyIncentiveInfos)
                    {
                        payEntity.PATID = newTrail.PATID;
                        payRepoEntites.Add(payEntity);
                    }
                    MonthlyIncentiveRepo.AddRange(payRepoEntites);
                    MonthlyIncentiveRepo.SaveChangesWithAudit();

                    resp.message = "File re-uploaded successfully";
                    resp.status = true;
                }

                else
                {
                    PayrollAuditTrial newTrail = new PayrollAuditTrial
                    {
                        FileName = file.FileName,
                        UploadedDateTime = DateTime.Now,
                        ActivityTypeID = activityTypeID,
                        ActivityPeriod = monthId + '-' + year,
                        ActivityStatusID = 1
                    };

                    newTrail.SetAdded();
                    PayrollAuditTrail.Add(newTrail);
                    PayrollAuditTrail.SaveChangesWithAudit();

                    List<EmployeeMonthlyIncentiveInfo> payRepoEntites = new List<EmployeeMonthlyIncentiveInfo>();

                    foreach (var payEntity in modelData.monthlyIncentiveInfos)
                    {
                        payEntity.PATID = newTrail.PATID;
                        payRepoEntites.Add(payEntity);
                    }
                    MonthlyIncentiveRepo.AddRange(payRepoEntites);
                    MonthlyIncentiveRepo.SaveChangesWithAudit();

                    resp.message = "File uploaded successfully";
                    resp.status = true;
                }

                unitOfWork.CommitChangesWithAudit();
            }

            return await Task.FromResult(resp);
        }

        MonthlyIncentiveDataModel ValidateMonthlyIncentiveExcel(ExcelWorksheet worksheet, int rowCount, string incentiveType, string month, string year, string secretKey, ref bool status)
        {
            MonthlyIncentiveDataModel monthlyIncentiveDataModel = new MonthlyIncentiveDataModel();
            var employees = EmployeeRepo.GetAllList();
            status = true;
            List<EmployeeMonthlyIncentiveDto> monthlyIncetiveDtos = new List<EmployeeMonthlyIncentiveDto>();
            List<EmployeeMonthlyIncentiveInfo> monthlyIncentiveInfos = new List<EmployeeMonthlyIncentiveInfo>();
            for (int row = 2; row <= rowCount; row++)
            {
                string format = "MMMM-yyyy";
                string formatJoin = "dd MMMM, yyyy";

                DateTime parsedDate;
                DateTime disburseDate;
                DateTime joinDate;

                bool isValidSalaryMonthYear = DateTime.TryParseExact(worksheet.Cells[row, 1].Text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
                bool isValidDisburseDate = DateTime.TryParseExact(worksheet.Cells[row, 2].Text, formatJoin, CultureInfo.InvariantCulture, DateTimeStyles.None, out disburseDate);
                bool isValidJoinDate = DateTime.TryParseExact(worksheet.Cells[row, 8].Text, formatJoin, CultureInfo.InvariantCulture, DateTimeStyles.None, out joinDate);

                var incentive = new EmployeeMonthlyIncentiveDto
                {
                    Salary_Month_Year = isValidSalaryMonthYear ? worksheet.Cells[row, 1].Text : string.Empty,
                    Disbursement_Date = isValidDisburseDate ? worksheet.Cells[row, 2].Text : string.Empty,
                    Employee_ID = !string.IsNullOrEmpty(worksheet.Cells[row, 3].Text) ? worksheet.Cells[row, 3].Text : string.Empty,
                    Designation = !string.IsNullOrEmpty(worksheet.Cells[row, 4].Text) ? worksheet.Cells[row, 4].Text : string.Empty,
                    Division = !string.IsNullOrEmpty(worksheet.Cells[row, 5].Text) ? worksheet.Cells[row, 5].Text : string.Empty,
                    Name = !string.IsNullOrEmpty(worksheet.Cells[row, 6].Text) ? worksheet.Cells[row, 6].Text : string.Empty,
                    Email = !string.IsNullOrEmpty(worksheet.Cells[row, 7].Text) ? worksheet.Cells[row, 7].Text : string.Empty,
                    Joining_Date = isValidJoinDate ? worksheet.Cells[row, 8].Text : string.Empty,
                    Adjusted_KPI_Performance_Score_Out_Of_100 = !string.IsNullOrEmpty(worksheet.Cells[row, 9].Text) ? worksheet.Cells[row, 9].Text : string.Empty,
                    ESSAU_Rating = !string.IsNullOrEmpty(worksheet.Cells[row, 10].Text) ? worksheet.Cells[row, 10].Text : string.Empty,
                    Attendance_And_Adherence_Quality_Score = !string.IsNullOrEmpty(worksheet.Cells[row, 11].Text) ? worksheet.Cells[row, 11].Text : string.Empty,
                    Eligible_Incentive = decimal.TryParse(worksheet.Cells[row, 12].Text, out var eI) ? eI : (decimal?)null,
                    Total_Earnings = decimal.TryParse(worksheet.Cells[row, 13].Text, out var tE) ? tE : (decimal?)null,
                    Adjustment = decimal.TryParse(worksheet.Cells[row, 14].Text, out var adj) ? adj : (decimal?)null,
                    Total_Adjustment = decimal.TryParse(worksheet.Cells[row, 15].Text, out var tAdj) ? tAdj : (decimal?)null,
                    Income_Tax = decimal.TryParse(worksheet.Cells[row, 16].Text, out var iT) ? iT : (decimal?)null,
                    Total_Deduction = decimal.TryParse(worksheet.Cells[row, 17].Text, out var tD) ? tD : (decimal?)null,
                    Net_Payable = decimal.TryParse(worksheet.Cells[row, 18].Text, out var nP) ? nP : (decimal?)null,
                    Amount_In_Words = !string.IsNullOrEmpty(worksheet.Cells[row, 19].Text) ? worksheet.Cells[row, 19].Text : string.Empty,
                    Wallet_Amount = decimal.TryParse(worksheet.Cells[row, 20].Text, out var wA) ? wA : (decimal?)null,
                };
                // Check for duplicate Employee_ID in the list
                if (monthlyIncetiveDtos.Any(p => p.Employee_ID == incentive.Employee_ID && !string.IsNullOrEmpty(incentive.Employee_ID)))
                {
                    status = false;
                    monthlyIncentiveDataModel.IsValid = false;
                    monthlyIncentiveDataModel.message = $"Duplicate Employee found for Employee_ID: {incentive.Employee_ID}";
                    return monthlyIncentiveDataModel;
                }
                #region Field Validation

                if (!isValidDisburseDate)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Disbursement Date,";
                }

                if (!isValidJoinDate)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Joining Date,";
                }

                var emp = employees.Where(e => e.EmployeeCode == incentive.Employee_ID.ToString()).ToList();
                if (emp == null || emp.Count == 0 || string.IsNullOrEmpty(incentive.Employee_ID))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Employee ID,";
                }

                if (string.IsNullOrEmpty(incentive.Designation))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Designation,";
                }

                if (string.IsNullOrEmpty(incentive.Division))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Division,";
                }
                if (string.IsNullOrEmpty(incentive.Name))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Name,";
                }
                if (string.IsNullOrEmpty(incentive.Email))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Email,";
                }

                if (string.IsNullOrEmpty(incentive.Adjusted_KPI_Performance_Score_Out_Of_100))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Adjusted KPI Performance Score Out Of 100,";
                }
                if (string.IsNullOrEmpty(incentive.ESSAU_Rating))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid ESSAU Rating,";
                }

                if (string.IsNullOrEmpty(incentive.Attendance_And_Adherence_Quality_Score))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Attendance And Adherence Quality Score,";
                }
                if (incentive.Eligible_Incentive < 0 || incentive.Eligible_Incentive.IsNull())
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Eligible Incentive,";
                }

                if (incentive.Total_Earnings < 0 || incentive.Total_Earnings.IsNull())
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Total Earnings,";
                }
                if (incentive.Adjustment.ToString().IsNumeric() == false)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Adjustment,";
                }

                if (incentive.Total_Adjustment.ToString().IsNumeric() == false)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Total Adjustment,";
                }
                if (incentive.Income_Tax < 0 || incentive.Income_Tax.IsNull() || incentive.Income_Tax.ToString().IsNumeric() == false)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Income Tax,";
                }

                if (incentive.Total_Deduction < 0 || incentive.Total_Deduction.IsNull() || incentive.Total_Deduction.ToString().IsNumeric() == false)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Total Deduction,";
                }

                if (incentive.Net_Payable < 0 || incentive.Net_Payable.IsNull() || incentive.Net_Payable.ToString().IsNumeric() == false)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Net Payable,";
                }

                if (string.IsNullOrEmpty(incentive.Amount_In_Words))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Amount In Words,";
                }

                if (incentive.Wallet_Amount < 0 || incentive.Wallet_Amount.IsNull() || incentive.Wallet_Amount.ToString().IsNumeric() == false)
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Wallet Amount,";
                }

                if (!incentive.Salary_Month_Year.Contains(month + '-' + year))
                {
                    status = false;
                    incentive.Validation_Result = $"{incentive.Validation_Result} Invalid Month/Year,";
                }

                if (incentive.Validation_Result.IsNullOrEmpty())
                {
                    incentive.Validation_Result = "Passed";
                }

                monthlyIncetiveDtos.Add(incentive);

                #endregion

                if (status)
                {
                    EmployeeMonthlyIncentiveInfo pay = new EmployeeMonthlyIncentiveInfo
                    {
                        IncentiveMonth = incentive.Salary_Month_Year, //month,
                        DisbursementDate = Convert.ToDateTime(incentive.Disbursement_Date),
                        EmployeeID = emp[0].EmployeeID,
                        EmployeeCode = emp[0].EmployeeCode,
                        Designation = incentive.Designation,
                        Division = incentive.Division,
                        EmployeeName = emp[0].FullName,
                        AdjustedKPIPerformanceScore = PayrollEncrypt(Convert.ToString(incentive.Adjusted_KPI_Performance_Score_Out_Of_100), secretKey),
                        ESSAURating = PayrollEncrypt(Convert.ToString(incentive.ESSAU_Rating), secretKey),
                        AttendanceAdherenceScore = PayrollEncrypt(Convert.ToString(incentive.Attendance_And_Adherence_Quality_Score), secretKey),
                        EligibleIncentive = PayrollEncrypt(Convert.ToString(incentive.Eligible_Incentive), secretKey),
                        TotalEarnings = PayrollEncrypt(Convert.ToString(incentive.Total_Earnings), secretKey),
                        Adjustment = PayrollEncrypt(Convert.ToString(incentive.Adjustment), secretKey),
                        TotalAdjustment = PayrollEncrypt(Convert.ToString(incentive.Total_Adjustment), secretKey),
                        IncomeTax = PayrollEncrypt(Convert.ToString(incentive.Income_Tax), secretKey),
                        TotalDeduction = PayrollEncrypt(Convert.ToString(incentive.Total_Deduction), secretKey),
                        NetPayment = PayrollEncrypt(Convert.ToString(incentive.Net_Payable), secretKey),
                        AmountInWords = PayrollEncrypt(Convert.ToString(incentive.Amount_In_Words), secretKey),
                        WalletAmount = PayrollEncrypt(Convert.ToString(incentive.Wallet_Amount), secretKey)
                    };
                    pay.SetAdded();
                    monthlyIncentiveInfos.Add(pay);
                }
            }
            monthlyIncentiveDataModel.monthlyIncentiveDtos = monthlyIncetiveDtos;
            monthlyIncentiveDataModel.monthlyIncentiveInfos = monthlyIncentiveInfos;
            return monthlyIncentiveDataModel;
        }

        public async Task<GenericResponse<EmployeeFestivalBonusDto>> UploadFestivalBonus(int patID, int activityTypeID, int year, int BonusType, IFormFile file)
        {
            var secretKey = string.IsNullOrEmpty(Convert.ToString(Config["AppSettings:EncSecret"])) ? string.Empty
                             : Config["AppSettings:EncSecret"];

            GenericResponse<EmployeeFestivalBonusDto> resp = new GenericResponse<EmployeeFestivalBonusDto>();
            PayrollAuditTrial auditData = new PayrollAuditTrial();

            var ActivityPeriodInfo = BonusType switch
            {
                1 => "Eid-Ul-Fitr" + "-" + year,
                2 => "Eid-Ul-Adha" + "-" + year,
                3 => "Durga Puja" + "-" + year,
                4 => "Buddha Purnima" + "-" + year,
                5 => "Christmas" + "-" + year,
                _ => null
            };

            if (patID == 0 && activityTypeID > 0 && BonusType > 0 && year > 0)
            {
                var auditTrail = PayrollAuditTrail.FirstOrDefault(a => a.ActivityTypeID == activityTypeID
                                                        && a.ActivityPeriod == ActivityPeriodInfo && a.ActivityStatusID == 1);
                if (!auditTrail.IsNullOrDbNull())
                {
                    resp.status = false;
                    resp.message = "Data was already uploaded for this month.";
                    return resp;
                }

            }



            if (patID > 0)
            {
                var auditTrail = PayrollAuditTrail.FirstOrDefault(a => a.PATID == patID && a.ActivityTypeID == activityTypeID && a.ActivityPeriod == ActivityPeriodInfo && a.ActivityStatusID == 1);
                //&& Convert.ToString(periodID).Equals(a.ActivityPeriod));

                if (auditTrail.IsNullOrDbNull())
                {
                    resp.status = false;
                    resp.message = "Invalid file uploaded.";
                    return resp;
                }

                auditData = auditTrail;
            }
            string[] excelValidCols = { "Bonus_Month_Year","Disbursement_Date","Employee_ID","Designation",
                                         "Name","Joining_Date","Earning_Field_1","Earning_Value_1",
                                         "Total_Earnings","Deduction_Field_1","Deduction_Value_1","Deduction_Field_2",
                                         "Deduction_Value_2","Total_Deduction","Net_Payment","Amount_In_Words",
                                         "Bank_Amount_BDT","Wallet_Amount","Cash_out_Charge"
                                      };
            using var memoryStream = new MemoryStream();
            file.CopyTo(memoryStream);
            var bytes = memoryStream.ToArray();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new MemoryStream(bytes)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;
                List<string> invalidColName = new List<string>();
                int columnCount = worksheet.Dimension.Columns;
                for (int col = 1; col <= columnCount; col++)
                {
                    var columnName = worksheet.Cells[1, col].Text;
                    var isOK = excelValidCols[col - 1].Trim().ToLower().Equals(columnName.Trim().ToLower())
                                ? true : false;
                    if (!isOK)
                        invalidColName.Add(columnName);
                }

                if (invalidColName.Count > 0)
                {
                    resp.status = false;
                    resp.message = "Invalid file column header.";
                    return resp;
                }

                bool validationStatus = true;
                var modelData = ValidateFestivalBonusExcel(worksheet, rowCount, BonusType, year.ToString(), secretKey, ref validationStatus);

                if (modelData.IsValid == false)
                {
                    resp.status = false;
                    resp.message = modelData.message;
                    return resp;
                }

                if (!validationStatus)
                {
                    resp.status = false;
                    resp.message = "Invalid file data.";
                    resp.data = modelData.festivalBonusDtos;
                    return resp;
                }

                using (var unitOfWork = new UnitOfWork())
                {
                    if (patID > 0)
                    {
                        PayrollAuditTrial trail = new PayrollAuditTrial
                        {
                            PATID = patID,
                            FileName = auditData.FileName,
                            UploadedDateTime = auditData.UploadedDateTime,
                            ActivityTypeID = auditData.ActivityTypeID,
                            ActivityPeriod = auditData.ActivityPeriod,
                            ActivityStatusID = 2,
                            CompanyID = auditData.CompanyID,
                            CreatedBy = auditData.CreatedBy,
                            CreatedDate = auditData.CreatedDate,
                            CreatedIP = auditData.CreatedIP,
                            RowVersion = auditData.RowVersion,
                        };
                        trail.SetModified();
                        PayrollAuditTrail.Add(trail);
                        PayrollAuditTrail.SaveChangesWithAudit();

                        PayrollAuditTrial newTrail = new PayrollAuditTrial
                        {
                            FileName = file.FileName,
                            UploadedDateTime = DateTime.Now,
                            ActivityTypeID = activityTypeID,
                            FestivalBonusTypeID = BonusType,
                            ActivityPeriod = auditData.ActivityPeriod,
                            ActivityStatusID = 1
                        };

                        newTrail.SetAdded();
                        PayrollAuditTrail.Add(newTrail);
                        PayrollAuditTrail.SaveChangesWithAudit();

                        var masterEntites = FestivalBonusRepo.Entities.Where(x => x.PATID == patID).ToList();
                        var payEntities = new List<EmployeeFestivalBonusInfo>();

                        foreach (var masterEntite in masterEntites)
                        {
                            masterEntite.SetDeleted();
                            payEntities.Add(masterEntite);
                        }

                        FestivalBonusRepo.AddRange(payEntities);
                        FestivalBonusRepo.SaveChangesWithAudit();

                        List<EmployeeFestivalBonusInfo> payRepoEntites = new List<EmployeeFestivalBonusInfo>();

                        foreach (var payEntity in modelData.festivalBonusInfos)
                        {
                            payEntity.PATID = newTrail.PATID;
                            payRepoEntites.Add(payEntity);
                        }
                        FestivalBonusRepo.AddRange(payRepoEntites);
                        FestivalBonusRepo.SaveChangesWithAudit();

                        resp.message = "File re-uploaded successfully";
                        resp.status = true;
                    }

                    else
                    {
                        PayrollAuditTrial newTrail = new PayrollAuditTrial
                        {
                            FileName = file.FileName,
                            UploadedDateTime = DateTime.Now,
                            ActivityTypeID = activityTypeID,
                            FestivalBonusTypeID = BonusType,
                            ActivityPeriod = BonusType switch
                            {
                                1 => "Eid-Ul-Fitr" + "-" + year,
                                2 => "Eid-Ul-Adha" + "-" + year,
                                3 => "Durga Puja" + "-" + year,
                                4 => "Buddha Purnima" + "-" + year,
                                5 => "Christmas" + "-" + year,
                                _ => null
                            },
                            ActivityStatusID = 1
                        };

                        newTrail.SetAdded();
                        PayrollAuditTrail.Add(newTrail);
                        PayrollAuditTrail.SaveChangesWithAudit();

                        List<EmployeeFestivalBonusInfo> payRepoEntites = new List<EmployeeFestivalBonusInfo>();

                        foreach (var payEntity in modelData.festivalBonusInfos)
                        {
                            payEntity.PATID = newTrail.PATID;
                            payRepoEntites.Add(payEntity);
                        }
                        FestivalBonusRepo.AddRange(payRepoEntites);
                        FestivalBonusRepo.SaveChangesWithAudit();

                        resp.message = "File uploaded successfully";
                        resp.status = true;
                    }

                    unitOfWork.CommitChangesWithAudit();
                }

            }
            return await Task.FromResult(resp);
        }

        FestivalBonusDataModel ValidateFestivalBonusExcel(ExcelWorksheet worksheet, int rowCount, int BonusType, string year, string secretKey, ref bool status)
        {
            FestivalBonusDataModel festivalBonusDataModel = new FestivalBonusDataModel();
            var employees = EmployeeRepo.GetAllList();
            status = true;
            List<EmployeeFestivalBonusDto> festivalBonusDtos = new List<EmployeeFestivalBonusDto>();
            List<EmployeeFestivalBonusInfo> festivalBonusInfos = new List<EmployeeFestivalBonusInfo>();
            for (int row = 2; row <= rowCount; row++)
            {
                string format = "MMMM-yyyy";
                string formatJoin = "dd MMMM, yyyy";

                DateTime parsedDate;
                DateTime disburseDate;
                DateTime joinDate;

                //bool isValidBonusMonthYear = DateTime.TryParseExact(worksheet.Cells[row, 1].Text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
                bool isValidDisburseDate = DateTime.TryParseExact(worksheet.Cells[row, 2].Text, formatJoin, CultureInfo.InvariantCulture, DateTimeStyles.None, out disburseDate);
                bool isValidJoinDate = DateTime.TryParseExact(worksheet.Cells[row, 6].Text, formatJoin, CultureInfo.InvariantCulture, DateTimeStyles.None, out joinDate);

                //var paySlip = new EmployeeFestivalBonusDto
                //{
                //    //Bonus_Month_Year = isValidBonusMonthYear ? worksheet.Cells[row, 1].Text : string.Empty,
                //    Bonus_Month_Year = !string.IsNullOrEmpty(worksheet.Cells[row, 1].Text) ? worksheet.Cells[row, 1].Text : string.Empty,
                //    Disbursement_Date = isValidDisburseDate ? worksheet.Cells[row, 2].Text : string.Empty,
                //    Employee_ID = !string.IsNullOrEmpty(worksheet.Cells[row, 3].Text) ? worksheet.Cells[row, 3].Text : string.Empty,
                //    Designation = !string.IsNullOrEmpty(worksheet.Cells[row, 4].Text) ? worksheet.Cells[row, 4].Text : string.Empty,
                //    Name = !string.IsNullOrEmpty(worksheet.Cells[row, 5].Text) ? worksheet.Cells[row, 5].Text : string.Empty,
                //    Joining_Date = isValidJoinDate ? worksheet.Cells[row, 6].Text : string.Empty,
                //    Earning_Field_1 = !string.IsNullOrEmpty(worksheet.Cells[row, 7].Text) ? worksheet.Cells[row, 7].Text : string.Empty,
                //    Earning_Value_1 = decimal.TryParse(worksheet.Cells[row, 8].Text, out var earning_value_1) ? earning_value_1 : -1,
                //    Total_Earnings = decimal.TryParse(worksheet.Cells[row, 9].Text, out var total_earnings) ? total_earnings : -1,
                //    Deduction_Field_1 = !string.IsNullOrEmpty(worksheet.Cells[row, 10].Text) ? worksheet.Cells[row, 10].Text : string.Empty,
                //    Deduction_Value_1 = decimal.TryParse(worksheet.Cells[row, 11].Text, out var deduction_value_1) ? deduction_value_1 : -1,
                //    Deduction_Field_2 = !string.IsNullOrEmpty(worksheet.Cells[row, 12].Text) ? worksheet.Cells[row, 12].Text : string.Empty,
                //    Deduction_Value_2 = decimal.TryParse(worksheet.Cells[row, 13].Text, out var deduction_value_2) ? deduction_value_2 : -1,
                //    Total_Deduction = decimal.TryParse(worksheet.Cells[row, 14].Text, out var total_deduction) ? total_deduction : -1,
                //    Net_Payment = decimal.TryParse(worksheet.Cells[row, 15].Text, out var net_payment) ? net_payment : -1,
                //    Amount_In_Words = !string.IsNullOrEmpty(worksheet.Cells[row, 16].Text) ? worksheet.Cells[row, 16].Text : string.Empty,
                //    Bank_Amount_BDT = decimal.TryParse(worksheet.Cells[row, 17].Text, out var bank_amt) ? bank_amt : -1,
                //    Wallet_Amount = decimal.TryParse(worksheet.Cells[row, 18].Text, out var wallet_amt) ? wallet_amt : -1,
                //    Cash_out_Charge = decimal.TryParse(worksheet.Cells[row, 19].Text, out var cash_out) ? cash_out : -1
                //};
                var paySlip = new EmployeeFestivalBonusDto
                {
                    //Bonus_Month_Year = isValidBonusMonthYear ? worksheet.Cells[row, 1].Text : string.Empty,
                    Bonus_Month_Year = !string.IsNullOrEmpty(worksheet.Cells[row, 1].Text) ? worksheet.Cells[row, 1].Text : string.Empty,
                    Disbursement_Date = isValidDisburseDate ? worksheet.Cells[row, 2].Text : string.Empty,
                    Employee_ID = !string.IsNullOrEmpty(worksheet.Cells[row, 3].Text) ? worksheet.Cells[row, 3].Text : string.Empty,
                    Designation = !string.IsNullOrEmpty(worksheet.Cells[row, 4].Text) ? worksheet.Cells[row, 4].Text : string.Empty,
                    Name = !string.IsNullOrEmpty(worksheet.Cells[row, 5].Text) ? worksheet.Cells[row, 5].Text : string.Empty,
                    Joining_Date = isValidJoinDate ? worksheet.Cells[row, 6].Text : string.Empty,
                    Earning_Field_1 = !string.IsNullOrEmpty(worksheet.Cells[row, 7].Text) ? worksheet.Cells[row, 7].Text : string.Empty,
                    Earning_Value_1 = decimal.TryParse(worksheet.Cells[row, 8].Text, out var earning_value_1) ? earning_value_1 : (decimal?)null,
                    Total_Earnings = decimal.TryParse(worksheet.Cells[row, 9].Text, out var total_earnings) ? total_earnings : (decimal?)null,
                    Deduction_Field_1 = !string.IsNullOrEmpty(worksheet.Cells[row, 10].Text) ? worksheet.Cells[row, 10].Text : string.Empty,
                    Deduction_Value_1 = decimal.TryParse(worksheet.Cells[row, 11].Text, out var deduction_value_1) ? deduction_value_1 : (decimal?)null,
                    Deduction_Field_2 = !string.IsNullOrEmpty(worksheet.Cells[row, 12].Text) ? worksheet.Cells[row, 12].Text : string.Empty,
                    Deduction_Value_2 = decimal.TryParse(worksheet.Cells[row, 13].Text, out var deduction_value_2) ? deduction_value_2 : (decimal?)null,
                    Total_Deduction = decimal.TryParse(worksheet.Cells[row, 14].Text, out var total_deduction) ? total_deduction : (decimal?)null,
                    Net_Payment = decimal.TryParse(worksheet.Cells[row, 15].Text, out var net_payment) ? net_payment : (decimal?)null,
                    Amount_In_Words = !string.IsNullOrEmpty(worksheet.Cells[row, 16].Text) ? worksheet.Cells[row, 16].Text : string.Empty,
                    Bank_Amount_BDT = decimal.TryParse(worksheet.Cells[row, 17].Text, out var bank_amt) ? bank_amt : (decimal?)null,
                    Wallet_Amount = decimal.TryParse(worksheet.Cells[row, 18].Text, out var wallet_amt) ? wallet_amt : (decimal?)null,
                    Cash_out_Charge = decimal.TryParse(worksheet.Cells[row, 19].Text, out var cash_out) ? cash_out : (decimal?)null
                };

                // Check for duplicate Employee_ID in the list
                if (festivalBonusDtos.Any(p => p.Employee_ID == paySlip.Employee_ID && !string.IsNullOrEmpty(paySlip.Employee_ID)))
                {
                    status = false;
                    festivalBonusDataModel.IsValid = false;
                    festivalBonusDataModel.message = $"Duplicate Employee found for Employee_ID: {paySlip.Employee_ID}";
                    return festivalBonusDataModel;
                }

                #region Field Validation

                //if (!isValidBonusMonthYear)
                //{
                //    status = false;
                //    paySlip.Validation_Result = "Invalid Bonus Month, Year,";
                //}

                if (!isValidDisburseDate)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Disbursement Date,";
                }

                var emp = employees.Where(e => e.EmployeeCode == paySlip.Employee_ID.ToString()).ToList();
                if (emp == null || emp.Count == 0 || string.IsNullOrEmpty(paySlip.Employee_ID))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Employee ID,";
                }

                if (string.IsNullOrEmpty(paySlip.Designation))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Designation,";
                }

                if (string.IsNullOrEmpty(paySlip.Name))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Name,";
                }

                if (!isValidJoinDate)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Joining Date,";
                }

                if (string.IsNullOrEmpty(paySlip.Earning_Field_1))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Earning Field 1,";
                }

                if (paySlip.Earning_Value_1 < 0 || paySlip.Earning_Value_1.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Earning Value 1,";
                }


                if (paySlip.Total_Earnings < 0 || paySlip.Total_Earnings.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Total Earnings,";
                }

                if (string.IsNullOrEmpty(paySlip.Deduction_Field_1))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Deduction Field 1,";
                }

                if (paySlip.Deduction_Value_1 < 0 || paySlip.Deduction_Value_1.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Deduction Value 1,";
                }

                if (string.IsNullOrEmpty(paySlip.Deduction_Field_2))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Deduction Field 2,";
                }

                if (paySlip.Deduction_Value_2 < 0 || paySlip.Deduction_Value_2.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Deduction Value 2,";
                }

                if (paySlip.Total_Deduction < 0 || paySlip.Total_Deduction.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Total Deductions,";
                }

                if (paySlip.Net_Payment < 0 || paySlip.Net_Payment.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Net Payment,";
                }
                if (string.IsNullOrEmpty(paySlip.Amount_In_Words))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Amount in words,";
                }
                if (paySlip.Bank_Amount_BDT < 0 || paySlip.Bank_Amount_BDT.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Bank Amount (BDT),";
                }
                if (paySlip.Wallet_Amount < 0 || paySlip.Wallet_Amount.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Wallet Amount,";
                }
                if (paySlip.Cash_out_Charge < 0 || paySlip.Cash_out_Charge.ToString().IsNumeric() == false)
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Cash out Charge,";
                }


                if (!paySlip.Bonus_Month_Year.ToString().Contains(BonusType switch
                {
                    1 => "Eid-Ul-Fitr",
                    2 => "Eid-Ul-Adha",
                    3 => "Durga Puja",
                    4 => "Buddha Purnima",
                    5 => "Christmas",
                    _ => null
                } + '-' + year))
                {
                    status = false;
                    paySlip.Validation_Result = $"{paySlip.Validation_Result} Invalid Month/ Bonus Type,";
                }
                if (paySlip.Validation_Result.IsNullOrEmpty())
                {
                    paySlip.Validation_Result = "Passed";
                }

                festivalBonusDtos.Add(paySlip);

                #endregion

                if (status)
                {
                    EmployeeFestivalBonusInfo pay = new EmployeeFestivalBonusInfo
                    {
                        BonusMonth = paySlip.Bonus_Month_Year,
                        DisbursementDate = Convert.ToDateTime(paySlip.Disbursement_Date),
                        EmployeeID = emp[0].EmployeeID,
                        EmployeeCode = emp[0].EmployeeCode,
                        Designation = paySlip.Designation,
                        EmployeeName = emp[0].FullName,
                        EarningField1 = PayrollEncrypt(Convert.ToString(paySlip.Earning_Field_1), secretKey),
                        EarningValue1 = PayrollEncrypt(Convert.ToString(paySlip.Earning_Value_1), secretKey),
                        TotalEarnings = PayrollEncrypt(Convert.ToString(paySlip.Total_Earnings), secretKey),
                        DeductionField1 = PayrollEncrypt(Convert.ToString(paySlip.Deduction_Field_1), secretKey),
                        DeductionValue1 = PayrollEncrypt(Convert.ToString(paySlip.Deduction_Value_1), secretKey),
                        DeductionField2 = PayrollEncrypt(Convert.ToString(paySlip.Deduction_Field_2), secretKey),
                        DeductionValue2 = PayrollEncrypt(Convert.ToString(paySlip.Deduction_Value_2), secretKey),
                        TotalDeductions = PayrollEncrypt(Convert.ToString(paySlip.Total_Deduction), secretKey),
                        NetPayment = PayrollEncrypt(Convert.ToString(paySlip.Net_Payment), secretKey),
                        AmountInWords = PayrollEncrypt(Convert.ToString(paySlip.Amount_In_Words), secretKey),
                        BankAmount = PayrollEncrypt(Convert.ToString(paySlip.Bank_Amount_BDT), secretKey),
                        WalletAmount = PayrollEncrypt(Convert.ToString(paySlip.Wallet_Amount), secretKey),
                        CashOutCharge = PayrollEncrypt(Convert.ToString(paySlip.Cash_out_Charge), secretKey)
                    };
                    pay.SetAdded();
                    festivalBonusInfos.Add(pay);
                }
            }

            festivalBonusDataModel.festivalBonusDtos = festivalBonusDtos;
            festivalBonusDataModel.festivalBonusInfos = festivalBonusInfos;
            return festivalBonusDataModel;
        }

        public async Task<EmployeePaySlipInfoDto> DownloadPayslip(PaySlipModelDto model)
        {
            var secretKey = Config["AppSettings:EncSecret"];

            string sql = $@"SELECT EPS.*, E.DateOfJoining JoiningDate, ISNULL(EBI.BankAccountName, '') BankAccountName, ISNULL(EBI.BankAccountNumber,'')BankAccountNumber, ISNULL(EBI.BankBranchName,'') BankBranchName, ISNULL(EBI.BankName, '') BankName , ISNULL(E.WalletNumber,'') WalletNumber
                            FROM EmployeePaySlipInfo EPS
                            left join Employee E on EPS.EmployeeID = E.EmployeeID
                            left join EmployeeBankInfo EBI on EPS.EmployeeID=EBI.EmployeeID
                            WHERE EPS.EmployeeID={AppContexts.User.EmployeeID} and PARSENAME(REPLACE(EPS.SalaryMonth, '-', '.'), 1)={model.fiscalYear} and PARSENAME(REPLACE(EPS.SalaryMonth, '-', '.'), 2)='{model.monthName}'";
            var data = EmployeeRepo.GetModelData<EmployeePaySlipInfoDto>(sql);
            EmployeePaySlipInfoDto result = new EmployeePaySlipInfoDto();
            if (data.IsNotNull())
            {
                EmployeePaySlipInfoDto pay = new EmployeePaySlipInfoDto
                {
                    PATID = data.PATID,
                    EPSIID = data.EPSIID,
                    SalaryMonth = data.SalaryMonth,
                    DisbursementDate = data.DisbursementDate,
                    EmployeeID = data.EmployeeID,
                    EmployeeCode = data.EmployeeCode,
                    Designation = data.Designation,
                    Division = data.Division,
                    EmployeeName = data.EmployeeName,
                    Department = data.Department,
                    BasicSalary = PayrollDecrypt(Convert.ToString(data.BasicSalary), secretKey),
                    HouseRent = PayrollDecrypt(Convert.ToString(data.HouseRent), secretKey),
                    MedicalAllowance = PayrollDecrypt(Convert.ToString(data.MedicalAllowance), secretKey),
                    ConveyanceAllowance = PayrollDecrypt(Convert.ToString(data.ConveyanceAllowance), secretKey),
                    PassageForTravel = PayrollDecrypt(Convert.ToString(data.PassageForTravel), secretKey),
                    PayrollCardPart = PayrollDecrypt(Convert.ToString(data.PayrollCardPart), secretKey),
                    ArrearBasicSalary = PayrollDecrypt(Convert.ToString(data.ArrearBasicSalary), secretKey),
                    ArrearHouseRent = PayrollDecrypt(Convert.ToString(data.ArrearHouseRent), secretKey),
                    ArrearMedicalAllowance = PayrollDecrypt(Convert.ToString(data.ArrearMedicalAllowance), secretKey),
                    ArrearConveyanceAllowance = PayrollDecrypt(Convert.ToString(data.ArrearConveyanceAllowance), secretKey),
                    ArrearPassageForTravel = PayrollDecrypt(Convert.ToString(data.ArrearPassageForTravel), secretKey),
                    TotalEarnings = PayrollDecrypt(Convert.ToString(data.TotalEarnings), secretKey),
                    TotalArrears = PayrollDecrypt(Convert.ToString(data.TotalArrears), secretKey),
                    TotalDeductions = PayrollDecrypt(Convert.ToString(data.TotalDeductions), secretKey),
                    IncomeTax = PayrollDecrypt(Convert.ToString(data.IncomeTax), secretKey),
                    DeductionField1 = PayrollDecrypt(Convert.ToString(data.DeductionField1), secretKey),
                    DeductionField2 = PayrollDecrypt(Convert.ToString(data.DeductionField2), secretKey),
                    NetPayable = PayrollDecrypt(Convert.ToString(data.NetPayable), secretKey),
                    AmountInWords = PayrollDecrypt(Convert.ToString(data.AmountInWords), secretKey),
                    BankAmount = PayrollDecrypt(Convert.ToString(data.BankAmount), secretKey),
                    WalletAmount = PayrollDecrypt(Convert.ToString(data.WalletAmount), secretKey),
                    CashOutCharge = PayrollDecrypt(Convert.ToString(data.CashOutCharge), secretKey),
                    MobileAllowance = PayrollDecrypt(Convert.ToString(data.MobileAllowance), secretKey),

                    MarketBonus = PayrollDecrypt(Convert.ToString(data.MarketBonus), secretKey),
                    WeekendAllowance = PayrollDecrypt(Convert.ToString(data.WeekendAllowance), secretKey),
                    FestivalHolidayAllowance = PayrollDecrypt(Convert.ToString(data.FestivalHolidayAllowance), secretKey),
                    SaturdayAllowance = PayrollDecrypt(Convert.ToString(data.SaturdayAllowance), secretKey),
                    TaxSupport = PayrollDecrypt(Convert.ToString(data.TaxSupport), secretKey),
                    FestivalBonusArrear = PayrollDecrypt(Convert.ToString(data.FestivalBonusArrear), secretKey),
                    SalaryAdvance = PayrollDecrypt(Convert.ToString(data.SalaryAdvance), secretKey),
                    TaxRefund = PayrollDecrypt(Convert.ToString(data.TaxRefund), secretKey),
                    LaptopRepairingCostDeducted = PayrollDecrypt(Convert.ToString(data.LaptopRepairingCostDeducted), secretKey),
                    ProvidentFund = PayrollDecrypt(Convert.ToString(data.ProvidentFund), secretKey),
                    MobileBillAdjustment = PayrollDecrypt(Convert.ToString(data.MobileBillAdjustment), secretKey),
                    CarAllowance = PayrollDecrypt(Convert.ToString(data.CarAllowance), secretKey),

                    JoiningDate = data.JoiningDate,
                    BankAccountName = data.BankAccountName,
                    BankAccountNumber = data.BankAccountNumber,
                    BankBranchName = data.BankBranchName,
                    BankName = data.BankName,
                    WalletNumber = data.WalletNumber,
                };
                result = pay;
            }

            return await Task.FromResult(result);
        }

        public async Task<byte[]> GeneratePaySlipAsync(EmployeePaySlipInfoDto paySlip)
        {
            string attachmentFolder = "upload\\attachments";
            string fontFolder = "upload\\fonts";
            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);

                Document document = new Document(pdf);

                document.SetMargins(30, 25, 80, 25);

                // Load Arial font (assuming the font file is in your project or system font folder)
                IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();

                string arialFontPath = System.IO.Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot", fontFolder, "Arial.ttf");
                PdfFont arialFont = PdfFontFactory.CreateFont(arialFontPath, PdfEncodings.IDENTITY_H);

                // Now set the Arial font for the document or specific elements
                document.SetFont(arialFont);


                // Create a 3-column header table
                Table header = new Table(new float[] { 1, 3, 1 }).UseAllAvailableWidth();
                header.SetBorder(Border.NO_BORDER);

                // Left column for logo
                Cell logoCell = new Cell();
                string logoPath = System.IO.Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot", attachmentFolder, "nagad_logo.jpeg");
                if (File.Exists(logoPath))
                {
                    ImageData imageData = ImageDataFactory.Create(logoPath);
                    Image logo = new Image(imageData);
                    logo.SetWidth(102); // Adjust width as needed
                    logoCell.Add(logo);
                }
                logoCell.SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE);

                // Right column for text
                Cell textCell = new Cell();
                textCell.Add(new Paragraph("Nagad Ltd.")
                        .SetFontSize(16)
                        .SetBold()
                        .SetFontColor(ColorConstants.RED)
                        .SetTextAlignment(TextAlignment.CENTER))
                    .Add(new Paragraph("Delta Dahlia Tower (Level 14); 36, Kemal Ataturk Avenue,\nBanani, Dhaka - 1213, Bangladesh")
                        .SetFontSize(8).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph("Pay slip")
                        .SetFontSize(13).SetBold().SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph($"Salary for the month of {paySlip.SalaryMonth}")
                        .SetFontSize(12).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE);

                Cell emptyCell = new Cell();
                emptyCell.Add(new Paragraph(" this is empty cell")
                        .SetFontColor(ColorConstants.WHITE))
                    .SetBorder(Border.NO_BORDER);


                header.AddCell(logoCell);
                header.AddCell(textCell);
                header.AddCell(emptyCell);

                document.Add(header);

                // Pay slip title
                //document.Add(new Paragraph("Pay slip")
                //    .SetTextAlignment(TextAlignment.CENTER)
                //    .SetFontSize(16)
                //    .SetBold());

                //// Salary period
                //document.Add(new Paragraph($"Salary for the month of {paySlip.DisbursementDate:MMMM-yyyy}")
                //    .SetTextAlignment(TextAlignment.CENTER));

                // Disbursement Date
                document.Add(new Paragraph($"Disbursement Date: {paySlip.DisbursementDate:dd MMMM, yyyy}")
                    .SetFontSize(8).SetBold()
                        .SetFontColor(ColorConstants.GRAY)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetMarginBottom(10));

                // Employee Information
                Table employeeInfo = new Table(new float[] { 2, 1, 2, 2, 1, 2 }).UseAllAvailableWidth();
                employeeInfo.SetBorder(new SolidBorder(1));

                // Header cell
                Cell infoHeader = new Cell(1, 6)
                    .Add(new Paragraph("Employee Information:").SetFontSize(10).SetBold())
                    .SetBorder(new SolidBorder(1));
                employeeInfo.AddCell(infoHeader);

                // Employee details
                AddEmployeeInfoRow(employeeInfo, "Employee ID", paySlip.EmployeeCode, "Name", paySlip.EmployeeName);
                AddEmployeeInfoRow(employeeInfo, "Designation", paySlip.Designation, "Department", paySlip.Department);
                AddEmployeeInfoRow(employeeInfo, "Division", paySlip.Division, "Joining Date", paySlip.JoiningDate.ToString("dd MMMM yyyy"));

                document.Add(employeeInfo);

                // Salary Details Header
                document.Add(new Paragraph("Salary Details (Component-Wise Breakdown):").SetFontSize(10).SetBold()
                    .SetMarginTop(10)
                    .SetMarginBottom(5));

                float[] columnWidths = new float[] { 2, 1, 2, 1, 2, 1 };

                Table salaryTable = new Table(columnWidths).UseAllAvailableWidth();
                salaryTable.SetBorder(Border.NO_BORDER);

                // Headers
                Cell earningsHeader = new Cell(1, 2)
                    .Add(new Paragraph("Earnings (BDT)").SetFontSize(10).SetBold().SetFont(arialFont))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(new SolidBorder(1));

                Cell arrearHeader = new Cell(1, 2)
                    .Add(new Paragraph("Arrear/Adjustment (BDT)").SetFontSize(10).SetBold().SetFont(arialFont))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(new SolidBorder(1));

                Cell deductionsHeader = new Cell(1, 2)
                    .Add(new Paragraph("Deductions (BDT)").SetFontSize(10).SetBold()).SetFont(arialFont)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(new SolidBorder(1));

                salaryTable.AddCell(earningsHeader);
                salaryTable.AddCell(arrearHeader);
                salaryTable.AddCell(deductionsHeader);

                List<Tuple<string, decimal>> arrearListFilter = new List<Tuple<string, decimal>>();
                List<Tuple<string, decimal>> deductionListFilter = new List<Tuple<string, decimal>>();

                //ArrearList Before Filter
                arrearListFilter.Add(new Tuple<string, decimal>("Basic", paySlip.ArrearBasicSalary.stringToDecimal()));
                arrearListFilter.Add(new Tuple<string, decimal>("House Rent", paySlip.ArrearHouseRent.stringToDecimal()));
                arrearListFilter.Add(new Tuple<string, decimal>("Medical", paySlip.ArrearMedicalAllowance.stringToDecimal()));
                arrearListFilter.Add(new Tuple<string, decimal>("Conveyance", paySlip.ArrearConveyanceAllowance.stringToDecimal()));
                arrearListFilter.Add(new Tuple<string, decimal>("Free or Concessional \nPassage for Travel", paySlip.ArrearPassageForTravel.stringToDecimal()));

                //DeductionList Before Filter
                deductionListFilter.Add(new Tuple<string, decimal>("Income Tax                        ", paySlip.IncomeTax.stringToDecimal()));
                deductionListFilter.Add(new Tuple<string, decimal>("Extra Mobile Bill Deducted ", paySlip.MobileAllowance.stringToDecimal()));
                deductionListFilter.Add(new Tuple<string, decimal>("Laptop Repairing Cost \nDeducted", paySlip.LaptopRepairingCostDeducted.stringToDecimal()));
                deductionListFilter.Add(new Tuple<string, decimal>("Provident Fund", paySlip.ProvidentFund.stringToDecimal()));

                //String trackArrear = "Free or Concessional \nPassage for Travel";
                //String trackDeduction = "Laptop Repairing Cost \nDeducted";

                //Filter
                arrearListFilter.RemoveAll(tuple => tuple.Item2 == 0);
                deductionListFilter.RemoveAll(tuple => tuple.Item2 == 0);

                //0th Block
                var arrearTupleVal = arrearListFilter.FirstOrDefault();

                var deductionTupleVal = deductionListFilter.FirstOrDefault();

                if (arrearTupleVal !=null && deductionTupleVal !=null )
                {
                    AddSalaryRowWithBorders(salaryTable,
                           ("Basic Salary", paySlip.BasicSalary.stringToDecimal()),
                           (arrearTupleVal.Item1, arrearTupleVal.Item2),
                           (deductionTupleVal.Item1, deductionTupleVal.Item2));

                    arrearListFilter.RemoveAt(0);
                    deductionListFilter.RemoveAt(0);
                }
                else if (arrearTupleVal != null && deductionTupleVal == null)
                {
                     AddSalaryRowWithBorders(salaryTable,
                           ("Basic Salary", paySlip.BasicSalary.stringToDecimal()),
                           (arrearTupleVal.Item1, arrearTupleVal.Item2),
                           ("", 0));

                    arrearListFilter.RemoveAt(0);
                }
                else if (arrearTupleVal == null && deductionTupleVal != null)
                {
                      AddSalaryRowWithBorders(salaryTable,
                           ("Basic Salary", paySlip.BasicSalary.stringToDecimal()),
                           ("", 0),
                           (deductionTupleVal.Item1, deductionTupleVal.Item2));

                    deductionListFilter.RemoveAt(0);
                }
                else
                {
                      AddSalaryRowWithBorders(salaryTable,
                           ("Basic Salary", paySlip.BasicSalary.stringToDecimal()),
                           ("", 0),
                           ("", 0));
                }



                //1st Block
                var arrearTupleVal1 = arrearListFilter.FirstOrDefault();

                var deductionTupleVal1 = deductionListFilter.FirstOrDefault();

                if (arrearTupleVal1 != null && deductionTupleVal1 != null)
                {
                    AddSalaryRowWithBorders(salaryTable,
                           ("House Rent Allowance", paySlip.HouseRent.stringToDecimal()),
                           (arrearTupleVal1.Item1, arrearTupleVal1.Item2),
                           (deductionTupleVal1.Item1, deductionTupleVal1.Item2));

                    arrearListFilter.RemoveAt(0);
                    deductionListFilter.RemoveAt(0);
                }
                else if (arrearTupleVal1 != null && deductionTupleVal1 == null)
                {
                    AddSalaryRowWithBorders(salaryTable,
                          ("House Rent Allowance", paySlip.HouseRent.stringToDecimal()),
                          (arrearTupleVal1.Item1, arrearTupleVal1.Item2),
                          ("", 0));

                    arrearListFilter.RemoveAt(0);
                }
                else if (arrearTupleVal1 == null && deductionTupleVal1 != null)
                {
                    AddSalaryRowWithBorders(salaryTable,
                         ("House Rent Allowance", paySlip.HouseRent.stringToDecimal()),
                         ("", 0),
                         (deductionTupleVal1.Item1, deductionTupleVal1.Item2));

                    deductionListFilter.RemoveAt(0);
                }
                else
                {
                    AddSalaryRowWithBorders(salaryTable,
                         ("House Rent Allowance", paySlip.HouseRent.stringToDecimal()),
                         ("", 0),
                         ("", 0));
                }


                //2nd Block
                var arrearTupleVal2 = arrearListFilter.FirstOrDefault();

                var deductionTupleVal2 = deductionListFilter.FirstOrDefault();

                if (arrearTupleVal2 != null && deductionTupleVal2 != null)
                {
                    AddSalaryRowWithBorders(salaryTable,
                           ("Medical Allowance", paySlip.MedicalAllowance.stringToDecimal()),
                           (arrearTupleVal2.Item1, arrearTupleVal2.Item2),
                           (deductionTupleVal2.Item1, deductionTupleVal2.Item2));

                    arrearListFilter.RemoveAt(0);
                    deductionListFilter.RemoveAt(0);
                }
                else if (arrearTupleVal2 != null && deductionTupleVal2 == null)
                {
                    AddSalaryRowWithBorders(salaryTable,
                          ("Medical Allowance", paySlip.MedicalAllowance.stringToDecimal()),
                          (arrearTupleVal2.Item1, arrearTupleVal2.Item2),
                          ("", 0));

                    arrearListFilter.RemoveAt(0);
                }
                else if (arrearTupleVal2 == null && deductionTupleVal2 != null)
                {
                    AddSalaryRowWithBorders(salaryTable,
                         ("Medical Allowance", paySlip.MedicalAllowance.stringToDecimal()),
                         ("", 0),
                         (deductionTupleVal2.Item1, deductionTupleVal2.Item2));

                    deductionListFilter.RemoveAt(0);
                }
                else
                {
                    AddSalaryRowWithBorders(salaryTable,
                         ("Medical Allowance", paySlip.MedicalAllowance.stringToDecimal()),
                         ("", 0),
                         ("", 0));
                }


                //3rd Block
                var arrearTupleVal3 = arrearListFilter.FirstOrDefault();

                var deductionTupleVal3 = deductionListFilter.FirstOrDefault();

                if (arrearTupleVal3 != null && deductionTupleVal3 != null)
                {
                    AddSalaryRowWithBorders(salaryTable,
                           ("Conveyance Allowance", paySlip.ConveyanceAllowance.stringToDecimal()),
                           (arrearTupleVal3.Item1, arrearTupleVal3.Item2),
                           (deductionTupleVal3.Item1, deductionTupleVal3.Item2));

                    arrearListFilter.RemoveAt(0);
                    deductionListFilter.RemoveAt(0);
                }
                else if (arrearTupleVal3 != null && deductionTupleVal3 == null)
                {
                    AddSalaryRowWithBorders(salaryTable,
                          ("Conveyance Allowance", paySlip.ConveyanceAllowance.stringToDecimal()),
                          (arrearTupleVal3.Item1, arrearTupleVal3.Item2),
                          ("", 0));

                    arrearListFilter.RemoveAt(0);
                }
                else if (arrearTupleVal3 == null && deductionTupleVal3 != null)
                {
                    AddSalaryRowWithBorders(salaryTable,
                         ("Conveyance Allowance", paySlip.ConveyanceAllowance.stringToDecimal()),
                         ("", 0),
                         (deductionTupleVal3.Item1, deductionTupleVal3.Item2));

                    deductionListFilter.RemoveAt(0);
                }
                else
                {
                    AddSalaryRowWithBorders(salaryTable,
                         ("Conveyance Allowance", paySlip.ConveyanceAllowance.stringToDecimal()),
                         ("", 0),
                         ("", 0));
                }


                //4th Block

                var arrearTupleVal4 = arrearListFilter.FirstOrDefault();

                var deductionTupleVal4 = deductionListFilter.FirstOrDefault();

                if (arrearTupleVal4 != null)
                {
                    AddSalaryRowWithBorders(salaryTable,
                           ("Free or Concessional \nPassage for Travel", paySlip.PassageForTravel.stringToDecimal()),
                           (arrearTupleVal4.Item1, arrearTupleVal4.Item2),
                           ("", 0));

                    arrearListFilter.RemoveAt(0);
                }
                else
                {
                    AddSalaryRowWithBorders(salaryTable,
                         ("Free or Concessional \nPassage for Travel", paySlip.PassageForTravel.stringToDecimal()),
                         ("", 0),
                         ("", 0));
                }

                //AddSalaryRowWithBordersNew(salaryTable,
                //    ("Basic Salary", paySlip.BasicSalary.stringToDecimal()),
                //    ("Basic", paySlip.ArrearBasicSalary.stringToDecimal()),
                //    ("Income Tax                        ", paySlip.IncomeTax.stringToDecimal()));

                if (paySlip.PayrollCardPart.stringToDecimal() > 0)
                {
                    AddSalaryRowWithBorders(salaryTable,
                      ("Payroll Card Part", paySlip.PayrollCardPart.stringToDecimal()),
                      ("", 0),
                      ("", 0));
                }

                //new 2 earning for payroll CR2

                if (paySlip.MobileBillAdjustment.stringToDecimal() > 0)
                {
                    AddSalaryRowWithBorders(salaryTable,
                      ("Mobile Bill Adjustment", paySlip.MobileBillAdjustment.stringToDecimal()),
                      ("", 0),
                      ("", 0));
                }

                if (paySlip.CarAllowance.stringToDecimal() > 0)
                {
                    AddSalaryRowWithBorders(salaryTable,
                      ("Car Allowance", paySlip.CarAllowance.stringToDecimal()),
                      ("", 0),
                      ("", 0));
                }

                //new 8 earning

                if (paySlip.MarketBonus.stringToDecimal() > 0)
                {
                    AddSalaryRowWithBorders(salaryTable,
                      ("Market Bonus", paySlip.MarketBonus.stringToDecimal()),
                      ("", 0),
                      ("", 0));
                }
                if (paySlip.WeekendAllowance.stringToDecimal() > 0)
                {
                    AddSalaryRowWithBorders(salaryTable,
                      ("Weekend Allowance", paySlip.WeekendAllowance.stringToDecimal()),
                      ("", 0),
                      ("", 0));
                }
                if (paySlip.FestivalHolidayAllowance.stringToDecimal() > 0)
                {
                    AddSalaryRowWithBorders(salaryTable,
                      ("Festival Holliday Allowance", paySlip.FestivalHolidayAllowance.stringToDecimal()),
                      ("", 0),
                      ("", 0));
                }
                if (paySlip.SaturdayAllowance.stringToDecimal() > 0)
                {
                    AddSalaryRowWithBorders(salaryTable,
                      ("Saturday Allowance", paySlip.SaturdayAllowance.stringToDecimal()),
                      ("", 0),
                      ("", 0));
                }
                if (paySlip.TaxSupport.stringToDecimal() > 0)
                {
                    AddSalaryRowWithBorders(salaryTable,
                      ("Tax Support", paySlip.TaxSupport.stringToDecimal()),
                      ("", 0),
                      ("", 0));
                }
                if (paySlip.FestivalBonusArrear.stringToDecimal() > 0)
                {
                    AddSalaryRowWithBorders(salaryTable,
                      ("Festival Bonus Arrear", paySlip.FestivalBonusArrear.stringToDecimal()),
                      ("", 0),
                      ("", 0));
                }
                if (paySlip.SalaryAdvance.stringToDecimal() > 0)
                {
                    AddSalaryRowWithBorders(salaryTable,
                      ("Salary Advance", paySlip.SalaryAdvance.stringToDecimal()),
                      ("", 0),
                      ("", 0));
                }
                if (paySlip.TaxRefund.stringToDecimal() > 0)
                {
                    AddSalaryRowWithBorders(salaryTable,
                      ("Tax Refund", paySlip.TaxRefund.stringToDecimal()),
                      ("", 0),
                      ("", 0));
                }

                // Add totals with box borders
                AddSalaryRowWithBorders(salaryTable,
                    ("Total : ", paySlip.TotalEarnings.stringToDecimal(), true),
                    (" ", paySlip.TotalArrears.stringToDecimal(), true),
                    (" ", paySlip.TotalDeductions.stringToDecimal(), true));
                //AddSalaryFooterTotalRowWithBorders(true, salaryTable,
                //    (" ", 0, true),
                //    (" ", 0, true),
                //    (" ", 0, true));
                //AddSalaryFooterNetPayRowWithBorders(salaryTable, paySlip.NetPayable.stringToDecimal());

                salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
                salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
                salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
                salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
                salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
                salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));

                // Add Net Payable row
                Cell netPayableLabelCell = new Cell(1, 4)
                    .Add(new Paragraph("Net Payable").SetFontSize(10).SetBold())
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetBorder(new SolidBorder(1)); // Set border for this cell

                Cell netPayableAmountCell = new Cell(1, 2)
                    .Add(new Paragraph(Math.Abs(paySlip.NetPayable.stringToDecimal()) == 0 ? "-" : paySlip.NetPayable.stringToDecimal().ToString("N2"))
                    .SetFontSize(10).SetBold()
                    .SetTextAlignment(TextAlignment.RIGHT))
                    .SetBorder(new SolidBorder(1)); // Set border for this cell

                salaryTable.AddCell(netPayableLabelCell);
                salaryTable.AddCell(netPayableAmountCell);
                document.Add(salaryTable);

                // Add amount in words
                document.Add(new Paragraph("In words : " + paySlip.AmountInWords)
                    .SetMarginTop(30).SetBold().SetFontSize(10)
                    .SetMarginBottom(5));

                // Add confidential note
                document.Add(new Paragraph("[N.B : Confidential Report. Please Do Not Share with Anyone]")
                    .SetFontSize(9).SetBold()
                    .SetMarginBottom(10));

                // Payment Methods Table
                document.Add(new Paragraph("Payment Methods:")
                    .SetMarginTop(40).SetBold().SetFontSize(10)
                    .SetMarginBottom(5));

                Table paymentTable = new Table(4).UseAllAvailableWidth();
                paymentTable.SetBorder(new SolidBorder(1));

                // Add headers
                paymentTable.AddCell(new Cell().Add(new Paragraph("Payment Type").SetFontSize(10).SetBold()).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph("Bank/Wallet Name").SetFontSize(10).SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph("Account/Wallet Number").SetFontSize(10).SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph("Amount (BDT)").SetFontSize(10).SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1)));

                // Add payment methods
                //foreach (var payment in paySlip.PaymentMethods)
                //{
                paymentTable.AddCell(new Cell().Add(new Paragraph("Bank Transfer"))
                .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.BankName)
                .SetFontSize(10).SetBorder(Border.NO_BORDER)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.BankAccountNumber).SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(10).SetBorder(Border.NO_BORDER)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.BankAmount.stringToDecimal() == 0 ? "-" : paySlip.BankAmount.stringToDecimal().ToString("N2"))
                .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(Border.NO_BORDER)));

                paymentTable.AddCell(new Cell().Add(new Paragraph("Wallet Transfer"))
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph("Nagad"))
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.WalletNumber)).SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.WalletAmount.stringToDecimal() == 0 ? "-" : paySlip.WalletAmount.stringToDecimal().ToString("N2"))).SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetBorder(new SolidBorder(1)));

                paymentTable.AddCell(new Cell().Add(new Paragraph("Cash Out Charge"))
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(""))
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(""))
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.CashOutCharge.stringToDecimal() == 0 ? "-" : paySlip.CashOutCharge.stringToDecimal().ToString("N2")))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetBorder(new SolidBorder(1)));
                //}

                document.Add(paymentTable);

                string[] footerTexts = new string[]
                        {
        " This is a system generated report, it does not require any signature.",
        "Copyright ©",
        $"{DateTime.Now.Year} Nagad Ltd. All rights reserved."
                        };
                string footerData = $"Page {pdf.GetNumberOfPages()}"; // Dynamic page number
                string printedDate = DateTime.Now.ToString("dd MMMM yyyy, hh:mm tt"); // Format the printed date
                pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new FooterEventHandler(footerTexts, printedDate));


                document.Close();
                return ms.ToArray();
            }
        }

  





        //My Task Old Start
        //    public async Task<byte[]> GeneratePaySlipAsync(EmployeePaySlipInfoDto paySlip)
        //    {
        //        string attachmentFolder = "upload\\attachments";
        //        string fontFolder = "upload\\fonts";
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            PdfWriter writer = new PdfWriter(ms);
        //            PdfDocument pdf = new PdfDocument(writer);

        //            Document document = new Document(pdf);

        //            document.SetMargins(30, 25, 80, 25);

        //            // Load Arial font (assuming the font file is in your project or system font folder)
        //            IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();

        //            string arialFontPath = System.IO.Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot", fontFolder, "Arial.ttf");
        //            PdfFont arialFont = PdfFontFactory.CreateFont(arialFontPath, PdfEncodings.IDENTITY_H);

        //            // Now set the Arial font for the document or specific elements
        //            document.SetFont(arialFont);


        //            // Create a 3-column header table
        //            Table header = new Table(new float[] { 1, 3, 1 }).UseAllAvailableWidth();
        //            header.SetBorder(Border.NO_BORDER);

        //            // Left column for logo
        //            Cell logoCell = new Cell();
        //            string logoPath = System.IO.Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot", attachmentFolder, "nagad_logo.jpeg");
        //            if (File.Exists(logoPath))
        //            {
        //                ImageData imageData = ImageDataFactory.Create(logoPath);
        //                Image logo = new Image(imageData);
        //                logo.SetWidth(102); // Adjust width as needed
        //                logoCell.Add(logo);
        //            }
        //            logoCell.SetBorder(Border.NO_BORDER)
        //                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

        //            // Right column for text
        //            Cell textCell = new Cell();
        //            textCell.Add(new Paragraph("Nagad Ltd.")
        //                    .SetFontSize(16)
        //                    .SetBold()
        //                    .SetFontColor(ColorConstants.RED)
        //                    .SetTextAlignment(TextAlignment.CENTER))
        //                .Add(new Paragraph("Delta Dahlia Tower (Level 14); 36, Kemal Ataturk Avenue,\nBanani, Dhaka - 1213, Bangladesh")
        //                    .SetFontSize(8).SetTextAlignment(TextAlignment.CENTER))
        //                .SetBorder(Border.NO_BORDER)
        //                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
        //                .Add(new Paragraph("Pay slip")
        //                    .SetFontSize(13).SetBold().SetTextAlignment(TextAlignment.CENTER))
        //                .SetBorder(Border.NO_BORDER)
        //                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
        //                .Add(new Paragraph($"Salary for the month of {paySlip.SalaryMonth}")
        //                    .SetFontSize(12).SetTextAlignment(TextAlignment.CENTER))
        //                .SetBorder(Border.NO_BORDER)
        //                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

        //            Cell emptyCell = new Cell();
        //            emptyCell.Add(new Paragraph(" this is empty cell")
        //                    .SetFontColor(ColorConstants.WHITE))
        //                .SetBorder(Border.NO_BORDER);


        //            header.AddCell(logoCell);
        //            header.AddCell(textCell);
        //            header.AddCell(emptyCell);

        //            document.Add(header);

        //            // Pay slip title
        //            //document.Add(new Paragraph("Pay slip")
        //            //    .SetTextAlignment(TextAlignment.CENTER)
        //            //    .SetFontSize(16)
        //            //    .SetBold());

        //            //// Salary period
        //            //document.Add(new Paragraph($"Salary for the month of {paySlip.DisbursementDate:MMMM-yyyy}")
        //            //    .SetTextAlignment(TextAlignment.CENTER));

        //            // Disbursement Date
        //            document.Add(new Paragraph($"Disbursement Date: {paySlip.DisbursementDate:dd MMMM, yyyy}")
        //                .SetFontSize(8).SetBold()
        //                    .SetFontColor(ColorConstants.GRAY)
        //                .SetTextAlignment(TextAlignment.RIGHT)
        //                .SetMarginBottom(10));

        //            // Employee Information
        //            Table employeeInfo = new Table(new float[] { 2, 1, 2, 2, 1, 2 }).UseAllAvailableWidth();
        //            employeeInfo.SetBorder(new SolidBorder(1));

        //            // Header cell
        //            Cell infoHeader = new Cell(1, 6)
        //                .Add(new Paragraph("Employee Information:").SetFontSize(10).SetBold())
        //                .SetBorder(new SolidBorder(1));
        //            employeeInfo.AddCell(infoHeader);

        //            // Employee details
        //            AddEmployeeInfoRow(employeeInfo, "Employee ID", paySlip.EmployeeCode, "Name", paySlip.EmployeeName);
        //            AddEmployeeInfoRow(employeeInfo, "Designation", paySlip.Designation, "Department", paySlip.Department);
        //            AddEmployeeInfoRow(employeeInfo, "Division", paySlip.Division, "Joining Date", paySlip.JoiningDate.ToString("dd MMMM yyyy"));

        //            document.Add(employeeInfo);

        //            // Salary Details Header
        //            document.Add(new Paragraph("Salary Details (Component-Wise Breakdown):").SetFontSize(10).SetBold()
        //                .SetMarginTop(10)
        //                .SetMarginBottom(5));

        //            float[] columnWidths = new float[] { 2, 1, 2, 1, 2, 1 };

        //            Table salaryTable = new Table(columnWidths).UseAllAvailableWidth();
        //            salaryTable.SetBorder(Border.NO_BORDER);

        //            // Headers
        //            Cell earningsHeader = new Cell(1, 2)
        //                .Add(new Paragraph("Earnings (BDT)").SetFontSize(10).SetBold().SetFont(arialFont))
        //                .SetTextAlignment(TextAlignment.CENTER)
        //                .SetBorder(new SolidBorder(1));

        //            Cell arrearHeader = new Cell(1, 2)
        //                .Add(new Paragraph("Arrear/Adjustment (BDT)").SetFontSize(10).SetBold().SetFont(arialFont))
        //                .SetTextAlignment(TextAlignment.CENTER)
        //                .SetBorder(new SolidBorder(1));

        //            Cell deductionsHeader = new Cell(1, 2)
        //                .Add(new Paragraph("Deductions (BDT)").SetFontSize(10).SetBold()).SetFont(arialFont)
        //                .SetTextAlignment(TextAlignment.CENTER)
        //                .SetBorder(new SolidBorder(1));

        //            salaryTable.AddCell(earningsHeader);
        //            salaryTable.AddCell(arrearHeader);
        //            salaryTable.AddCell(deductionsHeader);

        //            // Add salary components

        ////            AddSalaryRowIfNotZero(salaryTable,
        ////"Basic Salary", paySlip.BasicSalary.stringToDecimal(),
        ////"Basic", paySlip.ArrearBasicSalary.stringToDecimal(),
        ////"Income Tax", paySlip.IncomeTax.stringToDecimal());


        //            AddSalaryRowWithBorders(salaryTable,
        //                ("Basic Salary", paySlip.BasicSalary.stringToDecimal()),
        //                ("Basic", paySlip.ArrearBasicSalary.stringToDecimal()),
        //                ("Income Tax                        ", paySlip.IncomeTax.stringToDecimal()));

        //            AddSalaryRowWithBorders(salaryTable,
        //                ("House Rent Allowance", paySlip.HouseRent.stringToDecimal()),
        //                ("House Rent", paySlip.ArrearHouseRent.stringToDecimal()),
        //                ("Extra Mobile Bill Deducted ", paySlip.MobileAllowance.stringToDecimal() > 0 ? paySlip.MobileAllowance.stringToDecimal() : 0));

        //            AddSalaryRowWithBorders(salaryTable,
        //                ("Medical Allowance", paySlip.MedicalAllowance.stringToDecimal()),
        //                ("Medical", paySlip.ArrearMedicalAllowance.stringToDecimal()),
        //                (paySlip.LaptopRepairingCostDeducted.stringToDecimal() > 0 ? "Laptop Repairing Cost \nDeducted" : "", paySlip.LaptopRepairingCostDeducted.stringToDecimal() > 0 ? paySlip.LaptopRepairingCostDeducted.stringToDecimal() : 0));

        //            AddSalaryRowWithBorders(salaryTable,
        //                ("Conveyance Allowance", paySlip.ConveyanceAllowance.stringToDecimal()),
        //                ("Conveyance", paySlip.ArrearConveyanceAllowance.stringToDecimal()),
        //                (paySlip.ProvidentFund.stringToDecimal() > 0 ? "Provident Fund" : "", paySlip.ProvidentFund.stringToDecimal() > 0 ? paySlip.ProvidentFund.stringToDecimal() : 0));

        //            AddSalaryRowWithBorders(salaryTable,
        //                ("Free or Concessional \nPassage for Travel", paySlip.PassageForTravel.stringToDecimal()),
        //                ("Free or Concessional \nPassage for Travel", paySlip.ArrearPassageForTravel.stringToDecimal()),
        //                ("", 0));

        //            if (paySlip.PayrollCardPart.stringToDecimal() > 0)
        //            {
        //                AddSalaryRowWithBorders(salaryTable,
        //                  ("Payroll Card Part", paySlip.PayrollCardPart.stringToDecimal()),
        //                  ("", 0),
        //                  ("", 0));
        //            }

        //            //new 8 earning

        //            if (paySlip.MarketBonus.stringToDecimal() > 0)
        //            {
        //                AddSalaryRowWithBorders(salaryTable,
        //                  ("Market Bonus", paySlip.MarketBonus.stringToDecimal()),
        //                  ("", 0),
        //                  ("", 0));
        //            }
        //            if (paySlip.WeekendAllowance.stringToDecimal() > 0)
        //            {
        //                AddSalaryRowWithBorders(salaryTable,
        //                  ("Weekend Allowance", paySlip.WeekendAllowance.stringToDecimal()),
        //                  ("", 0),
        //                  ("", 0));
        //            }
        //            if (paySlip.FestivalHolidayAllowance.stringToDecimal() > 0)
        //            {
        //                AddSalaryRowWithBorders(salaryTable,
        //                  ("Festival Holliday Allowance", paySlip.FestivalHolidayAllowance.stringToDecimal()),
        //                  ("", 0),
        //                  ("", 0));
        //            }
        //            if (paySlip.SaturdayAllowance.stringToDecimal() > 0)
        //            {
        //                AddSalaryRowWithBorders(salaryTable,
        //                  ("Saturday Allowance", paySlip.SaturdayAllowance.stringToDecimal()),
        //                  ("", 0),
        //                  ("", 0));
        //            }
        //            if (paySlip.TaxSupport.stringToDecimal() > 0)
        //            {
        //                AddSalaryRowWithBorders(salaryTable,
        //                  ("Tax Support", paySlip.TaxSupport.stringToDecimal()),
        //                  ("", 0),
        //                  ("", 0));
        //            }
        //            if (paySlip.FestivalBonusArrear.stringToDecimal() > 0)
        //            {
        //                AddSalaryRowWithBorders(salaryTable,
        //                  ("Festival Bonus Arrear", paySlip.FestivalBonusArrear.stringToDecimal()),
        //                  ("", 0),
        //                  ("", 0));
        //            }
        //            if (paySlip.SalaryAdvance.stringToDecimal() > 0)
        //            {
        //                AddSalaryRowWithBorders(salaryTable,
        //                  ("Salary Advance", paySlip.SalaryAdvance.stringToDecimal()),
        //                  ("", 0),
        //                  ("", 0));
        //            }
        //            if (paySlip.TaxRefund.stringToDecimal() > 0)
        //            {
        //                AddSalaryRowWithBorders(salaryTable,
        //                  ("Tax Refund", paySlip.TaxRefund.stringToDecimal()),
        //                  ("", 0),
        //                  ("", 0));
        //            }

        //            // Add totals with box borders
        //            AddSalaryRowWithBorders(salaryTable,
        //                ("Total : ", paySlip.TotalEarnings.stringToDecimal(), true),
        //                (" ", paySlip.TotalArrears.stringToDecimal(), true),
        //                (" ", paySlip.TotalDeductions.stringToDecimal(), true));
        //            //AddSalaryFooterTotalRowWithBorders(true, salaryTable,
        //            //    (" ", 0, true),
        //            //    (" ", 0, true),
        //            //    (" ", 0, true));
        //            //AddSalaryFooterNetPayRowWithBorders(salaryTable, paySlip.NetPayable.stringToDecimal());

        //            salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
        //            salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
        //            salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
        //            salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
        //            salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
        //            salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));

        //            // Add Net Payable row
        //            Cell netPayableLabelCell = new Cell(1, 4)
        //                .Add(new Paragraph("Net Payable").SetFontSize(10).SetBold())
        //                .SetTextAlignment(TextAlignment.RIGHT)
        //                .SetBorder(new SolidBorder(1)); // Set border for this cell

        //            Cell netPayableAmountCell = new Cell(1, 2)
        //                .Add(new Paragraph(Math.Abs(paySlip.NetPayable.stringToDecimal()) == 0 ? "-" : paySlip.NetPayable.stringToDecimal().ToString("N2"))
        //                .SetFontSize(10).SetBold()
        //                .SetTextAlignment(TextAlignment.RIGHT))
        //                .SetBorder(new SolidBorder(1)); // Set border for this cell

        //            salaryTable.AddCell(netPayableLabelCell);
        //            salaryTable.AddCell(netPayableAmountCell);
        //            document.Add(salaryTable);

        //            // Add amount in words
        //            document.Add(new Paragraph("In words : " + paySlip.AmountInWords)
        //                .SetMarginTop(30).SetBold().SetFontSize(10)
        //                .SetMarginBottom(5));

        //            // Add confidential note
        //            document.Add(new Paragraph("[N.B : Confidential Report. Please Do Not Share with Anyone]")
        //                .SetFontSize(9).SetBold()
        //                .SetMarginBottom(10));

        //            // Payment Methods Table
        //            document.Add(new Paragraph("Payment Methods:")
        //                .SetMarginTop(40).SetBold().SetFontSize(10)
        //                .SetMarginBottom(5));

        //            Table paymentTable = new Table(4).UseAllAvailableWidth();
        //            paymentTable.SetBorder(new SolidBorder(1));

        //            // Add headers
        //            paymentTable.AddCell(new Cell().Add(new Paragraph("Payment Type").SetFontSize(10).SetBold()).SetBorder(new SolidBorder(1)));
        //            paymentTable.AddCell(new Cell().Add(new Paragraph("Bank/Wallet Name").SetFontSize(10).SetBold()
        //                .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1)));
        //            paymentTable.AddCell(new Cell().Add(new Paragraph("Account/Wallet Number").SetFontSize(10).SetBold()
        //                .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1)));
        //            paymentTable.AddCell(new Cell().Add(new Paragraph("Amount (BDT)").SetFontSize(10).SetBold()
        //                .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1)));

        //            // Add payment methods
        //            //foreach (var payment in paySlip.PaymentMethods)
        //            //{
        //            paymentTable.AddCell(new Cell().Add(new Paragraph("Bank Transfer"))
        //            .SetFontSize(10).SetBorder(new SolidBorder(1)));
        //            paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.BankName)
        //            .SetFontSize(10).SetBorder(Border.NO_BORDER)));
        //            paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.BankAccountNumber).SetTextAlignment(TextAlignment.CENTER)
        //            .SetFontSize(10).SetBorder(Border.NO_BORDER)));
        //            paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.BankAmount.stringToDecimal() == 0 ? "-" : paySlip.BankAmount.stringToDecimal().ToString("N2"))
        //            .SetFontSize(10)
        //                .SetTextAlignment(TextAlignment.CENTER)
        //                .SetBorder(Border.NO_BORDER)));

        //            paymentTable.AddCell(new Cell().Add(new Paragraph("Wallet Transfer"))
        //                .SetFontSize(10).SetBorder(new SolidBorder(1)));
        //            paymentTable.AddCell(new Cell().Add(new Paragraph("Nagad"))
        //                .SetFontSize(10).SetBorder(new SolidBorder(1)));
        //            paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.WalletNumber)).SetTextAlignment(TextAlignment.CENTER)
        //                .SetFontSize(10).SetBorder(new SolidBorder(1)));
        //            paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.WalletAmount.stringToDecimal() == 0 ? "-" : paySlip.WalletAmount.stringToDecimal().ToString("N2"))).SetTextAlignment(TextAlignment.CENTER)
        //                .SetFontSize(10)
        //                .SetBorder(new SolidBorder(1)));

        //            paymentTable.AddCell(new Cell().Add(new Paragraph("Cash Out Charge"))
        //                .SetFontSize(10).SetBorder(new SolidBorder(1)));
        //            paymentTable.AddCell(new Cell().Add(new Paragraph(""))
        //                .SetFontSize(10).SetBorder(new SolidBorder(1)));
        //            paymentTable.AddCell(new Cell().Add(new Paragraph(""))
        //                .SetFontSize(10).SetBorder(new SolidBorder(1)));
        //            paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.CashOutCharge.stringToDecimal() == 0 ? "-" : paySlip.CashOutCharge.stringToDecimal().ToString("N2")))
        //                .SetTextAlignment(TextAlignment.CENTER)
        //                .SetFontSize(10)
        //                .SetBorder(new SolidBorder(1)));
        //            //}

        //            document.Add(paymentTable);

        //            string[] footerTexts = new string[]
        //                    {
        //        " This is a system generated report, it does not require any signature.",
        //        "Copyright ©",
        //        $"{DateTime.Now.Year} Nagad Ltd. All rights reserved."
        //                    };
        //            string footerData = $"Page {pdf.GetNumberOfPages()}"; // Dynamic page number
        //            string printedDate = DateTime.Now.ToString("dd MMMM yyyy, hh:mm tt"); // Format the printed date
        //            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new FooterEventHandler(footerTexts, printedDate));



        //            document.Close();
        //            return ms.ToArray();
        //        }
        //    }
        //My Task Old End



        //private void AddEmployeeInfoRow(Table table, string label1, string value1, string label2, string value2)
        //{
        //    table.AddCell(new Cell().Add(new Paragraph(label1)).SetFontSize(10).SetBold().SetBorder(Border.NO_BORDER));
        //    table.AddCell(new Cell().Add(new Paragraph(" : ")).SetFontSize(10).SetBold().SetBorder(Border.NO_BORDER));
        //    table.AddCell(new Cell().Add(new Paragraph(value1)).SetFontSize(10).SetBorder(Border.NO_BORDER));
        //    table.AddCell(new Cell().Add(new Paragraph(label2)).SetFontSize(10).SetBold().SetBorder(Border.NO_BORDER));
        //    table.AddCell(new Cell().Add(new Paragraph(" : ")).SetFontSize(10).SetBold().SetBorder(Border.NO_BORDER));
        //    table.AddCell(new Cell().Add(new Paragraph(value2)).SetFontSize(10).SetBorder(Border.NO_BORDER));
        //}
        private void AddEmployeeInfoRow(Table table, string label1, string value1, string label2, string value2)
        {
            // Set a fixed width for each cell
            float cellWidthLabel = 80; // Example width, adjust as needed
            float cellWidthColon = 10; // Example width, adjust as needed
            float cellWidthValue = 190; // Example width, adjust as needed

            // Add label1
            table.AddCell(new Cell().Add(new Paragraph(label1))
                .SetFontSize(10)
                .SetBold()
                .SetBorder(Border.NO_BORDER)
                .SetWidth(cellWidthLabel)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.TOP));

            // Add separator
            table.AddCell(new Cell().Add(new Paragraph(" : "))
                .SetFontSize(10)
                .SetBold()
                .SetBorder(Border.NO_BORDER)
                .SetWidth(cellWidthColon)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.TOP));

            // Add value1
            table.AddCell(new Cell().Add(new Paragraph(value1))
                .SetFontSize(10)
                .SetBorder(Border.NO_BORDER)
                .SetWidth(cellWidthValue)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.TOP));

            // Add label2
            table.AddCell(new Cell().Add(new Paragraph(label2))
                .SetFontSize(10)
                .SetBold()
                .SetBorder(Border.NO_BORDER)
                .SetWidth(cellWidthLabel)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.TOP));

            // Add separator
            table.AddCell(new Cell().Add(new Paragraph(" : "))
                .SetFontSize(10)
                .SetBold()
                .SetBorder(Border.NO_BORDER)
                .SetWidth(cellWidthColon)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.TOP));

            // Add value2
            table.AddCell(new Cell().Add(new Paragraph(value2))
                .SetFontSize(10)
                .SetWidth(cellWidthValue)
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.TOP));
        }

        private static readonly Border ThinBorder = new SolidBorder(0.5f); // 0.5f for a thin border

        //Old

        private void AddSalaryRowWithBorders(Table table,
    (string label, decimal amount, bool isBold) earnings,
    (string label, decimal amount, bool isBold) arrear,
    (string label, decimal amount, bool isBold) deductions)
        {
            Border border = Border.NO_BORDER;
            Border totalBorder = new SolidBorder(1); // Border for total rows
            Border rightBorder = new SolidBorder(0.5f);
            Border noBorder = Border.NO_BORDER;

            // Determine if this is a total row
            bool isTotal = earnings.isBold || arrear.isBold || deductions.isBold;
            Border currentBorder = isTotal ? totalBorder : border;

            // Earnings columns
            if (!string.IsNullOrEmpty(earnings.label))
            {
                var labelPara = new Paragraph(earnings.label);
                if (earnings.isBold)
                {
                    labelPara.SetBold().SetTextAlignment(TextAlignment.RIGHT);
                }
                //table.AddCell(new Cell()
                //    .Add(labelPara)
                //    .SetFontSize(10)
                //    .SetBorder(currentBorder));
                table.AddCell(new Cell()
                    .Add(labelPara)
                    .SetFontSize(10)
                     .SetBorderRight(Border.NO_BORDER)
                     .SetBorderLeft(rightBorder)
                     .SetBorderTop(isTotal ? rightBorder : noBorder)
                     .SetBorderBottom(isTotal ? rightBorder : noBorder));

                // var amountPara = new Paragraph(earnings.amount > 0 ? earnings.amount.ToString("N2") : "(" + Math.Abs(earnings.amount).ToString("N2") + ")");
                var amountPara = new Paragraph(Math.Abs(earnings.amount) == 0 ? "-" : earnings.amount > 0 ? earnings.amount.ToString("N2") : "(" + Math.Abs(earnings.amount).ToString("N2") + ")");
                if (earnings.isBold) amountPara.SetBold();
                table.AddCell(new Cell()
                 .Add(amountPara)
                 .SetFontSize(10)
                 .SetTextAlignment(TextAlignment.RIGHT)
                 .SetBorderRight(rightBorder)
                 .SetBorderLeft(Border.NO_BORDER)
                 .SetBorderTop(isTotal ? rightBorder : noBorder)
                 .SetBorderBottom(isTotal ? rightBorder : noBorder));

            }
            else
            {
                table.AddCell(new Cell().SetBorder(currentBorder));
                table.AddCell(new Cell().SetBorderRight(rightBorder)
                 .SetBorderLeft(Border.NO_BORDER)
                 .SetBorderTop(isTotal ? rightBorder : noBorder)
                 .SetBorderBottom(isTotal ? rightBorder : noBorder));
            }

            // Arrear columns
            if (!string.IsNullOrEmpty(arrear.label))
            {
                var labelPara = new Paragraph(arrear.label);
                if (arrear.isBold) labelPara.SetBold();
                table.AddCell(new Cell()
                    .Add(labelPara)
                    .SetFontSize(10)
                     .SetBorderRight(Border.NO_BORDER)
                     .SetBorderLeft(rightBorder)
                     .SetBorderTop(isTotal ? rightBorder : noBorder)
                     .SetBorderBottom(isTotal ? rightBorder : noBorder));

                // var amountPara = new Paragraph(arrear.amount > 0 ? arrear.amount.ToString("N2") : "(" + Math.Abs(arrear.amount) + ")");
                var amountPara = new Paragraph(Math.Abs(arrear.amount) == 0 ? "-" : arrear.amount > 0 ? arrear.amount.ToString("N2") : "(" + Math.Abs(arrear.amount).ToString("N2") + ")");
                //var amountPara = new Paragraph("(" + Math.Abs(arrear.amount).ToString("N2") + ")");
                if (arrear.isBold) amountPara.SetBold();
                table.AddCell(new Cell()
                .Add(amountPara)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetBorderRight(rightBorder)
                 .SetBorderLeft(Border.NO_BORDER)
                .SetBorderTop(isTotal ? rightBorder : noBorder)
                .SetBorderBottom(isTotal ? rightBorder : noBorder));
            }
            else
            {
                table.AddCell(new Cell().SetBorder(currentBorder));
                table.AddCell(new Cell().SetBorderRight(rightBorder)
                 .SetBorderLeft(Border.NO_BORDER)
                 .SetBorderTop(isTotal ? rightBorder : noBorder)
                 .SetBorderBottom(isTotal ? rightBorder : noBorder));
            }

            // Deductions columns
            if (!string.IsNullOrEmpty(deductions.label))
            {
                var labelPara = new Paragraph(deductions.label);
                if (deductions.isBold) labelPara.SetBold();
                table.AddCell(new Cell()
                    .Add(labelPara)
                    .SetFontSize(10)
                     .SetBorderRight(Border.NO_BORDER)
                     .SetBorderLeft(rightBorder)
                     .SetBorderTop(isTotal ? rightBorder : noBorder)
                     .SetBorderBottom(isTotal ? rightBorder : noBorder));

                // var amountPara = new Paragraph(deductions.amount > 0 ? deductions.amount.ToString("N2") : "(" + Math.Abs(deductions.amount).ToString("N2") + ")");
                var amountPara = new Paragraph(Math.Abs(deductions.amount) == 0 ? "-" : deductions.amount > 0 ? deductions.amount.ToString("N2") : "(" + Math.Abs(deductions.amount).ToString("N2") + ")");
                if (deductions.isBold) amountPara.SetBold();
                table.AddCell(new Cell()
               .Add(amountPara)
               .SetFontSize(10)
               .SetTextAlignment(TextAlignment.RIGHT)
               .SetBorderRight(rightBorder)
                 .SetBorderLeft(Border.NO_BORDER)
               .SetBorderTop(isTotal ? rightBorder : noBorder)
               .SetBorderBottom(isTotal ? rightBorder : noBorder));
            }
            else
            {
                table.AddCell(new Cell().SetBorder(currentBorder));
                table.AddCell(new Cell().SetBorderRight(rightBorder)
                 .SetBorderLeft(Border.NO_BORDER)
                 .SetBorderTop(isTotal ? rightBorder : noBorder)
                 .SetBorderBottom(isTotal ? rightBorder : noBorder));
            }
        }

        //New Start

        //    private void AddSalaryRowWithBordersNew(Table table,
        //(string label, decimal amount, bool isBold) earnings,
        //(string label, decimal amount, bool isBold) arrear,
        //(string label, decimal amount, bool isBold) deductions)
        //    {
        //        Border border = Border.NO_BORDER;
        //        Border totalBorder = new SolidBorder(1); // Border for total rows
        //        Border rightBorder = new SolidBorder(0.5f);
        //        Border noBorder = Border.NO_BORDER;

        //        // Determine if this is a total row
        //        bool isTotal = earnings.isBold || arrear.isBold || deductions.isBold;
        //        Border currentBorder = isTotal ? totalBorder : border;

        //        // Earnings columns
        //        if (!string.IsNullOrEmpty(earnings.label))
        //        {
        //            var labelPara = new Paragraph(earnings.label);
        //            if (earnings.isBold)
        //            {
        //                labelPara.SetBold().SetTextAlignment(TextAlignment.RIGHT);
        //            }
        //            //table.AddCell(new Cell()
        //            //    .Add(labelPara)
        //            //    .SetFontSize(10)
        //            //    .SetBorder(currentBorder));
        //            table.AddCell(new Cell()
        //                .Add(labelPara)
        //                .SetFontSize(10)
        //                 .SetBorderRight(Border.NO_BORDER)
        //                 .SetBorderLeft(rightBorder)
        //                 .SetBorderTop(isTotal ? rightBorder : noBorder)
        //                 .SetBorderBottom(isTotal ? rightBorder : noBorder));

        //            // var amountPara = new Paragraph(earnings.amount > 0 ? earnings.amount.ToString("N2") : "(" + Math.Abs(earnings.amount).ToString("N2") + ")");
        //            var amountPara = new Paragraph(Math.Abs(earnings.amount) == 0 ? "-" : earnings.amount > 0 ? earnings.amount.ToString("N2") : "(" + Math.Abs(earnings.amount).ToString("N2") + ")");
        //            if (earnings.isBold) amountPara.SetBold();
        //            table.AddCell(new Cell()
        //             .Add(amountPara)
        //             .SetFontSize(10)
        //             .SetTextAlignment(TextAlignment.RIGHT)
        //             .SetBorderRight(rightBorder)
        //             .SetBorderLeft(Border.NO_BORDER)
        //             .SetBorderTop(isTotal ? rightBorder : noBorder)
        //             .SetBorderBottom(isTotal ? rightBorder : noBorder));

        //        }
        //        else
        //        {
        //            table.AddCell(new Cell().SetBorder(currentBorder));
        //            table.AddCell(new Cell().SetBorderRight(rightBorder)
        //             .SetBorderLeft(Border.NO_BORDER)
        //             .SetBorderTop(isTotal ? rightBorder : noBorder)
        //             .SetBorderBottom(isTotal ? rightBorder : noBorder));
        //        }

        //        // Arrear columns
        //        if (arrear.amount != 0)
        //        {
        //            var arrearLabelPara = new Paragraph(arrear.label);
        //            table.AddCell(new Cell()
        //                .Add(arrearLabelPara)
        //                .SetFontSize(10)
        //                .SetTextAlignment(TextAlignment.LEFT)
        //                .SetBorderRight(Border.NO_BORDER)
        //                .SetBorderLeft(rightBorder)
        //                .SetBorderTop(noBorder)
        //                .SetBorderBottom(noBorder));

        //            var arrearAmountPara = new Paragraph(arrear.amount == 0 ? "-" : arrear.amount.ToString("N2"));
        //            table.AddCell(new Cell()
        //                .Add(arrearAmountPara)
        //                .SetFontSize(10)
        //                .SetTextAlignment(TextAlignment.RIGHT)
        //                .SetBorderRight(rightBorder)
        //                .SetBorderLeft(Border.NO_BORDER)
        //                .SetBorderTop(noBorder)
        //                .SetBorderBottom(noBorder));
        //        }
        //        else
        //        {
        //            table.AddCell(new Cell().SetBorder(noBorder));
        //            table.AddCell(new Cell().SetBorderRight(rightBorder)
        //                .SetBorderLeft(Border.NO_BORDER)
        //                .SetBorderTop(noBorder)
        //                .SetBorderBottom(noBorder));
        //        }

        //        // Deductions columns (skip this row if the value is zero)
        //        if (deductions.amount != 0)
        //        {
        //            var deductionLabelPara = new Paragraph(deductions.label);
        //            table.AddCell(new Cell()
        //                .Add(deductionLabelPara)
        //                .SetFontSize(10)
        //                .SetTextAlignment(TextAlignment.LEFT)
        //                .SetBorderRight(Border.NO_BORDER)
        //                .SetBorderLeft(rightBorder)
        //                .SetBorderTop(noBorder)
        //                .SetBorderBottom(noBorder));

        //            var deductionAmountPara = new Paragraph(deductions.amount == 0 ? "-" : deductions.amount.ToString("N2"));
        //            table.AddCell(new Cell()
        //                .Add(deductionAmountPara)
        //                .SetFontSize(10)
        //                .SetTextAlignment(TextAlignment.RIGHT)
        //                .SetBorderRight(rightBorder)
        //                .SetBorderLeft(Border.NO_BORDER)
        //                .SetBorderTop(noBorder)
        //                .SetBorderBottom(noBorder));
        //        }
        //        else
        //        {
        //            table.AddCell(new Cell().SetBorder(noBorder));
        //            table.AddCell(new Cell().SetBorderRight(rightBorder)
        //                .SetBorderLeft(Border.NO_BORDER)
        //                .SetBorderTop(noBorder)
        //                .SetBorderBottom(noBorder));
        //        }
        //    }

        //    private void AddSalaryRowWithBordersNew(Table table,
        //(string label, decimal amount) earnings,
        //(string label, decimal amount) arrear,
        //(string label, decimal amount) deductions)
        //    {
        //        AddSalaryRowWithBordersNew(table,
        //            (earnings.label, earnings.amount, false),
        //            (arrear.label, arrear.amount, false),
        //            (deductions.label, deductions.amount, false));
        //    }

        // New End


        private void AddSalaryRowWithBorders(Table table,
            (string label, decimal amount) earnings,
            (string label, decimal amount) arrear,
            (string label, decimal amount) deductions)
        {
            AddSalaryRowWithBorders(table,
                (earnings.label, earnings.amount, false),
                (arrear.label, arrear.amount, false),
                (deductions.label, deductions.amount, false));
        }

       

        #region monthly payslip
        public async Task<EmployeeMonthlyIncentivePayslipDto> DownloadMonthlyIncentive(PaySlipModelDto model)
        {
            var secretKey = Config["AppSettings:EncSecret"];

            string sql = $@"SELECT EPS.*, E.WorkEmail Email, E.DateOfJoining JoiningDate, ISNULL(EBI.BankAccountName, '') BankAccountName
                            , ISNULL(EBI.BankAccountNumber,'')BankAccountNumber, ISNULL(EBI.BankBranchName,'') BankBranchName
                            , ISNULL(EBI.BankName, '') BankName , ISNULL(E.WalletNumber,'') WalletNumber
                            FROM EmployeeMonthlyIncentiveInfo EPS
                            left join Employee E on EPS.EmployeeID = E.EmployeeID
                            left join EmployeeBankInfo EBI on EPS.EmployeeID=EBI.EmployeeID
                            WHERE EPS.EmployeeID={AppContexts.User.EmployeeID} and PARSENAME(REPLACE(EPS.IncentiveMonth, '-', '.'), 1)={model.fiscalYear} and PARSENAME(REPLACE(EPS.IncentiveMonth, '-', '.'), 2)='{model.monthName}'";
            var incentive = EmployeeRepo.GetModelData<EmployeeMonthlyIncentivePayslipDto>(sql);
            EmployeeMonthlyIncentivePayslipDto result = new EmployeeMonthlyIncentivePayslipDto();
            if (incentive.IsNotNull())
            {
                EmployeeMonthlyIncentivePayslipDto pay = new EmployeeMonthlyIncentivePayslipDto
                {
                    PATID = incentive.PATID,
                    IncentiveMonth = incentive.IncentiveMonth, //month,
                    DisbursementDate = Convert.ToDateTime(incentive.DisbursementDate),
                    EmployeeID = incentive.EmployeeID,
                    EmployeeCode = incentive.EmployeeCode,
                    Designation = incentive.Designation,
                    Division = incentive.Division,
                    EmployeeName = incentive.EmployeeName,
                    AdjustedKPIPerformanceScore = PayrollDecrypt(Convert.ToString(incentive.AdjustedKPIPerformanceScore), secretKey),
                    ESSAURating = PayrollDecrypt(Convert.ToString(incentive.ESSAURating), secretKey),
                    AttendanceAdherenceScore = PayrollDecrypt(Convert.ToString(incentive.AttendanceAdherenceScore), secretKey),
                    EligibleIncentive = PayrollDecrypt(Convert.ToString(incentive.EligibleIncentive), secretKey),
                    TotalEarnings = PayrollDecrypt(Convert.ToString(incentive.TotalEarnings), secretKey),
                    Adjustment = PayrollDecrypt(Convert.ToString(incentive.Adjustment), secretKey),
                    TotalAdjustment = PayrollDecrypt(Convert.ToString(incentive.TotalAdjustment), secretKey),
                    IncomeTax = PayrollDecrypt(Convert.ToString(incentive.IncomeTax), secretKey),
                    TotalDeduction = PayrollDecrypt(Convert.ToString(incentive.TotalDeduction), secretKey),
                    NetPayment = PayrollDecrypt(Convert.ToString(incentive.NetPayment), secretKey),
                    AmountInWords = PayrollDecrypt(Convert.ToString(incentive.AmountInWords), secretKey),
                    WalletAmount = PayrollDecrypt(Convert.ToString(incentive.WalletAmount), secretKey),

                    JoiningDate = incentive.JoiningDate,
                    BankAccountName = incentive.BankAccountName,
                    BankAccountNumber = incentive.BankAccountNumber,
                    BankBranchName = incentive.BankBranchName,
                    BankName = incentive.BankName,
                    WalletNumber = incentive.WalletNumber,
                    Email = incentive.Email,
                };
                result = pay;
            }

            return await Task.FromResult(result);
        }

        public async Task<byte[]> GenerateMonthlyIncentiveAsync(EmployeeMonthlyIncentivePayslipDto paySlip)
        {
            string attachmentFolder = "upload\\attachments";
            string fontFolder = "upload\\fonts";
            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);

                Document document = new Document(pdf);

                // Load Arial font (assuming the font file is in your project or system font folder)
                IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();

                string arialFontPath = System.IO.Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot", fontFolder, "Arial.ttf");
                PdfFont arialFont = PdfFontFactory.CreateFont(arialFontPath, PdfEncodings.IDENTITY_H);

                // Now set the Arial font for the document or specific elements
                document.SetFont(arialFont);


                // Create a 3-column header table
                Table header = new Table(new float[] { 1, 3, 1 }).UseAllAvailableWidth();
                header.SetBorder(Border.NO_BORDER);

                // Left column for logo
                Cell logoCell = new Cell();
                string logoPath = System.IO.Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot", attachmentFolder, "nagad_logo.jpeg");
                if (File.Exists(logoPath))
                {
                    ImageData imageData = ImageDataFactory.Create(logoPath);
                    Image logo = new Image(imageData);
                    logo.SetWidth(102); // Adjust width as needed
                    logoCell.Add(logo);
                }
                logoCell.SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE);

                // Right column for text
                Cell textCell = new Cell();
                textCell.Add(new Paragraph("Nagad Ltd.")
                        .SetFontSize(16)
                        .SetBold()
                        .SetFontColor(ColorConstants.RED)
                        .SetTextAlignment(TextAlignment.CENTER))
                    .Add(new Paragraph("Delta Dahlia Tower (Level 14); 36, Kemal Ataturk Avenue,\nBanani, Dhaka - 1213, Bangladesh")
                        .SetFontSize(8).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph("Pay slip")
                        .SetFontSize(13).SetBold().SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph($"Incentive for the month of {paySlip.IncentiveMonth}")
                        .SetFontSize(12).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE);

                Cell emptyCell = new Cell();
                emptyCell.Add(new Paragraph(" this is empty cell")
                        .SetFontColor(ColorConstants.WHITE))
                    .SetBorder(Border.NO_BORDER);


                header.AddCell(logoCell);
                header.AddCell(textCell);
                header.AddCell(emptyCell);

                document.Add(header);

                // Pay slip title
                //document.Add(new Paragraph("Pay slip")
                //    .SetTextAlignment(TextAlignment.CENTER)
                //    .SetFontSize(16)
                //    .SetBold());

                //// Salary period
                //document.Add(new Paragraph($"Salary for the month of {paySlip.DisbursementDate:MMMM-yyyy}")
                //    .SetTextAlignment(TextAlignment.CENTER));

                // Disbursement Date
                document.Add(new Paragraph($"Disbursement Date: {paySlip.DisbursementDate:dd MMM, yyyy}")
                    .SetFontSize(8).SetBold()
                        .SetFontColor(ColorConstants.GRAY)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetMarginBottom(10));

                // Employee Information
                Table employeeInfo = new Table(new float[] { 2, 1, 2, 2, 1, 2 }).UseAllAvailableWidth();
                employeeInfo.SetBorder(new SolidBorder(1));

                // Header cell
                Cell infoHeader = new Cell(1, 6)
                    .Add(new Paragraph("Employee Information:").SetFontSize(10).SetBold())
                    .SetBorder(new SolidBorder(1));
                employeeInfo.AddCell(infoHeader);

                // Employee details
                AddEmployeeInfoRow(employeeInfo, "Employee ID", paySlip.EmployeeCode, "Name", paySlip.EmployeeName);
                AddEmployeeInfoRow(employeeInfo, "Division", paySlip.Division, "Designation", paySlip.Designation);
                AddEmployeeInfoRow(employeeInfo, "Joining Date", paySlip.JoiningDate.ToString("dd MMM yyyy"), "Email", paySlip.Email);

                document.Add(employeeInfo);

                // achievement details
                document.Add(new Paragraph("Achievement Details:").SetFontSize(10).SetBold()
                    .SetMarginTop(10)
                    .SetMarginBottom(5));
                Table acheTable = new Table(new float[] { 3, 1, 3 }).UseAllAvailableWidth();
                acheTable.SetBorder(Border.NO_BORDER).SetPadding(5);

                acheTable.AddCell(new Cell().Add(new Paragraph("Adjusted KPI Performance Score (out of 100)").SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER).SetBold()).SetBorder(new SolidBorder(1));
                acheTable.AddCell(new Cell().Add(new Paragraph("ESSAU Rating").SetFontSize(10)).SetBold())
                    .SetTextAlignment(TextAlignment.CENTER).SetBorder(new SolidBorder(1));
                acheTable.AddCell(new Cell().Add(new Paragraph("Attendance and Adherence Quality Score").SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER).SetBold()).SetBorder(new SolidBorder(1));

                acheTable.AddCell(new Cell().Add(new Paragraph(paySlip.AdjustedKPIPerformanceScore).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1));
                acheTable.AddCell(new Cell().Add(new Paragraph(paySlip.ESSAURating).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1));
                acheTable.AddCell(new Cell().Add(new Paragraph(paySlip.AttendanceAdherenceScore).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1));


                document.Add(acheTable);

                // Salary Details Header
                document.Add(new Paragraph("Incentive Details:").SetFontSize(10).SetBold()
                    .SetMarginTop(10)
                    .SetMarginBottom(5));

                float[] columnWidths = new float[] { 2, 1, 2, 1, 2, 1 };

                Table salaryTable = new Table(columnWidths).UseAllAvailableWidth();
                salaryTable.SetBorder(Border.NO_BORDER);

                // Headers
                Cell earningsHeader = new Cell(1, 2)
                    .Add(new Paragraph("Earnings (BDT)").SetFontSize(10).SetBold().SetFont(arialFont))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(new SolidBorder(1));

                Cell arrearHeader = new Cell(1, 2)
                    .Add(new Paragraph("Adjustment (BDT)").SetFontSize(10).SetBold().SetFont(arialFont))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(new SolidBorder(1));

                Cell deductionsHeader = new Cell(1, 2)
                    .Add(new Paragraph("Deductions (BDT)").SetFontSize(10).SetBold()).SetFont(arialFont)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(new SolidBorder(1));

                salaryTable.AddCell(earningsHeader);
                salaryTable.AddCell(arrearHeader);
                salaryTable.AddCell(deductionsHeader);

                // Add salary components
                AddSalaryRowWithBorders(salaryTable,
                    ("Eligible Incentive ", paySlip.EligibleIncentive.stringToDecimal()),
                    ("Adjustment", paySlip.Adjustment.stringToDecimal()),
                    ("Income Tax          ", paySlip.IncomeTax.stringToDecimal()));


                // Add totals with box borders
                //AddSalaryRowWithBorders(salaryTable,
                //    ("Total      : ", paySlip.TotalEarnings.stringToDecimal(), true),
                //    (" ", paySlip.TotalAdjustment.stringToDecimal(), true),
                //    (" ", paySlip.TotalDeduction.stringToDecimal(), true));

                Cell totalEarningCell1 = new Cell()
                    .Add(new Paragraph("Total                :").SetFontSize(10).SetBold())
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetBorderLeft(new SolidBorder(1))
                    .SetBorderRight(Border.NO_BORDER)
                    .SetBorderTop(new SolidBorder(1))
                    .SetBorderBottom(new SolidBorder(1)); // Set border for this cell

                Cell totalEarningCell2 = new Cell()
                    .Add(new Paragraph(Math.Abs(paySlip.TotalEarnings.stringToDecimal()) == 0 ? "-" : paySlip.TotalEarnings.stringToDecimal().ToString("N2")).SetFontSize(10).SetBold())
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetBorderRight(new SolidBorder(1))
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetBorderTop(new SolidBorder(1))
                    .SetBorderBottom(new SolidBorder(1)); // Set border for this cell

                Cell totalAdjustCell = new Cell(1, 2)
                    .Add(new Paragraph(Math.Abs(paySlip.TotalAdjustment.stringToDecimal()) == 0 ? "-" : paySlip.TotalAdjustment.stringToDecimal() > 0 ? paySlip.TotalAdjustment.stringToDecimal().ToString("N2") : "(" + Math.Abs(paySlip.TotalAdjustment.stringToDecimal()).ToString("N2") + ")")
                    .SetFontSize(10).SetBold()
                    .SetTextAlignment(TextAlignment.RIGHT))
                    .SetBorder(new SolidBorder(1)); // Set border for this cell

                Cell totalDeductCell = new Cell(1, 2)
                    .Add(new Paragraph(Math.Abs(paySlip.TotalDeduction.stringToDecimal()) == 0 ? "-" : paySlip.TotalDeduction.stringToDecimal().ToString("N2"))
                    .SetFontSize(10).SetBold()
                    .SetTextAlignment(TextAlignment.RIGHT))
                    .SetBorder(new SolidBorder(1)); // Set border for this cell

                salaryTable.AddCell(totalEarningCell1);
                salaryTable.AddCell(totalEarningCell2);
                salaryTable.AddCell(totalAdjustCell);
                salaryTable.AddCell(totalDeductCell);


                salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
                salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
                salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
                salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
                salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
                salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));

                // Add Net Payable row
                Cell netPayableLabelCell = new Cell(1, 4)
                    .Add(new Paragraph("Net Payable    :").SetFontSize(10).SetBold())
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetBorder(new SolidBorder(1)); // Set border for this cell

                Cell netPayableAmountCell = new Cell(1, 2)
                    .Add(new Paragraph(paySlip.NetPayment.stringToDecimal() == 0 ? "-" : paySlip.NetPayment.stringToDecimal().ToString("N2"))
                    .SetFontSize(10).SetBold()
                    .SetTextAlignment(TextAlignment.RIGHT))
                    .SetBorder(new SolidBorder(1)); // Set border for this cell

                salaryTable.AddCell(netPayableLabelCell);
                salaryTable.AddCell(netPayableAmountCell);
                document.Add(salaryTable);

                // Add amount in words
                document.Add(new Paragraph("In words : " + paySlip.AmountInWords)
                    .SetMarginTop(30).SetBold().SetFontSize(10)
                    .SetMarginBottom(5));

                // Add confidential note
                document.Add(new Paragraph("[N.B : Confidential Report. Please Do Not Share with Anyone]")
                    .SetFontSize(9).SetBold()
                    .SetMarginBottom(10));

                // Payment Methods Table
                document.Add(new Paragraph("Payment Methods:")
                    .SetMarginTop(40).SetBold().SetFontSize(10)
                    .SetMarginBottom(5));

                Table paymentTable = new Table(4).UseAllAvailableWidth();
                paymentTable.SetBorder(new SolidBorder(1));

                // Add headers
                paymentTable.AddCell(new Cell().Add(new Paragraph("Payment Type").SetFontSize(10).SetBold()).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph("Bank/Wallet Name").SetFontSize(10).SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph("Account/Wallet Number").SetFontSize(10).SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph("Amount (BDT)").SetFontSize(10).SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1)));

                // Add payment methods

                paymentTable.AddCell(new Cell().Add(new Paragraph("Wallet Transfer"))
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph("Nagad"))
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.WalletNumber)).SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.WalletAmount.stringToDecimal() == 0 ? "-" : paySlip.WalletAmount.stringToDecimal().ToString("N2")))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetBorder(new SolidBorder(1)));



                document.Add(paymentTable);

                string[] footerTexts = new string[]
                        {
            " This is a system generated report, it does not require any signature.",
            "Copyright ©",
            $"{DateTime.Now.Year} Nagad Ltd. All rights reserved."
                        };
                string footerData = $"Page {pdf.GetNumberOfPages()}"; // Dynamic page number
                string printedDate = DateTime.Now.ToString("dd MMMM yyyy, hh:mm tt"); // Format the printed date
                pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new FooterEventHandler(footerTexts, printedDate));



                document.Close();
                return ms.ToArray();
            }
        }
        #endregion


        #region Regular payslip
        public async Task<RegularIncentivePayslipDto> DownloadRegularIncentive(PaySlipModelDto model)
        {
            var secretKey = Config["AppSettings:EncSecret"];

            string sql = $@"SELECT EPS.*, E.WorkEmail Email, E.DateOfJoining JoiningDate, ISNULL(EBI.BankAccountName, '') BankAccountName
                            , ISNULL(EBI.BankAccountNumber,'')BankAccountNumber, ISNULL(EBI.BankBranchName,'') BankBranchName
                            , ISNULL(EBI.BankName, '') BankName , ISNULL(E.WalletNumber,'') WalletNumber, PARSENAME(REPLACE(pa.ActivityPeriod, '-', '.'), 2) QuarterName
                            FROM EmployeeRegularIncentiveInfo EPS
                            left join Employee E on EPS.EmployeeID = E.EmployeeID
                            left join EmployeeBankInfo EBI on EPS.EmployeeID=EBI.EmployeeID
							left join PayrollAuditTrial pa on pa.PATID=EPS.PATID
                            WHERE EPS.EmployeeID={AppContexts.User.EmployeeID} and PARSENAME(REPLACE(pa.ActivityPeriod, '-', '.'), 1)={model.fiscalYear} and PARSENAME(REPLACE(pa.ActivityPeriod, '-', '.'), 2)='{model.Quarter}'";
            var incentive = EmployeeRepo.GetModelData<RegularIncentivePayslipDto>(sql);
            RegularIncentivePayslipDto result = new RegularIncentivePayslipDto();
            if (incentive.IsNotNull())
            {
                RegularIncentivePayslipDto pay = new RegularIncentivePayslipDto
                {
                    PATID = incentive.PATID,
                    IncentiveType = incentive.IncentiveType,
                    DisbursementDate = Convert.ToDateTime(incentive.DisbursementDate),
                    EmployeeID = incentive.EmployeeID,
                    EmployeeCode = incentive.EmployeeCode,
                    Designation = incentive.Designation,
                    Division = incentive.Division,
                    EmployeeName = incentive.EmployeeName,
                    Particular1 = Convert.ToString(incentive.Particular1),
                    BasicEntitlement1 = PayrollDecrypt(Convert.ToString(incentive.BasicEntitlement1), secretKey),
                    Particular2 = Convert.ToString(incentive.Particular2),
                    BasicEntitlement2 = PayrollDecrypt(Convert.ToString(incentive.BasicEntitlement2), secretKey),
                    Particular3 = Convert.ToString(incentive.Particular3),
                    BasicEntitlement3 = PayrollDecrypt(Convert.ToString(incentive.BasicEntitlement3), secretKey),
                    Particular4 = Convert.ToString(incentive.Particular4),
                    BasicEntitlement4 = PayrollDecrypt(Convert.ToString(incentive.BasicEntitlement4), secretKey),
                    EligibleBonus = PayrollDecrypt(Convert.ToString(incentive.EligibleBonus), secretKey),
                    EligibleBonusTotal = PayrollDecrypt(Convert.ToString(incentive.EligibleBonusTotal), secretKey),
                    IncomeTax = PayrollDecrypt(Convert.ToString(incentive.IncomeTax), secretKey),
                    TotalDeduction = PayrollDecrypt(Convert.ToString(incentive.TotalDeduction), secretKey),
                    NetPayable = PayrollDecrypt(Convert.ToString(incentive.NetPayable), secretKey),
                    AmountInWords = PayrollDecrypt(Convert.ToString(incentive.AmountInWords), secretKey),
                    BankAmount = PayrollDecrypt(Convert.ToString(incentive.BankAmount), secretKey),

                    Particulars5 = Convert.ToString(incentive.Particulars5),
                    PerformanceRating1 = PayrollDecrypt(Convert.ToString(incentive.PerformanceRating1), secretKey),
                    Particulars6 = Convert.ToString(incentive.Particulars6),
                    PerformanceRating2 = PayrollDecrypt(Convert.ToString(incentive.PerformanceRating2), secretKey),


                    JoiningDate = incentive.JoiningDate,
                    BankAccountName = incentive.BankAccountName,
                    BankAccountNumber = incentive.BankAccountNumber,
                    BankBranchName = incentive.BankBranchName,
                    BankName = incentive.BankName,
                    WalletNumber = incentive.WalletNumber,
                    Email = incentive.Email,
                    QuarterName = incentive.QuarterName
                };
                result = pay;
            }

            return await Task.FromResult(result);
        }

        public async Task<byte[]> GenerateRegularIncentiveAsync(RegularIncentivePayslipDto paySlip)
        {
            string attachmentFolder = "upload\\attachments";
            string fontFolder = "upload\\fonts";
            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);

                Document document = new Document(pdf);

                // Load Arial font (assuming the font file is in your project or system font folder)
                IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();

                string arialFontPath = System.IO.Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot", fontFolder, "Arial.ttf");
                PdfFont arialFont = PdfFontFactory.CreateFont(arialFontPath, PdfEncodings.IDENTITY_H);

                // Now set the Arial font for the document or specific elements
                document.SetFont(arialFont);


                // Create a 3-column header table
                Table header = new Table(new float[] { 1, 3, 1 }).UseAllAvailableWidth();
                header.SetBorder(Border.NO_BORDER);

                // Left column for logo
                Cell logoCell = new Cell();
                string logoPath = System.IO.Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot", attachmentFolder, "nagad_logo.jpeg");
                if (File.Exists(logoPath))
                {
                    ImageData imageData = ImageDataFactory.Create(logoPath);
                    Image logo = new Image(imageData);
                    logo.SetWidth(102); // Adjust width as needed
                    logoCell.Add(logo);
                }
                logoCell.SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE);

                // Right column for text
                Cell textCell = new Cell();
                textCell.Add(new Paragraph("Nagad Ltd.")
                        .SetFontSize(16)
                        .SetBold()
                        .SetFontColor(ColorConstants.RED)
                        .SetTextAlignment(TextAlignment.CENTER))
                    .Add(new Paragraph("Delta Dahlia Tower (Level 14); 36, Kemal Ataturk Avenue,\nBanani, Dhaka - 1213, Bangladesh")
                        .SetFontSize(8).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph("Pay slip")
                        .SetFontSize(13).SetBold().SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph($"Performance Bonus for {paySlip.IncentiveType}")
                        .SetFontSize(12).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE);

                Cell emptyCell = new Cell();
                emptyCell.Add(new Paragraph(" this is empty cell")
                        .SetFontColor(ColorConstants.WHITE))
                    .SetBorder(Border.NO_BORDER);


                header.AddCell(logoCell);
                header.AddCell(textCell);
                header.AddCell(emptyCell);

                document.Add(header);

                // Pay slip title
                //document.Add(new Paragraph("Pay slip")
                //    .SetTextAlignment(TextAlignment.CENTER)
                //    .SetFontSize(16)
                //    .SetBold());

                //// Salary period
                //document.Add(new Paragraph($"Salary for the month of {paySlip.DisbursementDate:MMMM-yyyy}")
                //    .SetTextAlignment(TextAlignment.CENTER));

                // Disbursement Date
                document.Add(new Paragraph($"Disbursement Date: {paySlip.DisbursementDate:dd MMMM, yyyy}")
                    .SetFontSize(8).SetBold()
                        .SetFontColor(ColorConstants.GRAY)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetMarginBottom(10));

                // Employee Information
                Table employeeInfo = new Table(new float[] { 2, 1, 2, 2, 1, 2 }).UseAllAvailableWidth();
                employeeInfo.SetBorder(new SolidBorder(1));

                // Header cell
                Cell infoHeader = new Cell(1, 6)
                    .Add(new Paragraph("Employee Information:").SetFontSize(9).SetBold())
                    .SetBorder(new SolidBorder(1));
                employeeInfo.AddCell(infoHeader);

                // Employee details
                AddEmployeeInfoRow(employeeInfo, "Employee ID", paySlip.EmployeeCode, "Name", paySlip.EmployeeName);
                AddEmployeeInfoRow(employeeInfo, "Division", paySlip.Division, "Designation", paySlip.Designation);
                AddEmployeeInfoRow(employeeInfo, "Joining Date", paySlip.JoiningDate.ToString("dd MMMM yyyy"), "Email", paySlip.Email);

                document.Add(employeeInfo);

                // achievement details
                document.Add(new Paragraph("Achievement Details:").SetFontSize(10).SetBold()
                    .SetMarginTop(10)
                    .SetMarginBottom(5));
                Table acheTable = new Table(new float[] { 1, 1 }).UseAllAvailableWidth();
                acheTable.SetBorder(Border.NO_BORDER).SetPadding(5);

                if(paySlip.QuarterName == "H1" || paySlip.QuarterName == "H2")
                {
                    acheTable.AddCell(new Cell().Add(new Paragraph("Particular").SetFontSize(10))
                      .SetTextAlignment(TextAlignment.CENTER).SetBold()).SetBorder(new SolidBorder(1));
                    acheTable.AddCell(new Cell().Add(new Paragraph("Performance Rating").SetFontSize(10))
                        .SetTextAlignment(TextAlignment.CENTER).SetBold()).SetBorder(new SolidBorder(1));
                }
                else
                {
                    acheTable.AddCell(new Cell().Add(new Paragraph("Particulars").SetFontSize(10))
                        .SetTextAlignment(TextAlignment.CENTER).SetBold()).SetBorder(new SolidBorder(1));
                    acheTable.AddCell(new Cell().Add(new Paragraph("No. of Basic Entitlement").SetFontSize(10))
                        .SetTextAlignment(TextAlignment.CENTER).SetBold()).SetBorder(new SolidBorder(1));
                }
                //For H1
                if (paySlip.QuarterName == "H1")
                {
                        acheTable.AddCell(new Cell().Add(new Paragraph(paySlip.Particulars5).SetFontSize(10))
                      .SetTextAlignment(TextAlignment.CENTER).SetBold()).SetBorder(new SolidBorder(1));
                        acheTable.AddCell(new Cell().Add(new Paragraph(paySlip.PerformanceRating1).SetFontSize(10))
                            .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1));
                }
                //For H2
                if (paySlip.QuarterName == "H2")
                {
                        acheTable.AddCell(new Cell().Add(new Paragraph(paySlip.Particulars6).SetFontSize(10))
                .SetTextAlignment(TextAlignment.CENTER).SetBold()).SetBorder(new SolidBorder(1));
                        acheTable.AddCell(new Cell().Add(new Paragraph(paySlip.PerformanceRating2).SetFontSize(10))
                            .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1));
                }

                //For Q1 Or Q2
                if (paySlip.QuarterName == "Q1" || paySlip.QuarterName == "Q2")
                {
                    if (paySlip.QuarterName == "Q1")
                    {
                        acheTable.AddCell(new Cell().Add(new Paragraph(paySlip.Particular1).SetFontSize(10))
                      .SetTextAlignment(TextAlignment.CENTER).SetBold()).SetBorder(new SolidBorder(1));
                        acheTable.AddCell(new Cell().Add(new Paragraph(paySlip.BasicEntitlement1).SetFontSize(10))
                            .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1));
                    }
                    if (paySlip.QuarterName == "Q2")
                    {
                        acheTable.AddCell(new Cell().Add(new Paragraph(paySlip.Particular2).SetFontSize(10))
                      .SetTextAlignment(TextAlignment.CENTER).SetBold()).SetBorder(new SolidBorder(1));
                        acheTable.AddCell(new Cell().Add(new Paragraph(paySlip.BasicEntitlement2).SetFontSize(10))
                        .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1));
                    }
                }
                //For Q3 Or Q4
                if (paySlip.QuarterName == "Q3" || paySlip.QuarterName == "Q4")
                {
                    if (paySlip.QuarterName == "Q3")
                    {
                        acheTable.AddCell(new Cell().Add(new Paragraph(paySlip.Particular3).SetFontSize(10))
                .SetTextAlignment(TextAlignment.CENTER).SetBold()).SetBorder(new SolidBorder(1));
                        acheTable.AddCell(new Cell().Add(new Paragraph(paySlip.BasicEntitlement3).SetFontSize(10))
                            .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1));
                    }
                    if (paySlip.QuarterName == "Q4")
                    {
                        acheTable.AddCell(new Cell().Add(new Paragraph(paySlip.Particular4).SetFontSize(10))
                .SetTextAlignment(TextAlignment.CENTER).SetBold()).SetBorder(new SolidBorder(1));
                        acheTable.AddCell(new Cell().Add(new Paragraph(paySlip.BasicEntitlement4).SetFontSize(10))
                            .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1));
                    }
                }

                document.Add(acheTable);

                // Salary Details Header
                document.Add(new Paragraph("Performance Bonus Details:").SetFontSize(10).SetBold()
                    .SetMarginTop(10)
                    .SetMarginBottom(5));

                float[] columnWidths = new float[] { 1, 1, 1, 1 };

                Table salaryTable = new Table(columnWidths).UseAllAvailableWidth();
                salaryTable.SetBorder(Border.NO_BORDER);

                // Headers
                Cell earningsHeader = new Cell(1, 2)
                    .Add(new Paragraph("Eligible Bonus(BDT)").SetFontSize(10).SetBold().SetFont(arialFont))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(new SolidBorder(1));

                Cell deductionsHeader = new Cell(1, 2)
                    .Add(new Paragraph("Deductions (BDT)").SetFontSize(10).SetBold()).SetFont(arialFont)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(new SolidBorder(1));

                salaryTable.AddCell(earningsHeader);
                salaryTable.AddCell(deductionsHeader);

                // Add salary components
                salaryTable.AddCell(new Cell().Add(new Paragraph(paySlip.EligibleBonus).SetFontSize(10))
                      .SetTextAlignment(TextAlignment.LEFT).SetBorderLeft(new SolidBorder(1))
                     .SetBorderRight(Border.NO_BORDER)
                     .SetBorderTop(new SolidBorder(1))
                     .SetBorderBottom(new SolidBorder(1)));
                salaryTable.AddCell(new Cell().Add(new Paragraph(paySlip.EligibleBonusTotal.stringToDecimal() == 0 ? "-" : paySlip.EligibleBonusTotal.stringToDecimal().ToString("N2")).SetFontSize(10))
                      .SetTextAlignment(TextAlignment.RIGHT).SetBorderRight(new SolidBorder(1))
                     .SetBorderLeft(Border.NO_BORDER)
                     .SetBorderTop(new SolidBorder(1))
                     .SetBorderBottom(new SolidBorder(1)));

                salaryTable.AddCell(new Cell().Add(new Paragraph("Income Tax                  ").SetFontSize(10))
                    .SetTextAlignment(TextAlignment.LEFT).SetBorderLeft(new SolidBorder(1))
                     .SetBorderRight(Border.NO_BORDER)
                     .SetBorderTop(new SolidBorder(1))
                     .SetBorderBottom(new SolidBorder(1)));
                salaryTable.AddCell(new Cell().Add(new Paragraph(paySlip.IncomeTax.stringToDecimal() == 0 ? "-" : paySlip.IncomeTax.stringToDecimal().ToString("N2")).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.RIGHT).SetBorderRight(new SolidBorder(1))
                     .SetBorderLeft(Border.NO_BORDER)
                     .SetBorderTop(new SolidBorder(1))
                     .SetBorderBottom(new SolidBorder(1)));


                Cell totalEarningCell1 = new Cell()
                     .Add(new Paragraph("Total                :").SetFontSize(10).SetBold())
                     .SetTextAlignment(TextAlignment.LEFT)
                     .SetBorderLeft(new SolidBorder(1))
                     .SetBorderRight(Border.NO_BORDER)
                     .SetBorderTop(new SolidBorder(1))
                     .SetBorderBottom(new SolidBorder(1)); // Set border for this cell

                Cell totalEarningCell2 = new Cell()
                    .Add(new Paragraph(Math.Abs(paySlip.EligibleBonusTotal.stringToDecimal()) == 0 ? "-" : paySlip.EligibleBonusTotal.stringToDecimal().ToString("N2")).SetFontSize(10).SetBold())
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetBorderRight(new SolidBorder(1))
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetBorderTop(new SolidBorder(1))
                    .SetBorderBottom(new SolidBorder(1)); // Set border for this cell



                Cell totalDeductCell = new Cell(1, 2)
                    .Add(new Paragraph(Math.Abs(paySlip.TotalDeduction.stringToDecimal()) == 0 ? "-" : paySlip.TotalDeduction.stringToDecimal().ToString("N2"))
                    .SetFontSize(10).SetBold()
                    .SetTextAlignment(TextAlignment.RIGHT))
                    .SetBorder(new SolidBorder(1)); // Set border for this cell

                salaryTable.AddCell(totalEarningCell1);
                salaryTable.AddCell(totalEarningCell2);
                salaryTable.AddCell(totalDeductCell);




                salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
                salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
                salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
                salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));

                // Add Net Payable row
                Cell netPayableLabelCell = new Cell(1, 2)
                    .Add(new Paragraph("Net Payable    :").SetFontSize(10).SetBold())
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetBorder(new SolidBorder(1)); // Set border for this cell

                Cell netPayableAmountCell = new Cell(1, 2)
                    .Add(new Paragraph(paySlip.NetPayable.stringToDecimal() == 0 ? "-" : paySlip.NetPayable.stringToDecimal().ToString("N2"))
                    .SetFontSize(10).SetBold()
                    .SetTextAlignment(TextAlignment.RIGHT))
                    .SetBorder(new SolidBorder(1)); // Set border for this cell

                salaryTable.AddCell(netPayableLabelCell);
                salaryTable.AddCell(netPayableAmountCell);
                document.Add(salaryTable);

                // Add amount in words
                document.Add(new Paragraph("In words : " + paySlip.AmountInWords)
                    .SetMarginTop(30).SetBold().SetFontSize(10)
                    .SetMarginBottom(5));

                // Add confidential note
                document.Add(new Paragraph("[N.B : Confidential Report. Please Do Not Share with Anyone]")
                    .SetFontSize(9).SetBold()
                    .SetMarginBottom(10));

                // Payment Methods Table
                document.Add(new Paragraph("Payment Methods:")
                    .SetMarginTop(40).SetBold().SetFontSize(10)
                    .SetMarginBottom(5));

                Table paymentTable = new Table(4).UseAllAvailableWidth();
                paymentTable.SetBorder(new SolidBorder(1));

                // Add headers
                paymentTable.AddCell(new Cell().Add(new Paragraph("Payment Type").SetFontSize(10).SetBold()).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph("Bank/Wallet Name").SetFontSize(10).SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph("Account/Wallet Number").SetFontSize(10).SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph("Amount (BDT)").SetFontSize(10).SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1)));

                // Add payment methods

                paymentTable.AddCell(new Cell().Add(new Paragraph("Bank Transfer"))
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.BankName))
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.BankAccountNumber)).SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.BankAmount.stringToDecimal() == 0 ? "-" : paySlip.BankAmount.stringToDecimal().ToString("N2")))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetBorder(new SolidBorder(1)));



                document.Add(paymentTable);

                string[] footerTexts = new string[]
                        {
            " This is a system generated report, it does not require any signature.",
            "Copyright ©",
            $"{DateTime.Now.Year} Nagad Ltd. All rights reserved."
                        };
                string footerData = $"Page {pdf.GetNumberOfPages()}"; // Dynamic page number
                string printedDate = DateTime.Now.ToString("dd MMMM yyyy, hh:mm tt"); // Format the printed date
                pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new FooterEventHandler(footerTexts, printedDate));



                document.Close();
                return ms.ToArray();
            }
        }
        #endregion

        #region Festival Bonus payslip
        public async Task<EmployeeFestivalBonusPayslipDto> DownloadFestivalBonus(PaySlipModelDto model)
        {
            var secretKey = Config["AppSettings:EncSecret"];

            string sql = $@"SELECT EPS.*, E.WorkEmail Email, E.DateOfJoining JoiningDate, ISNULL(EBI.BankAccountName, '') BankAccountName
                            , ISNULL(EBI.BankAccountNumber,'')BankAccountNumber, ISNULL(EBI.BankBranchName,'') BankBranchName
                            , ISNULL(EBI.BankName, '') BankName , ISNULL(E.WalletNumber,'') WalletNumber
                            FROM EmployeeFestivalBonusInfo EPS
                            left join Employee E on EPS.EmployeeID = E.EmployeeID
                            left join EmployeeBankInfo EBI on EPS.EmployeeID=EBI.EmployeeID
                            LEFT JOIN PayrollAuditTrial PA on PA.PATID = EPS.PATID
                            WHERE EPS.EmployeeID={AppContexts.User.EmployeeID} and PARSENAME(REPLACE(EPS.BonusMonth, '-', '.'), 1)={model.fiscalYear} and PA.FestivalBonusTypeID='{model.FesivalBonusTypeID}'";
            var incentive = EmployeeRepo.GetModelData<EmployeeFestivalBonusPayslipDto>(sql);
            EmployeeFestivalBonusPayslipDto result = new EmployeeFestivalBonusPayslipDto();
            if (incentive.IsNotNull())
            {
                EmployeeFestivalBonusPayslipDto pay = new EmployeeFestivalBonusPayslipDto
                {
                    PATID = incentive.PATID,
                    BonusMonth = incentive.BonusMonth,
                    DisbursementDate = Convert.ToDateTime(incentive.DisbursementDate),
                    EmployeeID = incentive.EmployeeID,
                    EmployeeCode = incentive.EmployeeCode,
                    Designation = incentive.Designation,
                    EmployeeName = incentive.EmployeeName,
                    EarningField1 = PayrollDecrypt(Convert.ToString(incentive.EarningField1), secretKey),
                    EarningValue1 = PayrollDecrypt(Convert.ToString(incentive.EarningValue1), secretKey),
                    TotalEarnings = PayrollDecrypt(Convert.ToString(incentive.TotalEarnings), secretKey),
                    DeductionField1 = PayrollDecrypt(Convert.ToString(incentive.DeductionField1), secretKey),
                    DeductionValue1 = PayrollDecrypt(Convert.ToString(incentive.DeductionValue1), secretKey),
                    DeductionField2 = PayrollDecrypt(Convert.ToString(incentive.DeductionField2), secretKey),
                    DeductionValue2 = PayrollDecrypt(Convert.ToString(incentive.DeductionValue2), secretKey),
                    TotalDeductions = PayrollDecrypt(Convert.ToString(incentive.TotalDeductions), secretKey),
                    NetPayment = PayrollDecrypt(Convert.ToString(incentive.NetPayment), secretKey),
                    AmountInWords = PayrollDecrypt(Convert.ToString(incentive.AmountInWords), secretKey),
                    BankAmount = PayrollDecrypt(Convert.ToString(incentive.BankAmount), secretKey),
                    WalletAmount = PayrollDecrypt(Convert.ToString(incentive.WalletAmount), secretKey),
                    CashOutCharge = PayrollDecrypt(Convert.ToString(incentive.CashOutCharge), secretKey),

                    JoiningDate = incentive.JoiningDate,
                    BankAccountName = incentive.BankAccountName,
                    BankAccountNumber = incentive.BankAccountNumber,
                    BankBranchName = incentive.BankBranchName,
                    BankName = incentive.BankName,
                    WalletNumber = incentive.WalletNumber,
                    Email = incentive.Email,
                };
                result = pay;
            }

            return await Task.FromResult(result);
        }

        public async Task<byte[]> GenerateFestivalBonusAsync(EmployeeFestivalBonusPayslipDto paySlip)
        {
            string attachmentFolder = "upload\\attachments";
            string fontFolder = "upload\\fonts";
            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);

                Document document = new Document(pdf);

                // Load Arial font (assuming the font file is in your project or system font folder)
                IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();

                string arialFontPath = System.IO.Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot", fontFolder, "Arial.ttf");
                PdfFont arialFont = PdfFontFactory.CreateFont(arialFontPath, PdfEncodings.IDENTITY_H);

                // Now set the Arial font for the document or specific elements
                document.SetFont(arialFont);


                // Create a 3-column header table
                Table header = new Table(new float[] { 1, 3, 1 }).UseAllAvailableWidth();
                header.SetBorder(Border.NO_BORDER);

                // Left column for logo
                Cell logoCell = new Cell();
                string logoPath = System.IO.Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot", attachmentFolder, "nagad_logo.jpeg");
                if (File.Exists(logoPath))
                {
                    ImageData imageData = ImageDataFactory.Create(logoPath);
                    Image logo = new Image(imageData);
                    logo.SetWidth(102); // Adjust width as needed
                    logoCell.Add(logo);
                }
                logoCell.SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE);

                // Right column for text
                Cell textCell = new Cell();
                textCell.Add(new Paragraph("Nagad Ltd.")
                        .SetFontSize(16)
                        .SetBold()
                        .SetFontColor(ColorConstants.RED)
                        .SetTextAlignment(TextAlignment.CENTER))
                    .Add(new Paragraph("Delta Dahlia Tower (Level 14); 36, Kemal Ataturk Avenue,\nBanani, Dhaka - 1213, Bangladesh")
                        .SetFontSize(8).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph("Pay slip")
                        .SetFontSize(13).SetBold().SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph($"Bonus Pay Slip {paySlip.BonusMonth}")
                        .SetFontSize(12).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE);

                Cell emptyCell = new Cell();
                emptyCell.Add(new Paragraph(" this is empty cell")
                        .SetFontColor(ColorConstants.WHITE))
                    .SetBorder(Border.NO_BORDER);


                header.AddCell(logoCell);
                header.AddCell(textCell);
                header.AddCell(emptyCell);

                document.Add(header);

                // Pay slip title
                //document.Add(new Paragraph("Pay slip")
                //    .SetTextAlignment(TextAlignment.CENTER)
                //    .SetFontSize(16)
                //    .SetBold());

                //// Salary period
                //document.Add(new Paragraph($"Salary for the month of {paySlip.DisbursementDate:MMMM-yyyy}")
                //    .SetTextAlignment(TextAlignment.CENTER));

                // Disbursement Date
                document.Add(new Paragraph($"Disbursement Date: {paySlip.DisbursementDate:dd MMMM, yyyy}")
                    .SetFontSize(8).SetBold()
                        .SetFontColor(ColorConstants.GRAY)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetMarginBottom(10));

                // Employee Information
                Table employeeInfo = new Table(new float[] { 2, 1, 2, 2, 1, 2 }).UseAllAvailableWidth();
                employeeInfo.SetBorder(new SolidBorder(1));

                // Header cell
                Cell infoHeader = new Cell(1, 6)
                    .Add(new Paragraph("Employee Details:").SetFontSize(10).SetBold())
                    .SetBorder(new SolidBorder(1));
                employeeInfo.AddCell(infoHeader);

                // Employee details
                AddEmployeeInfoRow(employeeInfo, "Employee ID", paySlip.EmployeeCode, "Name", paySlip.EmployeeName);
                AddEmployeeInfoRow(employeeInfo, "Designation", paySlip.Designation, "Joining Date", paySlip.JoiningDate.ToString("dd MMMM yyyy"));

                document.Add(employeeInfo);

                // achievement details
                document.Add(new Paragraph("Bonus Details (Component-wise Breakdown):").SetFontSize(10).SetBold()
                    .SetMarginTop(20)
                    .SetMarginBottom(5));



                float[] columnWidths = new float[] { 1, 1, 1, 1 };

                Table salaryTable = new Table(columnWidths).UseAllAvailableWidth();
                salaryTable.SetBorder(Border.NO_BORDER);

                // Headers
                Cell earningsHeader1 = new Cell()
                    .Add(new Paragraph("Earnings").SetFontSize(10).SetBold().SetFont(arialFont))
                    .SetTextAlignment(TextAlignment.LEFT).SetBorderLeft(new SolidBorder(1))
                     .SetBorderRight(Border.NO_BORDER)
                     .SetBorderTop(new SolidBorder(1))
                     .SetBorderBottom(new SolidBorder(1));
                Cell earningsHeader2 = new Cell()
                    .Add(new Paragraph("BDT").SetFontSize(10).SetBold().SetFont(arialFont))
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetBorderRight(new SolidBorder(1))
                     .SetBorderLeft(Border.NO_BORDER)
                     .SetBorderTop(new SolidBorder(1))
                     .SetBorderBottom(new SolidBorder(1));

                Cell deductionsHeader1 = new Cell()
                    .Add(new Paragraph("Deductions").SetFontSize(10).SetBold()).SetFont(arialFont)
                    .SetTextAlignment(TextAlignment.LEFT).SetBorderLeft(new SolidBorder(1))
                     .SetBorderRight(Border.NO_BORDER)
                     .SetBorderTop(new SolidBorder(1))
                     .SetBorderBottom(new SolidBorder(1));
                Cell deductionsHeader2 = new Cell()
                    .Add(new Paragraph("BDT").SetFontSize(10).SetBold()).SetFont(arialFont)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetBorderRight(new SolidBorder(1))
                     .SetBorderLeft(Border.NO_BORDER)
                     .SetBorderTop(new SolidBorder(1))
                     .SetBorderBottom(new SolidBorder(1));

                salaryTable.AddCell(earningsHeader1);
                salaryTable.AddCell(earningsHeader2);
                salaryTable.AddCell(deductionsHeader1);
                salaryTable.AddCell(deductionsHeader2);

                // Add salary components
                salaryTable.AddCell(new Cell().Add(new Paragraph(paySlip.EarningField1).SetFontSize(10))
                      .SetTextAlignment(TextAlignment.LEFT).SetBorderLeft(new SolidBorder(1))
                     .SetBorderRight(Border.NO_BORDER)
                     .SetBorderTop(new SolidBorder(1))
                     .SetBorderBottom(new SolidBorder(1)));
                salaryTable.AddCell(new Cell().Add(new Paragraph(paySlip.EarningValue1.stringToDecimal() == 0 ? "-" : paySlip.EarningValue1.stringToDecimal().ToString("N2")).SetFontSize(10))
                      .SetTextAlignment(TextAlignment.RIGHT).SetBorderRight(new SolidBorder(1))
                     .SetBorderLeft(Border.NO_BORDER)
                     .SetBorderTop(new SolidBorder(1))
                     .SetBorderBottom(new SolidBorder(1)));

                salaryTable.AddCell(new Cell().Add(new Paragraph(paySlip.DeductionField2).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.LEFT).SetBorderLeft(new SolidBorder(1))
                     .SetBorderRight(Border.NO_BORDER)
                     .SetBorderTop(new SolidBorder(1))
                     .SetBorderBottom(new SolidBorder(1)));
                salaryTable.AddCell(new Cell().Add(new Paragraph(paySlip.DeductionValue2.stringToDecimal() == 0 ? "-" : paySlip.DeductionValue2.stringToDecimal().ToString("N2")).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.RIGHT).SetBorderRight(new SolidBorder(1))
                     .SetBorderLeft(Border.NO_BORDER)
                     .SetBorderTop(new SolidBorder(1))
                     .SetBorderBottom(new SolidBorder(1)));


                Cell totalEarningCell1 = new Cell()
                     .Add(new Paragraph("Total Earnings          ").SetFontSize(10).SetBold())
                     .SetTextAlignment(TextAlignment.LEFT)
                     .SetBorderLeft(new SolidBorder(1))
                     .SetBorderRight(Border.NO_BORDER)
                     .SetBorderTop(new SolidBorder(1))
                     .SetBorderBottom(new SolidBorder(1)); // Set border for this cell

                Cell totalEarningCell2 = new Cell()
                    .Add(new Paragraph(Math.Abs(paySlip.TotalEarnings.stringToDecimal()) == 0 ? "-" : paySlip.TotalEarnings.stringToDecimal().ToString("N2")).SetFontSize(10).SetBold())
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetBorderRight(new SolidBorder(1))
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetBorderTop(new SolidBorder(1))
                    .SetBorderBottom(new SolidBorder(1)); // Set border for this cell



                Cell totalDeductionCell1 = new Cell()
                     .Add(new Paragraph("Total Deduction           ").SetFontSize(10).SetBold())
                     .SetTextAlignment(TextAlignment.LEFT)
                     .SetBorderLeft(new SolidBorder(1))
                     .SetBorderRight(Border.NO_BORDER)
                     .SetBorderTop(new SolidBorder(1))
                     .SetBorderBottom(new SolidBorder(1)); // Set border for this cell

                Cell totalDeductionCell2 = new Cell()
                    .Add(new Paragraph(Math.Abs(paySlip.TotalDeductions.stringToDecimal()) == 0 ? "-" : paySlip.TotalDeductions.stringToDecimal().ToString("N2")).SetFontSize(10).SetBold())
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetBorderRight(new SolidBorder(1))
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetBorderTop(new SolidBorder(1))
                    .SetBorderBottom(new SolidBorder(1)); // Set border for this cell


                salaryTable.AddCell(totalEarningCell1);
                salaryTable.AddCell(totalEarningCell2);
                salaryTable.AddCell(totalDeductionCell1);
                salaryTable.AddCell(totalDeductionCell2);




                salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
                salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
                salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));
                salaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetHeight(15));

                // Add Net Payable row
                Cell netPayableLabelCell = new Cell(1, 2)
                    .Add(new Paragraph("Net Payment    :  BDT. " + (paySlip.NetPayment.stringToDecimal() == 0 ? "-" : paySlip.NetPayment.stringToDecimal().ToString("N2"))).SetFontSize(10).SetBold())
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetBorder(Border.NO_BORDER); // Set border for this cell


                salaryTable.AddCell(netPayableLabelCell);
                document.Add(salaryTable);

                // Add amount in words
                document.Add(new Paragraph("In words   : " + paySlip.AmountInWords)
                    .SetMarginTop(30).SetBold().SetFontSize(10)
                    .SetMarginBottom(5));

                // Add confidential note
                document.Add(new Paragraph("[N.B : Confidential Report. Please Do Not Share with Anyone]")
                    .SetFontSize(9).SetBold()
                    .SetMarginBottom(10));

                // Payment Methods Table
                document.Add(new Paragraph("Payment Methods:")
                    .SetMarginTop(40).SetBold().SetFontSize(10)
                    .SetMarginBottom(5));

                Table paymentTable = new Table(4).UseAllAvailableWidth();
                paymentTable.SetBorder(new SolidBorder(1));

                // Add headers
                paymentTable.AddCell(new Cell().Add(new Paragraph("Payment Type").SetFontSize(10).SetBold()).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph("Bank/Wallet Name").SetFontSize(10).SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph("Account/Wallet Number").SetFontSize(10).SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph("Amount (BDT)").SetFontSize(10).SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)).SetBorder(new SolidBorder(1)));

                // Add payment methods

                paymentTable.AddCell(new Cell().Add(new Paragraph("Bank Transfer"))
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.BankName))
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.BankAccountNumber)).SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.BankAmount.stringToDecimal() == 0 ? "-" : paySlip.BankAmount.stringToDecimal().ToString("N2")))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetBorder(new SolidBorder(1)));


                paymentTable.AddCell(new Cell().Add(new Paragraph("Wallet Transfer"))
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph("Nagad"))
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.WalletNumber)).SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.WalletAmount.stringToDecimal() == 0 ? "-" : paySlip.WalletAmount.stringToDecimal().ToString("N2")))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetBorder(new SolidBorder(1)));


                paymentTable.AddCell(new Cell().Add(new Paragraph("Cash Out Charge"))
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(" "))
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(" ")).SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10).SetBorder(new SolidBorder(1)));
                paymentTable.AddCell(new Cell().Add(new Paragraph(paySlip.CashOutCharge.stringToDecimal() == 0 ? "-" : paySlip.CashOutCharge.stringToDecimal().ToString("N2")))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetBorder(new SolidBorder(1)));


                document.Add(paymentTable);

                string[] footerTexts = new string[]
                        {
            " This is a system generated report, it does not require any signature.",
            $"Copyright © {DateTime.Now.Year} Nagad Ltd. All rights reserved.",
                        };
                string footerData = $"Page {pdf.GetNumberOfPages()}"; // Dynamic page number
                string printedDate = DateTime.Now.ToString("dd MMMM yyyy, hh:mm tt"); // Format the printed date
                pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new FooterEventHandler(footerTexts, printedDate));



                document.Close();
                return ms.ToArray();
            }
        }

        #endregion
    }

    public class FooterEventHandler : IEventHandler
    {
        private readonly string[] footerTexts;
        private readonly string printedDate;
        private const float FOOTER_START = 55f;
        private const float SPACER_HEIGHT = 80f; // Height of invisible spacer box

        public FooterEventHandler(string[] footerTexts, string printedDate)
        {
            this.footerTexts = footerTexts;
            this.printedDate = printedDate;
        }

        public void HandleEvent(Event evt)
        {
            if (evt is PdfDocumentEvent docEvent)
            {
                PdfDocument pdfDoc = docEvent.GetDocument();
                PdfPage page = docEvent.GetPage();
                Rectangle pageSize = page.GetPageSize();
                Document document = new Document(pdfDoc);

                // Add invisible spacer box at the bottom
                Table spacer = new Table(1).UseAllAvailableWidth();
                spacer.AddCell(new Cell()
                    .SetHeight(SPACER_HEIGHT)
                    .SetBorder(Border.NO_BORDER)
                    .SetBackgroundColor(ColorConstants.WHITE));

                float yPosition = pageSize.GetBottom() + SPACER_HEIGHT;
                spacer.SetFixedPosition(pageSize.GetLeft(), yPosition, pageSize.GetWidth());

                // Create canvas for footer elements
                Canvas canvas = new Canvas(new PdfCanvas(page), pdfDoc, pageSize);
                canvas.SetFontColor(ColorConstants.GRAY);


                // Draw horizontal line
                float lineYPos = FOOTER_START;
                float marginSpace = 30;
                float lineWidth = pageSize.GetWidth() - (2 * marginSpace);

                canvas.ShowTextAligned(
                    new Paragraph().SetBorderTop(new SolidBorder(ColorConstants.GRAY, 1)).SetWidth(lineWidth).SetMarginTop(50),
                    pageSize.GetWidth() / 2,
                    lineYPos,
                    TextAlignment.CENTER
                );

                // Add footer texts
                float yPos = FOOTER_START - 25;
                foreach (var text in footerTexts)
                {
                    Paragraph footerParagraph = new Paragraph(text)
                        .SetFontSize(8)
                        .SetTextAlignment(TextAlignment.CENTER);

                    canvas.ShowTextAligned(
                        footerParagraph,
                        pageSize.GetWidth() / 2,
                        yPos,
                        TextAlignment.CENTER
                    );
                    yPos -= 12;
                }

                // Add page number and printed date
                float xPos = pageSize.GetWidth() - 50;

                // Add page number
                //Paragraph pageNumber = new Paragraph($"Page {pdfDoc.GetPageNumber(page)}")
                //    .SetFontSize(8)
                //    .SetFontColor(ColorConstants.GRAY)
                //    .SetTextAlignment(TextAlignment.RIGHT);

                //canvas.ShowTextAligned(
                //    pageNumber,
                //    xPos,
                //    24,
                //    TextAlignment.RIGHT
                //);

                // Add printed date
                Paragraph dateParagraph = new Paragraph($"Print Date: {printedDate}")
                    .SetFontSize(8)
                    .SetBold()
                    .SetFontColor(ColorConstants.GRAY)
                    .SetTextAlignment(TextAlignment.RIGHT);

                canvas.ShowTextAligned(
                    dateParagraph,
                    xPos,
                    12,
                    TextAlignment.RIGHT
                );

                canvas.Close();
            }
        }
    }

    //public class FooterEventHandler : IEventHandler
    //{
    //    private readonly string[] footerTexts; // Array to hold multiple lines of footer text
    //    private readonly string printedDate; // Variable to hold the printed date

    //    public FooterEventHandler(string[] footerTexts, string printedDate)
    //    {
    //        this.footerTexts = footerTexts;
    //        this.printedDate = printedDate;
    //    }

    //    public void HandleEvent(Event evt)
    //    {
    //        if (evt is PdfDocumentEvent docEvent)
    //        {
    //            PdfDocument pdfDoc = docEvent.GetDocument();
    //            Document document = new Document(pdfDoc);

    //            // Get the current page
    //            int pageNumber = 1;
    //            float yPos = 30; // Adjust Y position for the footer
    //            Rectangle pageSize = pdfDoc.GetDefaultPageSize();

    //            float lineYPos = 55; // Position for the horizontal line above footer
    //            float marginSpace = 30; // Space to leave on left and right
    //            float lineWidth = pageSize.GetWidth() - (2 * marginSpace); // Line width with left and right margins

    //            // Create the Canvas to draw the footer elements
    //            Canvas canvas = new Canvas(new PdfCanvas(docEvent.GetPage()), pdfDoc, pageSize);
    //            canvas.SetFontColor(ColorConstants.GRAY);

    //            // Draw the horizontal line centered with left and right margins
    //            canvas.ShowTextAligned(
    //                new Paragraph().SetBorderTop(new SolidBorder(ColorConstants.GRAY, 1)).SetWidth(lineWidth).SetPaddingTop(50),
    //                pageSize.GetWidth() / 2, // Center the line horizontally
    //                lineYPos, // Line Y position
    //                TextAlignment.CENTER
    //            );

    //            // Loop through the footer texts and add them to the document
    //            foreach (var text in footerTexts)
    //            {
    //                Paragraph footerParagraph = new Paragraph(text)
    //                    .SetFontSize(8)
    //                    .SetTextAlignment(TextAlignment.CENTER);

    //                document.ShowTextAligned(footerParagraph, pdfDoc.GetDefaultPageSize().GetWidth() / 2, yPos, pageNumber, TextAlignment.CENTER, VerticalAlignment.BOTTOM, 0);
    //                yPos -= 12; // Adjust Y position for the next line (12 points spacing)
    //            }

    //            // Add printed date at the bottom right corner
    //            float xPos = pdfDoc.GetDefaultPageSize().GetWidth() - 50; // Adjust X position for the printed date
    //            Paragraph dateParagraph = new Paragraph($"Print Date: {printedDate}")
    //                .SetFontSize(8).SetBold()
    //                    .SetFontColor(ColorConstants.GRAY)
    //                .SetTextAlignment(TextAlignment.RIGHT);
    //            document.ShowTextAligned(dateParagraph, xPos, 12, pageNumber, TextAlignment.RIGHT, VerticalAlignment.BOTTOM, 0);
    //        }
    //    }
    //}
}
