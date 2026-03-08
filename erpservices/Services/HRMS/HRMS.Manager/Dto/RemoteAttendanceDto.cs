using Core;
using DAL.Core.EntityBase;
using HRMS.DAL.Entities;
using Manager.Core.Mapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Security.Manager.Dto
{
    [AutoMap(typeof(RemoteAttendance)), Serializable]
    public class RemoteAttendanceDto : Auditable
    {
        public int RAID { get; set; }
        public DateTime AttendanceDate { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeNote { get; set; }
        public string EntryType { get; set; }
        public int StatusID { get; set; }
        public int ApproverID { get; set; }
        public string ApprovarNote { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string StatusName { get; set; }
        public string ApproverName { get; set; }
        public string ApproverNote { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeImagePath { get; set; }
        public string ApproverImagePath { get; set; }
        public string Channel { get; set; }
        public int DistrictID { get; set; }
        public int DivisionID { get; set; }
        public int ThanaID { get; set; }
        public string Area { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string DivisionName { get; set; }
        public string DistrictName { get; set; }
        public string ThanaName { get; set; }
        public string InTimeError { get; set; }
        public List<int> SelectedIds { get; set; }
    }
}
