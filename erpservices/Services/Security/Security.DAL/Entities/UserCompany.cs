using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("UserCompany")]
    public class UserCompany : Auditable
    {
        public int UserID { set; get; }
        public bool IsDefault { set; get; }
    }
}
