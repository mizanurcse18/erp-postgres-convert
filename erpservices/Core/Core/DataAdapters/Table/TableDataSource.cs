using System.Collections.Generic;

namespace Core.DataAdapters.Table
{
    public static class TableDataSource<TP, TC>
        where TP : class, new()
        where TC : class, new()
    {
        public static object GetDataSource(IEnumerable<TP> parentDataSource, IEnumerable<TC> childrenDataSource,
            string parentKey = "parentKey", string childKey = "childKey", string linker = "key")
        {
            return new { parentDataSource, childrenDataSource, parentKey, childKey, linker };
        }

        public static object GetDataSource(GridModel parentDataSource, IEnumerable<TC> childrenDataSource,
            string parentKey = "parentKey", string childKey = "childKey", string linker = "key")
        {
            return new { parentDataSource, childrenDataSource, parentKey, childKey, linker };
        }
    }
}
