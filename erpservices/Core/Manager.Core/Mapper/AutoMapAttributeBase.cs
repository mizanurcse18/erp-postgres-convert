using System;

using AutoMapper;

namespace Manager.Core.Mapper
{
    public abstract class AutoMapAttributeBase : System.Attribute
    {
        public Type[] TargetTypes { get; }

        protected AutoMapAttributeBase(params Type[] targetTypes)
        {
            TargetTypes = targetTypes;
        }

        public abstract void CreateMap(IProfileExpression profile, Type type);
    }
}
