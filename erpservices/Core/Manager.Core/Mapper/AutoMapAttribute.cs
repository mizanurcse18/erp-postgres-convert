using System;
using AutoMapper;
using Core.Extensions;

namespace Manager.Core.Mapper
{
    public class AutoMapAttribute : AutoMapAttributeBase
    {
        public AutoMapAttribute(params Type[] targetTypes)
            : base(targetTypes)
        {
        }

        public override void CreateMap(IProfileExpression profile, Type type)
        {
            if (TargetTypes.IsNullOrEmpty()) return;

            foreach (var targetType in TargetTypes)
            {
                profile.CreateMap(type, targetType).IgnoreReadOnly(type);
                profile.CreateMap(targetType, type).IgnoreReadOnly(targetType);
            }
        }
    }
}
