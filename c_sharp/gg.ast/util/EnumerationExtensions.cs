/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;
using System.Collections.Generic;
using System.Linq;

namespace gg.ast.util
{
    /// <summary>
    /// Extensions on enumerations
    /// </summary>
    public static class EnumerationExtensions
    {
        /// <summary>
        /// Checks if the given predicate is true for all elements in the data.
        /// </summary>
        /// <typeparam name="T">Type of the element in the enumeration</typeparam>
        /// <param name="enumeration"></param>
        /// <param name="test">Predicate which takes an element of T and the index of the element in the enumeration</param>
        /// <returns>True if the predicate evaluates to true for all elements in the enumeration, false otherwise.</returns>
        public static bool AllIndexed<T>(this IEnumerable<T> enumeration, Func<T, int, bool> test)
        {
            for (var i = 0; i < enumeration.Count(); i++)
            {
                if (!test(enumeration.ElementAt(i), i))
                {
                    return false;
                }
            }

            return true;
        }

        public static void ForEachIndexed<T>(this IEnumerable<T> enumeration, Action<T, int> action)
        {
            for (var i = 0; i < enumeration.Count(); i++)
            {
                action(enumeration.ElementAt(i), i);
            }
        }
    }
}
