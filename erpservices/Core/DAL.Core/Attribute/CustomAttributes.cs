using System;

namespace DAL.Core.Attribute
{
    /// <summary>
    /// Used to define auto-repository types for entities.
    /// This can be used for DbContext types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RepositoryTypesAttribute : System.Attribute
    {
        public Type RepositoryInterface { get; }

        public Type RepositoryImplementation { get; }

        public RepositoryTypesAttribute(Type repositoryInterface, Type repositoryImplementation)
        {
            RepositoryInterface = repositoryInterface;
            RepositoryImplementation = repositoryImplementation;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class LoggableAttribute : System.Attribute
    {
    }
}
