using System;

using AutoMapper;
using Core.Extensions;

namespace Manager.Core.Mapper
{
    public class AutoMapFromAttribute : AutoMapAttributeBase
    {
        public MemberList MemberList { get; set; }

        public AutoMapFromAttribute(params Type[] targetTypes)
          : base(targetTypes)
        {
        }

        public AutoMapFromAttribute(MemberList memberList, params Type[] targetTypes)
          : this(targetTypes)
        {
            MemberList = memberList;
        }

        public override void CreateMap(IProfileExpression profile, Type type)
        {
            if (TargetTypes.IsNullOrEmpty()) return;

            foreach (Type targetType in TargetTypes)
            {
                profile.CreateMap(targetType, type, 0).IgnoreReadOnly(targetType);
            }
        }
    }
}
