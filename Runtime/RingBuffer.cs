///
/// RingBuffer by Nothke
///
/// A simple fixed-sized, contiguous, double-sided indexable ring buffer.
/// PushBack/PushFront automatically overwrite if over capacity.
/// Not safe for threading.
/// Indexing, pushing and removing elements from ends is O(1)
/// Cannot insert an element in between.
/// 
/// ============================================================================
///
/// MIT License
///
/// Copyright(c) 2021 Ivan Notaro�
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all
/// copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.
/// 
/// ============================================================================
///

using System.Collections;
using System.Collections.Generic;
using System;

namespace Nothke.Collections
{
    /// <summary>
    /// A simple fixed-sized double-sided indexable ring buffer. PushBack/PushFront automatically overwrite if over capacity. Not safe for threading.
    /// </summary>
    public class RingBuffer<T> : IEnumerable<T>, IReadOnlyCollection<T>, ICollection
    {
        T[] array;
        readonly int size;

        int end = 0;
        int start = 0;
        int count = 0;

        public int Count => count;

        public bool IsSynchronized => true; // Not sure
        public object SyncRoot => true; // Not sure

        public RingBuffer(ICollection<T> collection)
        {
            size = collection.Count;
            array = new T[size];

            int i = 0;
            foreach (var c in collection)
            {
                array[i] = c;
                i++;
            }

            start = 0;
            end = size - 1;
        }

        public RingBuffer(int size)
        {
            this.size = size;
            array = new T[size];
        }

        public T this[int i]
        {
            get
            {
                if (i < 0 || i >= count)
                    throw new IndexOutOfRangeException();

                i = start + i;

                if (i >= size)
                    i %= size;

                return array[i];
            }

            set
            {
                if (i < 0 || i >= count)
                    throw new IndexOutOfRangeException();

                i = start + i;

                if (i >= size)
                    i %= size;

                array[i] = value;
            }
        }

        /// <summary>
        /// Adds an element to the back of the buffer. If the buffer is full the first value will be overwritten.
        /// </summary>
        public void PushBack(T element)
        {
            if (count == 0) // No moving
            {
                array[end] = element;
                count++;
                return;
            }

            end++;

            if (count < size)
                count++;

            if (end >= size)
            {
                end = 0;
            }

            if (end == start)
            {
                start++;

                if (start >= size)
                    start = 0;
            }

            array[end] = element;
        }

        /// <summary>
        /// Adds an element to the front of the buffer. If the buffer is full the last value will be overwritten.
        /// </summary>
        public void PushFront(T element)
        {
            if (count == 0) // No moving
            {
                array[end] = element;
                count++;
                return;
            }

            start--;

            if (count < size)
                count++;

            if (start < 0)
            {
                start = size - 1;
            }

            if (start == end)
            {
                end--;

                if (end < 0)
                    end = size - 1;
            }

            array[start] = element;
        }

        /// <summary>
        /// Removes the last element from the buffer.
        /// </summary>
        public T RemoveBack()
        {
            if (count == 0)
                throw new Exception("Empty, can't remove");

            T deq = array[end];

            end--;

            if (end < 0)
                end = size - 1;

            count--;

            return deq;
        }

        /// <summary>
        /// Removes the first element from the buffer.
        /// </summary>
        public T RemoveFront()
        {
            if (count == 0)
                throw new Exception("Empty, can't remove");

            T deq = array[start];

            start++;

            if (start >= size)
                start = 0;

            count--;

            return deq;
        }

        /// <summary>
        /// Last element
        /// </summary>
        public T PeekBack()
        {
            return array[end];
        }

        /// <summary>
        /// First element
        /// </summary
        public T PeekFront()
        {
            return array[start];
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (count == 0)
                yield break;

            int cur = start;
            yield return array[cur];

            if (count == 1)
                yield break;

            do
            {
                cur++;

                if (cur >= size)
                    cur = 0;

                yield return array[cur];

            } while (cur != end);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (count == 0)
                yield break;

            int cur = start;
            yield return array[cur];

            if (count == 1)
                yield break;

            do
            {
                cur++;

                if (cur >= size)
                    cur = 0;

                yield return array[cur];

            } while (cur != end);
        }

        /// <summary>
        /// Untested
        /// </summary>
        public void CopyTo(Array arrayTarget, int index)
        {
            if (count == 0)
                return;

            var target = array as T[];

            int ai = 0;

            int cur = start;
            target[ai] = array[cur];

            if (count == 1)
                return;

            do
            {
                cur++;

                if (cur >= size)
                    cur = 0;

                target[ai] = array[cur];

            } while (cur != end);
        }
    }
}