using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("Person")]
    public class Person : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PersonID { get; set; }
        [Required]
        [Loggable]
        public string FirstName { get; set; }
        [Loggable]
        public string LastName { get; set; }
        [Loggable]
        public string Mobile { get; set; }
        [Loggable]
        public string Mobile2 { get; set; }
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Please enter a valid email address.")]
        [Loggable]
        public string Email { get; set; }
        [Loggable]
        public int? GenderID { get; set; }
        [Loggable]
        public DateTime? DateOfBirth { get; set; }
        [Loggable]
        public string AlternateEmail { get; set; }
        [Loggable]
        public int? ReligionID { get; set; }
        [Loggable]
        public string Nationality { get; set; }
        [Loggable]
        public string IsBangladeshi { get; set; }
        [Loggable]
        public int? BloodGroupID { get; set; }
        [Loggable]
        public int? PersonTypeID { get; set; }
        [Loggable]
        public string FatherName { get; set; }
        [Loggable]
        public string MotherName { get; set; }
        [Loggable]
        public string NIDNumber { get; set; }
        [Loggable]
        public string PassportNumber { get; set; }
        [Loggable]
        public DateTime? PassportIssueDate { get; set; }
        [Loggable]
        public DateTime? PassportExpiryDate { get; set; }
        [Loggable]
        public string BirthCertificate { get; set; }
        [Loggable]
        public string DrivingLicense { get; set; }
        [Loggable]
        public string TINNumber { get; set; }
        [Loggable]
        public string TaxZone { get; set; }
        [Loggable]
        public int? MaritalStatusID { get; set; }
        [Loggable]
        public string BanglaName { get; set; }
        [Loggable]
        public string BanglaFullName { get; set; }
        [Loggable]
        public DateTime? MarriageDate { get; set; }
        [Loggable]
        public DateTime? FatherDOB { get; set; }
        [Loggable]
        public bool IsFatherAlive { get; set; }
        [Loggable]
        public DateTime? MotherDOB { get; set; }
        [Loggable]
        public bool IsMotherAlive { get; set; }
        [Loggable]
        public string SpouseName { get; set; }
        [Loggable]
        public DateTime? SpouseDOB { get; set; }
        [Loggable]
        public int? SpouseGenderID { get; set; }
        [Loggable]
        public string MaritalDetails { get; set; }
    }
}
