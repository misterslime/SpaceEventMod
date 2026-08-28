using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod;

internal static class ListExtensions
{
    /// <summary>
    /// Swap the value at the “removed” index with the value at the back, and then remove from the back.
    /// This assumes list ordering does not matter, and is generally faster.
    /// 
    /// https://www.vertexfragment.com/ramblings/list-removal-performance/
    /// </summary>
    /// <param name="index">Index to remove at.</param>
    public static void RemoveUnorderedAt<T>(this List<T> list, int index)
    {
        list[index] = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
    }

    /// <summary>
    /// Fills the provided List with the specified value.
    /// The list will be cleared and that value will be inserted the specified number of times.<para/>
    /// 
    /// If count is not specified, then the current list size will be used (not capacity).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="value"></param>
    /// <param name="count"></param>
    public static void Fill<T>(this List<T> list, T value, int count = -1)
    {
        count = (count == -1) ? list.Count : count;

        list.Clear();

        for (int i = 0; i < count; ++i)
        {
            list.Add(value);
        }
    }

    /// <summary>
    /// Remove last element of list.
    /// </summary>
    /// <returns>The value that was removed.</returns>
    public static T Pop<T>(this List<T> values)
    {
        return Pop(values, values.Count - 1);
    }

    /// <summary>
    /// Remove at index.
    /// </summary>
    /// <returns>The value that was removed.</returns>
    public static T Pop<T>(this List<T> values, int index)
    {
        T v = values[index];
        values.RemoveAt(index);
        return v;
    }
}
