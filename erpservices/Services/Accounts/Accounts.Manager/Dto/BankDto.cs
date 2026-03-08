using DAL.Core.EntityBase;
using Accounts.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Accounts.Manager.Dto
{
    [AutoMap(typeof(Bank)), Serializable]
    public class BankDto:Auditable
    {
        public long BankID { get; set; }
        public string BankName { get; set; }
        public string BankAddress { get; set; }
        public string ConcernPersonName { get; set; }
        public string ConcernPersonPhoneNumber { get; set; }
        public bool IsRemovable { get; set; }
        public bool IsActive { get; set; }

    }
}
