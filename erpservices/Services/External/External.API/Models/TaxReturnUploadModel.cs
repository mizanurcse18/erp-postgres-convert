using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace External.API.Models
{

    public class EncryptedTaxReturnUploadModel
    {
        public string EncryptedData { get; set; }
    }
    public class TaxReturnUploadModel
    {
        public string EmployeeCode { get; set; }
        public string TINNumber { get; set; }
        public int DocumentTypeID { get; set; }
        public string IncomeYearTitle { get; set; }
        public string AssessmentYearTitle { get; set; }
        public string RegSlNo { get; set; }
        public string TaxZone { get; set; }
        public string TaxCircle { get; set; }
        public string TaxUnit { get; set; }
        public decimal PayableAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime SubmissionDate { get; set; }
        public List<FilePath> filePaths { get; set; }

    }
    public class FilePath
    {
        public string link { get; set; }
    }
}
