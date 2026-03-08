
using DAL.Core.EntityBase;
using SCM.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCM.Manager.Dto
{
    [AutoMap(typeof(Supplier)), Serializable]
    public class SupplierDto : Auditable
    {
        public long SupplierID { get; set; }

        public string SupplierNameError { get; set; }
        public string SupplierName { get; set; }
        
        public int SupplierTypeID { get; set; }
        public string SupplierTypeName { get; set; }

        public int SupplierCategoryID { get; set; }
        public string SupplierCategoryName { get; set; }

        public string RegisteredAddress { get; set; }
        
        public string CorrespondingAddress { get; set; }
        
        public string PhoneNumber { set; get; }
        
        public string PostalCode { set; get; }
        
        public string EmailAddress { set; get; }
        
        public string TINNumber { set; get; }
        
        public string VATRegistrationNumber { set; get; }
        
        public string BankName { get; set; }
        
        public string BankBranch { get; set; }
        
        public string BankAccountName { get; set; }
        
        public string BankAccountNumber { get; set; }
        
        public string MyProperty { get; set; }
        
        public string RoutingNumber { get; set; }
        
        public string SwiftCode { get; set; }
        
        public string ContactName1 { get; set; }
        
        public string ContactEmail1 { get; set; }
        
        public string PhoneNumber1 { get; set; }
        
        public string ContactName2 { get; set; }
        
        public string ContactEmail2 { get; set; }
        
        public string PhoneNumber2 { get; set; }
        
        public string Remarks { get; set; }
        public List<Attachments> Attachments { get; set; }
        public long? GLID { get; set; }
        public string ExternalID { set; get; }
        public string MerchantWalletNo { set; get; }
        public string SupplierCode { set; get; }
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
        public string DocumentType { get; set; }
        public string Description { get; set; }
        public int ParentFUID { get; set; }
    }
}
