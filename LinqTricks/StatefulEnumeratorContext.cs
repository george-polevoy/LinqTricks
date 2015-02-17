using System;
using System.Collections.Generic;

namespace LinqTricks
{
    public class StatefulEnumeratorContext<T>
    {
        private readonly IEnumerator<T> _enumerator;

        private bool _done;

        private bool _hasNext;

        private Queue<T> _pending = new Queue<T>();

        public StatefulEnumeratorContext(IEnumerator<T> enumerator)
        {
            _enumerator = enumerator;
            _done = !enumerator.MoveNext();
            _hasNext = !_done;
        }

        public bool IsDone()
        {
            return _done;
        }

        public IEnumerable<T> CompensateOne()
        {
            return Compensate(1);
        }

        public IEnumerable<T> CompensateOneX()
        {
            var last = default(T);
            bool lastWasPending = false;
            try
            {
                while (_pending.Count > 0)
                {
                    lastWasPending = true;
                    yield return last = _pending.Dequeue();
                }

                while (!_done)
                {
                    lastWasPending = false;
                    yield return last = _enumerator.Current;
                    _done = !_enumerator.MoveNext();
                }
            }
            finally
            {
                if (!_done)
                {
                    _pending.Enqueue(last);
                }
                if (!lastWasPending)
                    _done = !_enumerator.MoveNext();
            }
        }

        public IEnumerable<T> Compensate(int balance)
        {
            var newQueue = new Queue<T>(balance);
            if (balance < 1)
                throw new ArgumentOutOfRangeException("balance", balance, "Must be greater than one");
            var last = default(T);
            bool lastWasPending = false;
            try
            {
                while (_pending.Count > 0)
                {
                    lastWasPending = true;
                    last = _pending.Dequeue();
                    if (newQueue.Count >= balance)
                        newQueue.Dequeue();
                    newQueue.Enqueue(last);
                    yield return last;
                }

                while (!_done)
                {
                    lastWasPending = false;
                    last = _enumerator.Current;
                    if (newQueue.Count >= balance)
                        newQueue.Dequeue();
                    newQueue.Enqueue(last);
                    yield return last;
                    _done = !_enumerator.MoveNext();
                }
            }
            finally
            {
                //if (!_done)
                //{
                //    _pending.Enqueue(last);
                //}
                
                if (!lastWasPending)
                    _done = !_enumerator.MoveNext();

                foreach (var i in _pending)
                {
                    newQueue.Enqueue(i);
                }
                _pending = newQueue;
            }
        }

        public IEnumerable<T> Continue()
        {
            bool lastWasPending = false;
            try
            {
                while (_pending.Count > 0)
                {
                    lastWasPending = true;
                    yield return _pending.Dequeue();
                }

                while (!_done)
                {
                    lastWasPending = false;
                    yield return _enumerator.Current;
                    _done = !_enumerator.MoveNext();
                }
            }
            finally
            {
                if (!_done && _pending.Count < 1 && !lastWasPending)
                    _done = !_enumerator.MoveNext();
            }
        }
    }
}