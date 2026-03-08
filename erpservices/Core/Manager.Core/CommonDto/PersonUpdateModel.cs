
using Security.Manager;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Manager.Core.CommonDto
{
    public class PersonUpdateModel
    {
        //public PersonCoreDto MasterModel { get; set; }
        public int PersonID { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Mobile { get; set; }

        public string Mobile2 { get; set; }
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Please enter a valid email address.")]

        public string Email { get; set; }

        public int? GenderID { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string AlternateEmail { get; set; }

        public int? ReligionID { get; set; }

        public string Nationality { get; set; }

        public string IsBangladeshi { get; set; }

        public int? BloodGroupID { get; set; }

        public int? PersonTypeID { get; set; }
        public string FatherName { get; set; }

        public string MotherName { get; set; }

        public string NIDNumber { get; set; }

        public string PassportNumber { get; set; }

        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{yyyy-MM-dd}")]
        public string PassportIssueDate { get; set; }
        public DateTime? PassportIssueDateFormatted
        {
            get
            {
                string sysFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                

                string[] formats = {"M/d/yyyy h:mm:ss tt", "M/d/yyyy h:mm tt",
                     "MM/dd/yyyy hh:mm:ss tt", "M/d/yyyy h:mm:ss",
                     "M/d/yyyy hh:mm tt", "M/d/yyyy hh tt",
                     "M/d/yyyy h:mm", "M/d/yyyy h:mm","dd/MM/yyyy hh:mm:ss tt",
                     "MM/dd/yyyy hh:mm", "M/dd/yyyy hh:mm","yyyy-MM-dd"};


                string[] dateStrings = { PassportIssueDate };
                DateTime formattedDate;
                DateTime? md = null;

                
                    foreach (string dateString in dateStrings)
                    {
                        if (DateTime.TryParseExact(dateString, formats,
                                                   new CultureInfo("en-US"),
                                                   DateTimeStyles.None,
                                                   out formattedDate))
                        {
                            md = formattedDate;
                        }

                    }
                
                //DateTime d = DateTime.ParseExact(PassportIssueDate.ToString().Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                return md;
            } 
        }

        public DateTime? PassportExpiryDate { get; set; }

        public string BirthCertificate { get; set; }

        public string DrivingLicense { get; set; }

        public string TINNumber { get; set; }

        public string TaxZone { get; set; }

        public int? MaritalStatusID { get; set; }

        public string BanglaName { get; set; }

        public string BanglaFullName { get; set; }

        public DateTime? MarriageDate { get; set; }
        public DateTime? FatherDOB { get; set; }
        public string IsFatherAlive { get; set; }
        public DateTime? MotherDOB { get; set; }
        public string IsMotherAlive { get; set; }

        public int? PresentDistrictID { get; set; }
        public int? PresentThanaID { get; set; }
        public int? PresentPostCode { get; set; }
        public string PresentAddress { get; set; }

        public int? PermanentDistrictID { get; set; }
        public int? PermanentThanaID { get; set; }
        public int? PermanentPostCode { get; set; }
        public string PermanentAddress { get; set; }
        public bool IsSameAsPresentAddress { get; set; }

        public string SpouseName { get; set; }
        public DateTime? SpouseDOB { get; set; }
        public int? SpouseGenderID { get; set; }
        public string SpouseGenderName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int updatedBy { get; set; }
        public List<PersonFamilyInfoCoreDto> ChildrenInfo { get; set; }
        public List<NomineeCoreDto> NomineeInfo { get; set; }
    }
}
