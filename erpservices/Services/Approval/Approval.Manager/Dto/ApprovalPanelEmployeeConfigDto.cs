
using Approval.DAL.Entities;
using Core;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Manager.Dto
{
    [AutoMap(typeof(ApprovalPanelEmployeeConfig)), Serializable]
    public class ApprovalPanelEmployeeConfigDto : Auditable
    {
        public int APPEConfigID { get; set; }
        public int DivisionID { get; set; }
        public string DivisionName { get; set; }
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public int APPanelID { get; set; }
        public string PanelName { get; set; }
        public decimal SequenceNo { get; set; }
        //public string Particulars { get; set; }
        public int? ProxyEmployeeID { get; set; }
        public string ProxyEmployeeName { get; set; }
        public bool? IsProxyEmployeeEnabled { get; set; }
        public string ImagePath { get; set; }
        public string NFAApprovalSequenceTypeName { get; set; }
        public int NFAApprovalSequenceType { get; set; }
        public bool IsEditable { get; set; }
        //public bool IsSCM { get; set; }
        public string ApproverPhotoUrl { get; set; }
        public bool IsMultiProxy { get; set; }
        public List<MultipleProxyDetailsConfigDto> MultipleProxyDetails { get; set; }
        public int? ProxyEmployeeDivisionID { get; set; }
        public int? ProxyEmployeeDepartmentID { get; set; }
        public string ProxyEmployeeImagePath { get; set; }
        public string ApprovalPanelCategoryName { get; set; }
        public int? TemplateID { get; set; }
    }

    [AutoMap(typeof(ApprovalPanelProxyEmployeeConfig)), Serializable]
    public class MultipleProxyDetailsConfigDto : Auditable
    {
        public int APPanelProxyEmployeeID { get; set; }
        public int APPanelEmployeeID { get; set; }
        public int DivisionID { get; set; }
        public int DepartmentID { get; set; }
        public int EmployeeID { get; set; }
        public int APPanelID { get; set; }
        public string ProxyEmployeeName { get; set; }
        public string DivisionName { get; set; }
        public string DepartmentName { get; set; }
        public string ProxyEmployeeImagePath { get; set; }
    }
}
