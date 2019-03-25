using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ClientApp
{
    public static class Extensions
    {
        public static int RemoveAll<T>(this ObservableCollection<T> collection, Func<T, bool> condition)
        {
            var removeItemsList = collection.Where(condition).ToList();

            foreach (var item in removeItemsList)
                collection.Remove(item);

            return removeItemsList.Count;
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var x in enumerable)
            {
                action(x);
            }
        }
    }
}
