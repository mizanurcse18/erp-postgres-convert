using Core;
using DAL.Core.EntityBase;
using HRMS.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class AuditMasterDto
    {
        public int EAMID { get; set; }
        public string ReferenceNo { get; set; }
        public string ReferenceKeyword { get; set; }
        public DateTime AuditDate { get; set; } = DateTime.Now;
        public MercentOrUddokta MercentOrUddokta { get; set; }
        public bool IsDraft { get; set; }
        public List<AuditChildDto> AuditDetails { get; set; }  
        public int ApprovalStatusID { get; set; }
        public string ApprovalStatus {  get; set; }
        public int ApprovalProcessID { get; set; }
        public bool CanEdit { get; set; }
        public string Remarks { get; set; }
        public string Requirements {  get; set; }
        public string Longtitude { get; set; }
        public string Latitude { get; set; }
        public List<Attachments> Attachments { get; set; }
        public string CapturedImage { get; set; }

    }

    [AutoMap(typeof(ExternalAuditChild)), Serializable]
    public class AuditChildDto : Auditable
    {
        public int EAMID { get; set; }
        public int EACID { get; set; }
        public List<Question> Question { get; set; }
        public string Feedback { get; set; }
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public List<Attachments> Attachments { get; set; }
        public bool IsRequried { get; set; }
        public bool IsPOSMRequired { get; set; }
        public List<ComboModel> POSM { get; set; } = new List<ComboModel>();
        public List<ComboModel> MultiPOSMList { get; set; } = new List<ComboModel>();
        public string Requirements { get; set; }
        public string POSMString { get; set; } = string.Empty;
    }

    public class Question
    {
        public int value { get; set; }
        public string label { get; set; }
    }

    public class MercentOrUddokta
    {
        public int value { get; set; }
        public string label { get; set; }
    }
}
