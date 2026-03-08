using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class GenericResponse <T> where T : class
    {
        public bool status { get; set; }
        public string message { get; set; }
        public List<T> data { get; set; } = new List<T>();
    }
}
