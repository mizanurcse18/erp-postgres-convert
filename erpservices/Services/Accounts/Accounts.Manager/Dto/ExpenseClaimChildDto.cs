using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.Manager
{
    [AutoMap(typeof(ExpenseClaimChild)), Serializable]
    public class ExpenseClaimChildDto : Auditable
    {
        public long ECChildID { get; set; }
     
        public long ECMasterID { get; set; }
      
        public DateTime ExpenseClaimDate { get; set; }
     
        public int PurposeID { get; set; }
      
        public string Details { get; set; }
       
        public decimal ExpenseClaimAmount { get; set; }
        
        public string Remarks { get; set; }
        public string Purpose { get; set; }

        public List<Attachments> Attachments { get; set; }
    }
}
