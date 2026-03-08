using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("Supplier"), Serializable]
    public class Supplier : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long SupplierID { get; set; }
        [Required]
        [Loggable]
        public string SupplierName { get; set; }
        [Loggable]
        public int SupplierTypeID { get; set; }
        [Loggable]
        public int SupplierCategoryID { get; set; }
        [Loggable]
        public string RegisteredAddress { get; set; }
        [Loggable]
        public string CorrespondingAddress { get; set; }
        [Loggable]
        public string PhoneNumber { set; get; }
        [Loggable]
        public string PostalCode { set; get; }
        [Loggable]
        public string EmailAddress { set; get; }
        [Loggable]
        public string TINNumber { set; get; }
        [Loggable]
        public string VATRegistrationNumber { set; get; }
        [Loggable]
        public string BankName { get; set; }        
        [Loggable]
        public string BankBranch { get; set; }
        [Loggable]
        public string BankAccountName { get; set; }
        [Loggable]
        public string BankAccountNumber { get; set; }
        [Loggable]
        public long? GLID { get; set; }
        [Loggable]
        public string RoutingNumber	{ get; set; }
        [Loggable]
        public string SwiftCode	{ get; set; }
        [Loggable]
        public string ContactName1 { get; set; }
        [Loggable]
        public string ContactEmail1 { get; set; }
        [Loggable]
        public string PhoneNumber1 { get; set; }
        [Loggable]
        public string ContactName2 { get; set; }
        [Loggable]
        public string ContactEmail2 { get; set; }
        [Loggable]
        public string PhoneNumber2 { get; set; }
        [Loggable]
        public string Remarks { get; set; }
        [Loggable]
        public string ExternalID { set; get; }
        [Loggable]
        public string MerchantWalletNo { set; get; }
        [Loggable]
        public string SupplierCode { set; get; }
        [Loggable]
        public string BINNumber { set; get; }
    }
}
