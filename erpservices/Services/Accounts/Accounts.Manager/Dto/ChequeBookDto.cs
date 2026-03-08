using DAL.Core.EntityBase;
using Accounts.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Accounts.Manager.Dto
{
    [AutoMap(typeof(ChequeBook)), Serializable]
    public class ChequeBookDto:Auditable
    {
        public int CBID { get; set; }
        public long GLID { get; set; }
        public string GLCode { get; set; }
        public string GLName { get; set; }
        public long BankID { get; set; }
        public string BankName { get; set; }
        public string ChequeBookNo { get; set; }
        public int NoOfPage { get; set; }
        public bool IsRemovable { get; set; }
        public long AccountNo { get; set; }
        public string AccountName { get; set; }
        public string BranchName { get; set; }
        public string RoutingName { get; set; }
        public string SwiftCode { get; set; }
        public bool IsActive { get; set; }
        public int StartLeaf { get; set; }
        public int EndLeaf { get; set; }
        public List<ChequeBookChildItemDetails> ChequeBookChildItemDetails { get; set; }

    }

    public class ChequeBookChildItemDetails
    {
        public int CBCID { get; set; }
        public int CBID { get; set; }
        public int LeafNo { get; set; }
        public bool IsActiveLeaf { get; set; }
        public bool IsUsed { get; set; }
    }
}
