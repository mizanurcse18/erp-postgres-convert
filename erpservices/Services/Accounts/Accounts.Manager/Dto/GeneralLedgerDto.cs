using System;
using System.Collections.Generic;
using System.Text;

namespace Accounts.Manager.Dto
{
    public class GeneralLedgerDto
    {
        public long GLID { get; set; }
        public String GLName { get; set; }       
        public String GLCode { get; set; }      
        public long? GLGroupID { get; set; }      
        public decimal? GLTotalAmount { get; set; }       
        public decimal? OpeningBalance { get; set; }     
        public DateTime? OpeningBalanceOn { get; set; }        
        public string ExternalID { get; set; }        
        public string BalanceType { get; set; }        
        public bool IsBudgetEnable { get; set; }        
        public bool IsActive { get; set; }        
        public bool IsAllowManualPosting { get; set; }       
        public DateTime? StartDate { get; set; }        
        public int GLTypeID { get; set; }        
        public int GLLayerID { get; set; }
    }
}
