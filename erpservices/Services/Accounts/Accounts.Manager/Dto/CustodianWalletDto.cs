using DAL.Core.EntityBase;
using Accounts.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Accounts.Manager.Dto
{
    [AutoMap(typeof(CustodianWallet)), Serializable]
    public class CustodianWalletDto : Auditable
    {
        public long CWID { get; set; }
        public string WalletName { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public long EmployeeID { get; set; }
        public decimal ReimbursementThreshold { get; set; }
        public Decimal OpeningBalance { get; set; }
        public Decimal CurrentBalance { get; set; }
        public Decimal Limit { get; set; }
        public List<int> DivisionIDList { get; set; } = new List<int>();
        public List<int> DepartmentIDList { get; set; } = new List<int>();
        public string DivisionIDsStr { get; set; }
        public string DepartmentIDsStr { get; set; }
        public bool IsActive { get; set; }
        public List<Attachments> Attachments { get; set; }

        public string DivisionIDs
        {
            get
            {

                return DivisionIDList != null && DivisionIDList.Count > 0 ? String.Join(",", DivisionIDList) : null;

            }
        }
        public string DepartmentIDs
        {
            get
            {

                return DepartmentIDList != null && DepartmentIDList.Count > 0 ? String.Join(",", DepartmentIDList) : null;

            }
        }
    }
}
