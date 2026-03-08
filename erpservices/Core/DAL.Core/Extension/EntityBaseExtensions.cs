using System;
using System.Linq;
using System.Collections.Generic;

using DAL.Core.EntityBase;
using DAL.Core.Extension;

namespace DAL.Core.Extension
{
    public static class EntityBaseExtensions
    {
        public static void MapToAuditFields(this Auditable source, Auditable destination)
        {
            if (source.IsNull() || destination.IsNull()) return;

            destination.CompanyID = source.CompanyID;
            destination.CreatedBy = source.CreatedBy;
            destination.CreatedDate = source.CreatedDate;
            destination.CreatedIP = source.CreatedIP;
            destination.RowVersion = source.RowVersion;
            destination.SetUnchanged();
        }

        public static void MapToAuditFields(this IEnumerable<Auditable> source, IEnumerable<Auditable> destination)
        {
            var aSource = source.ToArray();
            var aDestination = destination.ToArray();

            for (var i = 0; i < aSource.Length; i++)
            {
                if (i > aDestination.Length) break;
                aSource[i].MapToAuditFields(aDestination[i]);
            }
        }

        public static IEnumerable<EntityBase.EntityBase> GetChanges(this IEnumerable<EntityBase.EntityBase> list)
        {
            return list.Where(item => item.IsAdded || item.IsModified || item.IsDeleted);
        }

        public static IEnumerable<EntityBase.EntityBase> GetChanges(this IEnumerable<EntityBase.EntityBase> list, ModelState itemState)
        {
            return list.Where(item => item.ObjectState.Equals(itemState));
        }

        public static void ChangeState<T>(this List<T> list, ModelState itemState) where T : EntityBase.EntityBase
        {
            foreach (var item in list)
            {
                item.ObjectState = itemState;
            }
        }

        public static IEnumerable<EntityBase.EntityBase>[] Reverse(this IEnumerable<EntityBase.EntityBase>[] list)
        {
            var listTemp = new IEnumerable<EntityBase.EntityBase>[list.Length];

            var i = 0;
            for (var index = list.Length; index >= 1; --index)
            {
                listTemp[i] = list[index - 1];
                i++;
            }

            return listTemp;
        }

        public static void AcceptChanges<T>(this List<T> list) where T : EntityBase.EntityBase
        {
            var items = list.Cast<EntityBase.EntityBase>().ToArray();

            foreach (var item in items)
            {
                switch (item.ObjectState)
                {
                    case ModelState.Deleted:
                    case ModelState.Detached:
                        list.Remove((T)Convert.ChangeType(item, typeof(T)));
                        break;
                    case ModelState.Modified:
                    case ModelState.Added:
                        item.SetUnchanged();
                        break;
                }
            }
        }

        public static bool IsNull(this EntityBase.EntityBase obj)
        {
            return obj == null;
        }

        public static bool IsNotNull(this EntityBase.EntityBase obj)
        {
            return obj != null;
        }
    }
}
