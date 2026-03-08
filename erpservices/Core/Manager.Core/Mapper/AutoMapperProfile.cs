using System.Linq;
using System.Reflection;

using AutoMapper;
using Manager.Core.Mapper;

namespace Manager.Core.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile(Assembly assembly)
        {
            FindAndAutoMapTypes(this, assembly);
        }

        private void FindAndAutoMapTypes(IProfileExpression profile, Assembly assembly)
        {
            var typeArray = assembly.GetTypes().Where(type => type.IsDefined(typeof(AutoMapAttribute)) ||
                                                              type.IsDefined(typeof(AutoMapFromAttribute)) ||
                                                              type.IsDefined(typeof(AutoMapToAttribute)));

            foreach (var type in typeArray)
            {
                profile.CreateAutoAttributeMaps(type);
            }
        }
    }
}
