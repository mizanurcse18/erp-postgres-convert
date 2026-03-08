using System;
using System.Reflection;

using AutoMapper;

namespace Manager.Core.Mapper
{
    public static class AutoMapperConfigurationExtensions
    {
        public static void CreateAutoAttributeMaps(this IProfileExpression profile, Type type)
        {
            foreach (var customAttribute in type.GetCustomAttributes<AutoMapAttributeBase>())
            {
                customAttribute.CreateMap(profile, type);
            }
        }
    }
}
