using System;
using System.ComponentModel;

using AutoMapper;
using Core.Extensions;
using IgnoreMapAttribute = Manager.Core.Attribute.IgnoreMapAttribute;

namespace Manager.Core.Mapper
{
    public static class AutoMapExtensions
    {
        // Static mapper instance for backward compatibility
        private static IMapper _mapper;
        
        /// <summary>
        /// Initialize the static mapper instance (call this during application startup)
        /// </summary>
        public static void Initialize(IMapper mapper)
        {
            _mapper = mapper;
        }
        
        /// <summary>
        /// Converts an object to another using AutoMapper library. Creates a new object of <typeparamref name="TDestination" />.
        /// There must be a mapping between objects before calling this method.
        /// </summary>
        /// <typeparam name="TDestination">Type of the destination object</typeparam>
        /// <param name="source">Source object</param>
        public static TDestination MapTo<TDestination>(this object source)
        {
            if (_mapper == null)
                throw new InvalidOperationException("AutoMapper has not been initialized. Call Initialize() first.");
            return _mapper.Map<TDestination>(source);
        }

        /// <summary>
        /// Execute a mapping from the source object to the existing destination object
        /// There must be a mapping between objects before calling this method.
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="source">Source object</param>
        /// <param name="destination">Destination object</param>
        /// <returns></returns>
        public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination)
        {
            if (_mapper == null)
                throw new InvalidOperationException("AutoMapper has not been initialized. Call Initialize() first.");
            return _mapper.Map(source, destination);
        }

        public static IMappingExpression IgnoreReadOnly(this IMappingExpression expression, Type sourceType)
        {
            foreach (var property in sourceType.GetProperties())
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(sourceType)[property.Name];
                IgnoreMapAttribute attribute = (IgnoreMapAttribute)descriptor.Attributes[typeof(IgnoreMapAttribute)];

                if (attribute.IsNotNull() && attribute.IsIgnore)
                {
                    expression.ForMember(property.Name, opt => opt.Ignore());
                }
            }

            return expression;
        }
    }
}
