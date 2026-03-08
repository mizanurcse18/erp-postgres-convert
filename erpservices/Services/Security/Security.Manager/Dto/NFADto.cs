using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    public class NFADto
    {
        public int NFAID { get; set; }
        public DateTime? NFADate { get; set; }
        public string ReferenceNo { get; set; }
        public string Subject { get; set; }
        public string Preamble { get; set; }
        public List<ItemDetails> ItemDetails { get; set; }
        public string PriceAndCommercial { get; set; }
        public string Solicitation { get; set; }
        public string BudgetPlanRemarks { get; set; }
        public int TemplateID { get; set; }
        public int ApprovalProcessID { get; set; }=0;
        public List<Attachments> Attachments { get; set; }
        public string Description { get;set; }
        public string DescriptionImageURL { get;set; }
        public string ReferenceKeyword { get;set; }
        public bool IsDraft { get; set; }
        public int BudgetPlanCategoryID { get; set; }
    }

    public class ItemDetails
    {
        public int NFACID { get; set; }
        public int NFACSID { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public decimal? Unit { get; set; }
        public string UnitType { get; set; }
        public decimal? UnitPrice { get; set; }
        //public decimal UnitPrice { get; set; }
        public string VAT { get; set; }
        public string Vendor { get; set; }
        public decimal? Total { get; set; }
        public string Type { get; set; }
        public string Duration { get; set; }
        public string CostType { get; set; }
        public decimal? EstimatedBudgetAmount { get; set; }
        public decimal? AITPercent { get; set; }
        public long ItemID { get; set; }
        public decimal Qty { get; set; }
        public int UOM { get; set; }
        public string UnitCode { get; set; }
    }
    public class Attachments
    {
        public string AID { get; set; }
        public int FUID { get; set; }
        public int ID
        {
            get
            {
                int fuid;
                if (int.TryParse(AID, out fuid))
                {
                    return fuid;
                }
                else
                {
                    return 0;
                }
            }
        }
        public string AttachedFile { get; set; }
        public string Type { get; set; }
        public string OriginalName { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int ReferenceId { get; set; }
        public decimal Size { get; set; }
        public string Description { get; set; }
    }
}
