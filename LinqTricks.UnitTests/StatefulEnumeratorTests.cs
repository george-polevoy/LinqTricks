using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace LinqTricks.UnitTests
{
    public class StatefulEnumeratorTests
    {
        
        [Test]
        public void MultiplePending1()
        {
            using (var e = new[] { 1, 2, 3, 4, 5, 6 }.ToList().GetEnumerator())
            {
                var state = e.CreateStatefulEnumerator();

                var its3 = state.Compensate(1).TakeWhile(i => i < 4).Count();

                var rest = state.Continue().ToList();

                CollectionAssert.AreEquivalent(new[] { 4, 5, 6 }, rest);
            }
        }

        [Test]
        public void MultiplePending2()
        {
            using (var e = new[]{1,2,3,4,5,6}.ToList().GetEnumerator())
            {
                var state = e.CreateStatefulEnumerator();

                var its3 = state.Compensate(2).TakeWhile(i => i < 4).Count();

                var rest = state.Continue().ToList();

                CollectionAssert.AreEquivalent(new[]{3,4,5,6}, rest);
            }
        }

        [Test]
        public void MultiplePending4()
        {
            using (var e = new[] { 1, 2, 3, 4, 5, 6 }.ToList().GetEnumerator())
            {
                var state = e.CreateStatefulEnumerator();

                var its3 = state.Compensate(4).TakeWhile(i => i < 4).Count();

                var rest = state.Continue().ToList();

                CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4, 5, 6 }, rest);
            }
        }

        [Test]
        public void MultiplePending5()
        {
            using (var e = new[] { 1, 2, 3, 4, 5, 6 }.ToList().GetEnumerator())
            {
                var state = e.CreateStatefulEnumerator();

                var its3 = state.Compensate(5).TakeWhile(i => i < 4).Count();

                var rest = state.Continue().ToList();

                CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4, 5, 6 }, rest);
            }
        }

        [Test]
        public void MultiplePendingEnd()
        {
            using (var e = new[] { 1, 2, 3, 4, 5, 6 }.ToList().GetEnumerator())
            {
                var state = e.CreateStatefulEnumerator();

                var its3 = state.Continue().Take(3).Count();

                var rest = state.Compensate(4).ToList();

                CollectionAssert.AreEquivalent(new[] { 4, 5, 6 }, rest);
            }
        }

        [Test]
        public void FindSubsec()
        {
            using (var e = Enumerable.Range(1,10000).Select(i=>i%100).GetEnumerator())
            {
                var state = e.CreateStatefulEnumerator();

                var set = new HashSet<int> {1, 2};
                foreach (var token in state.TakeSubsequences(2).Where(t=>set.SetEquals(t)).Take(3))
                {
                    Console.WriteLine("Found");
                    foreach (var i in token)
                        Console.WriteLine(i);
                }
                Console.WriteLine(state.Continue().Count());
            }
        }
        
        [Test]
        public void TestStatefulEnumeration()
        {
            using (var e = Enumerable.Range(0, 20).Select(i=>1<<i).GetEnumerator())
            {
                var state = e.CreateStatefulEnumerator();
                
                var sumUpTo2 = state.CompensateOne().TakeWhile(i => i <= 2).Sum();
                Assert.IsFalse(state.IsDone());
                Assert.AreEqual(3, sumUpTo2);

                var anyMore = state.CompensateOne().Any();
                Assert.IsTrue(anyMore);
                Assert.IsFalse(state.IsDone());
                
                var its4 = state.CompensateOne().First();
                Assert.AreEqual(4, its4);
                Assert.IsFalse(state.IsDone());

                var its32 = state.CompensateOne().TakeWhile(i => i <= 32).Last();
                Assert.AreEqual(32, its32);
                Assert.IsFalse(state.IsDone());

                var next = state.Continue().First();
                Assert.AreEqual(64, next);
                Assert.IsFalse(state.IsDone());
                
                var theRest = state.Continue().TakeWhile(i=>i < 512).ToList();
                CollectionAssert.AreEquivalent(new[]{128,256}, theRest);
                Assert.IsFalse(state.IsDone());
                
                Assert.Less(1, state.Continue().Count());
                Assert.IsTrue(state.IsDone());
            }
        }

        [Test]
        [TestCase("", "", 1)]
        [TestCase("", "", 2)]
        [TestCase("1", "(1)", 1)]
        [TestCase("1", "(1)", 2)]
        [TestCase("1,2", "(1)(2)",1)]
        [TestCase("1,2", "(1,2)", 2)]
        [TestCase("1,2", "(1,2)", 3)]
        [TestCase("1,2,3", "(1)(2)(3)", 1)]
        [TestCase("1,2,3", "(1,2)(3)", 2)]
        [TestCase("1,2,3", "(1,2,3)", 3)]
        public void TestSplitToStatefulSeries(string sourceStringRepr, string expectedStringRepr, int n)
        {
            var source = sourceStringRepr.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
            var q = source.TakeStatefulSeries(n);
            var actualStringRepr =
                q.Aggregate(new StringBuilder(), (sb, item) => sb.Append("(").Append(string.Join(",", item)).Append(")"))
                    .ToString();
            Assert.AreEqual(expectedStringRepr, actualStringRepr);
        }
    }
}
