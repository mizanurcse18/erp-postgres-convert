using HRMS.DAL.Entities;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using Manager.Core.CommonDto;

namespace HRMS.Manager.Dto
{
    public class DocumentUploadDto
    {
        public int DUID { get; set; }
        public int EmployeeID { get; set; }
        public string TINNumber { get; set; }
        public int DocumentTypeID { get; set; }
        public int IncomeYear { get; set; }
        public int AssessmentYear { get; set; }
        public string RegSlNo { get; set; }
        public string TaxZone { get; set; }
        public string TaxCircle { get; set; }
        public string TaxUnit { get; set; }
        public decimal PayableAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime SubmissionDate { get; set; }
        public int ApprovalStatusID { get; set; }
        public bool IsDraft { get; set; }
        public bool IsUploaded { get; set; }
        public string ApiResponse { get; set; }

        public bool IsReassessment { get; set; }
        public bool IsEditable { get; set; }
        public List<Attachments> Attachments { get; set; }
        public int ApprovalProcessID { get; set; } = 0;
        public DateTime CreatedDate { get; set; }

        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string DivisionName { get; set; }
        public string DepartmentName { get; set; }
        public string WorkMobile { get; set; }
        public string ImagePath { get; set; }


        public bool IsCurrentAPEmployee { get; set; }
        public int APEmployeeFeedbackID { get; set; }
        public int APForwardInfoID { get; set; }
        public bool IsReturned { get; set; }
        public string ReferenceKeyword { get; set; }
        public string IncomeYearTitle { get; set; }
        public string AssessmentYearTitle { get; set; }
        public string BaseUrl { get; set; }
        
        public DocumentUploadDto() {
            FilePaths = new List<FilePath>();
        }
        public List<FilePath> FilePaths { get; set; }
    }

    public class FilePath
    {
        public string link { get; set; }
    }


}
