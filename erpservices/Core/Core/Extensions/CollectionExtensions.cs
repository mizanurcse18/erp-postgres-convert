using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Core.Extensions
{
    /// <summary>Extension methods for Collections.</summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Checks whatever given collection object is null or has no item.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this ICollection<T> source)
        {
            return !source.IsNotNull() || source.Count <= 0;
        }

        /// <summary>
        /// Adds an item to the collection if it's not already in the collection.
        /// </summary>
        /// <param name="source">Collection</param>
        /// <param name="item">Item to check and add</param>
        /// <typeparam name="T">Type of the items in the collection</typeparam>
        /// <returns>Returns True if added, returns False if not.</returns>
        public static bool AddIfNotContains<T>(this ICollection<T> source, T item)
        {
            if (source.IsNull()) throw new ArgumentNullException(nameof(source));
            if (source.Contains(item)) return false;

            source.Add(item);
            return true;
        }

        public static MemberExpression GetMemberExpression(this Expression expression)
        {
            if (expression is MemberExpression memberExpression)
            {
                return memberExpression;
            }

            if (expression is UnaryExpression unaryExpression)
            {
                return unaryExpression.GetMemberExpression();
            }

            return null;
        }

        public static MemberExpression GetMemberExpression(this UnaryExpression unaryExpression)
        {
            return (MemberExpression)unaryExpression.Operand;
        }

        public static void IterateWithIndex<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            var index = 0;
            foreach (var item in source) action(item, index++);
        }
    }
}
