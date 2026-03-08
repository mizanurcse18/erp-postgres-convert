using Accounts.DAL.Entities;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Accounts.Manager.Dto
{
    public class IOUDto
    {
        public IOUMasterDto master { get; set; }
        public List<ChildItemDetails> childs { get; set; }
        public int ApprovalProcessID { get; set; }
        public List<ChildItemDetails> ItemDetails { get; set; }
    }

    [AutoMap(typeof(IOUChild)), Serializable]
    public class ChildItemDetails : Auditable
    {
        public long IOUChildID { get; set; }
        public long IOUMasterID { get; set; }
        public string Description { get; set; }
        public decimal IOUAmount { get; set; }
        public string Remarks { get; set; }
        public List<Attachments> Attachments { get; set; }
    }
}
