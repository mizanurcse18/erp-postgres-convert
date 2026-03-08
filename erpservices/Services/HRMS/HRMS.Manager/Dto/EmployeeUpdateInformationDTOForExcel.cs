using System;
using System.Collections.Generic;
using System.Text;
using DAL.Core.EntityBase;
using HRMS.DAL.Entities;

namespace HRMS.Manager.Dto
{
    public class EmployeeUpdateInformationDTOForExcel
    {
        public string Employee_Code { get; set; }
        public string Designation { get; set; }
        public string Internal_Designation { get; set; }
        public string Job_Grade_Name { get; set; }
        public string Supervisor_Employee_Code { get; set; }
        public string Department { get; set; }
        public string Division { get; set; }
        // Employee_Code, Designation, Internal_Designation, Job_Grade_Name, Supervisor_Employee_Code, Department, Division
    }
}