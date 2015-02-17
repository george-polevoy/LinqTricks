using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqTricks
{
    public static class EnumerableExtensions
    {
        public static StatefulEnumeratorContext<T> CreateStatefulEnumerator<T>(this IEnumerator<T> enumerator)
        {
            return new StatefulEnumeratorContext<T>(enumerator);
        }

        public static IEnumerable<IEnumerable<T>> TakeStatefulSeries<T>(this IEnumerable<T> source, int partitionSize)
        {
            using (var e = source.GetEnumerator())
            {
                var state = e.CreateStatefulEnumerator();
                while (!state.IsDone())
                {
                    yield return state.Continue().Take(partitionSize);
                }
            }
        }

        public static IEnumerable<IList<T>> TakeSubsequences<T>(this StatefulEnumeratorContext<T> state, int maxLength) where T : IEquatable<T>
        {
            while (!state.IsDone())
            {
                var one = state.Compensate(1).Take(1).Count();

                if (one < 1)
                    yield break;

                var token = state.Compensate(maxLength - 1).Take(maxLength).ToList();

                yield return token;
            }
        }
    }
}
