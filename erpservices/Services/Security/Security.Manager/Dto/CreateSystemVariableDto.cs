using System;

namespace Security.Manager.Dto
{
    public class CreateSystemVariableDto
    {
        public string EntityTypeName { get; set; }
        public string SystemVariableCode { get; set; }
        public string SystemVariableDescription { get; set; }
        public int? NumericValue { get; set; } = 1;
        public int Sequence { get; set; }
        public bool IsSystemGenerated { get; set; }
        public bool IsInactive { get; set; }
    }
} 