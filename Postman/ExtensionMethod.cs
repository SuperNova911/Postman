using System;
using System.Collections.Generic;
using System.Text;

namespace Postman
{
    public static class ExtensionMethod
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> collection, int splitSize)
        {
            splitSize = Math.Clamp(splitSize, 1, splitSize);

            IEnumerator<T> enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return NextChunk(enumerator, splitSize);
            }
        }

        private static IEnumerable<T> NextChunk<T>(IEnumerator<T> collection, int splitSize)
        {
            do
            {
                yield return collection.Current;
            }
            while (--splitSize > 0 && collection.MoveNext());
        }
    }
}
