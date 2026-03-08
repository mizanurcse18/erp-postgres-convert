using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Accounts.Manager
{
    [AutoMap(typeof(IOUChild)), Serializable]
    public class IOUChildDto:Auditable
    {
        public long IOUChildID { get; set; }
        public long IOUMasterID { get; set; }
        public string Description { get; set; }
        public decimal IOUAmount { get; set; }
        public string Remarks { get; set; }
        public List<Attachments> Attachments { get; set; }
    }
}
