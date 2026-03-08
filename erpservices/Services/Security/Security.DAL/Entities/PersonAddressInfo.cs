using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
namespace Security.DAL.Entities
{
    [Table("PersonAddressInfo")]
    public class PersonAddressInfo : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PAIID { get; set; }
        [Loggable]
        public int PersonID { get; set; }
        [Loggable]
        public int AddressTypeID { get; set; }
        [Loggable]
        public int? DistrictID { get; set; }
        [Loggable]
        public int? ThanaID { get; set; }
        [Loggable]
        public int? PostCode { get; set; }
        [Loggable]
        public String Address { get; set; }
        public bool IsSameAsPresentAddress { get; set; }
    }
}
