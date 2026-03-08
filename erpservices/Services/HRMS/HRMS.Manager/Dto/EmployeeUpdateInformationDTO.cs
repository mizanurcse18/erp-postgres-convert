using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class EmployeeUpdateInformationDTO
    {
        public string Employee_Code { get; set; }
        public string Designation { get; set; }
        public string Internal_Designation { get; set; }
        public string Job_Grade_Name { get; set; }
        public string Supervisor_Employee_Code { get; set; }
        public string Department { get; set; }
        public string Division { get; set; }
        public string? Validation_Result { get; set; } = null;
    }
}

