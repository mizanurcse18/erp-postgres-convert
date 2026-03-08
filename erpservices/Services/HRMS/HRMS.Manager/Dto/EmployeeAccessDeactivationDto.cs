using HRMS.DAL.Entities;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using Manager.Core.CommonDto;

namespace HRMS.Manager.Dto
{
    public class EmployeeAccessDeactivationDto 
    {
        public int EADID { get; set; }
        public int EEIID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime DateOfJoining { get; set; }
        public DateTime DateOfResignation { get; set; }
        public DateTime LastWorkingDay { get; set; }
        public bool? IsCoreFunctional { get; set; }
        public int ApprovalStatusID { get; set; }
        public bool IsSentForDivisionClearance { get; set; }
        public bool IsReassessment { get; set; }
        public int? DivisionClearanceApprovalStatusID { get; set; }
        public DateTime? SentForDivisionClearanceDate { get; set; }
        public string Description { get; set; }
        public bool IsDraft { get; set; }
        public bool IsEditable { get; set; }
        public bool IsDraftForDivClearence { get; set; }
        public List<Attachments> Attachments { get; set; }
        public string EmployeeNameError { get; set; }
        public int ApprovalProcessID { get; set; } = 0;
        public int ADApprovalProcessID { get; set; } = 0;
        public DateTime CreatedDate { get; set; }
        public string EEFullName { get; set; }
        public string Tenure { get; set; }
        public string EmployeeType { get; set; }
        public string EEEmployeeCode { get; set; }
        public string EEDesignationName { get; set; }
        public string EEDivisionName { get; set; }
        public string EEDepartmentName { get; set; }
        public string EEEmail { get; set; }
        public string EEMobile { get; set; }

        public List<ManualApprovalPanelEmployeeDto> DivisionClearenceApprovalPanelList { get; set; }

        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string DivisionName { get; set; }
        public string DepartmentName { get; set; }
        public string WorkMobile { get; set; }
        public string ImagePath { get; set; }
        public string EmpImagePath { get; set; }
        public string FunctionalOrNot { get; set; }
        public string SupervisorFullName { get; set; }
    }
}
