using DAL.Core.Attribute;
using DAL.Core.Repository;

namespace DAL.Core.Helper
{
    internal static class RepositoryTypes
    {
        public static RepositoryTypesAttribute Default { get; private set; }

        static RepositoryTypes()
        {
            Default = new RepositoryTypesAttribute(typeof(IRepository<>), typeof(Repository<,>));
        }
    }
}
