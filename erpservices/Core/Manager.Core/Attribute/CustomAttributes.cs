using System;

namespace Manager.Core.Attribute
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreMapAttribute : System.Attribute
    {
        public IgnoreMapAttribute(bool isIgnore)
        {
            IsIgnore = isIgnore;
        }

        public bool IsIgnore { get; }
    }
}
