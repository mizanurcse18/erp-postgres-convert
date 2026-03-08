using Accounts.DAL.Entities;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Accounts.Manager.Dto
{
    public class PettyCashExpenseClaimDto
    {
        public long PCEMID { get; set; }
        public string ReferenceKeyword { get; set; }
        //public long IOUMasterID { get; set; }
        public long PCAMID { get; set; }
        public int ApprovalProcessID { get; set; }
        public bool IsDraft { get; set; }
        public List<PettyCashExpenseClaimDetails> Details { get; set; }

    }

    [AutoMap(typeof(PettyCashExpenseChild)), Serializable]
    public class PettyCashExpenseClaimDetails : Auditable
    {
        public long PCECID { get; set; }
        public long PCEMID { get; set; }
        public DateTime ExpenseDate { get; set; }
        public int PurposeID { get; set; }
        public string Details { get; set; }
        //public decimal ExpenseClaimAmount { get; set; }
        public decimal ExpenseAmount { get; set; }
        public string Remarks { get; set; }
        public List<Attachments> Attachments { get; set; }
    }

    //public class Attachments
    //{
    //    public string AID { get; set; }
    //    public int FUID { get; set; }
    //    public int ID
    //    {
    //        get
    //        {
    //            int fuid;
    //            if (int.TryParse(AID, out fuid))
    //            {
    //                return fuid;
    //            }
    //            else
    //            {
    //                return 0;
    //            }
    //        }
    //    }
    //    public string AttachedFile { get; set; }
    //    public string Type { get; set; }
    //    public string OriginalName { get; set; }
    //    public string FilePath { get; set; }
    //    public string FileName { get; set; }
    //    public int ReferenceId { get; set; }
    //    public int ParentFUID { get; set; }
    //    public decimal Size { get; set; }
    //    public string Description { get; set; }
    //    public string DocumentType { get; set; }
    //}
}
