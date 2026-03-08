using Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class JobGradeDecryptDto
    {
        public int EmploymentID { get; set; }
        public int EmployeeID { get; set; }
        public string JobGradeID { get; set; }
        public int JobGrade
        {
            get
            {

                if (!string.IsNullOrWhiteSpace(JobGradeID))
                {
                    string decryptedVal = Util.Decrypt(JobGradeID);
                    if (!string.IsNullOrWhiteSpace(decryptedVal))
                    {
                        return Convert.ToInt32(decryptedVal);
                    }
                    return 0;
                }
                return 0;
            }
        }
    }
}

