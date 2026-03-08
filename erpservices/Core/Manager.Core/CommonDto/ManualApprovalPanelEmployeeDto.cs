using Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Manager.Core.CommonDto
{
    public class ManualApprovalPanelEmployeeDto
    {
        public int MAPPanelEmployeeID { get; set; }
        public int DivisionID { get; set; }
        public string DivisionName { get; set; }
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public int APPanelID { get; set; }
        public string PanelName { get; set; }
        public decimal SequenceNo { get; set; }
        public int? ProxyEmployeeID { get; set; }
        public string ProxyEmployeeName { get; set; }
        public bool IsProxyEmployeeEnabled { get; set; }
        public string ImagePath { get; set; }
        public string NFAApprovalSequenceTypeName { get; set; }
        public int? NFAApprovalSequenceType { get; set; }
        public bool IsEditable { get; set; }
        public bool IsSCM { get; set; }
        public string ApproverPhotoUrl { get; set; }
        public bool IsMultiProxy { get; set; }
        public int APTypeID { get; set; }
        public string EmployeeCode { get; set; }
        public long ReferenceID { get; set; }
        public bool IsSystemGenerated { get; set; }
        public List<ManualMultipleProxyDetailsDto> ManualMultipleProxyDetails { get; set; }
        public int? ProxyEmployeeDivisionID { get; set; }
        public int? ProxyEmployeeDepartmentID { get; set; }
        public string ProxyEmployeeImagePath { get; set; }
        public string ProxyApprovalPanelEmployeeName { get; set; }
    }
    public class ManualMultipleProxyDetailsDto
    {
        public int MAPPanelProxyEmployeeID { get; set; }
        public int MAPPanelEmployeeID { get; set; }
        public int DivisionID { get; set; }
        public int DepartmentID { get; set; }
        public int EmployeeID { get; set; }
        public int APPanelID { get; set; }
        public string ProxyEmployeeName { get; set; }
        public string EmployeeName { get; set; }
        public string DivisionName { get; set; }
        public string DepartmentName { get; set; }
        public string ProxyEmployeeImagePath { get; set; }
    }
}
