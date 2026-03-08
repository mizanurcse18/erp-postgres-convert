using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.Text;
using Manager.Core.Mapper;
using Accounts.DAL.Entities;
using DAL.Core.Attribute;
using System.ComponentModel.DataAnnotations;

namespace Accounts.Manager.Dto
{
    public class VoucherDto
    {
        public long VoucherMasterId { get; set; }
        public string ReferenceNo { get; set; }
        public bool IsExcelUpload { get; set; }
        public int VoucherTypeId { get; set; }
        public string VoucherTypeName { get; set;}
        public int EmployeeID { get; set;}
        public DateTime VoucherDate { get; set; }
        public string Remarks { get; set; }
        public long TotalDebit { get; set; }
        public long TotalCredit { get; set; }
        public List<VoucherChildDto> VoucherDetails { get; set; }
        public List<Attachments> Attachments { get; set; }

    }

    [AutoMap(typeof(VoucherChild)), Serializable]
    public class VoucherChildDto : Auditable
    {
        public long VoucherChildID { get; set; }
        public long VoucherMasterID { get; set; }
        public int TxnTypeID { get; set; }
        public string TxnType { get; set; }
        public long COAID { get; set; }
        public string COAGLCode { get; set; }
        public int CostCenterID { get; set; }
        public string CostCenterLabel { get; set; }
        public int BudgetHeadID { get; set; }
        public string BudgetHeadLabel { get; set; }
        public string Narration { get; set; }
        public int ModeOfPaymentID { get; set; }
        public string ModeOfPaymentLabel { get; set; }
        public long CBID { get; set; }
        public long CBCID { get; set; }
        public long LeafNo { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public bool IsActive { get; set; }

    }
}
