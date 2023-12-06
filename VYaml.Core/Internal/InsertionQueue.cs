#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace VYaml.Internal
{
    class InsertionQueue<T>
    {
        const int MinimumGrow = 4;
        const int GrowFactor = 200;

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            private set;
        }

        T[] array;
        int headIndex;
        int tailIndex;

        public InsertionQueue(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity");
            array = new T[capacity];
            headIndex = tailIndex = Count = 0;
        }

        public void Clear()
        {
            headIndex = tailIndex = Count = 0;
        }

        public T Peek()
        {
            if (Count == 0) ThrowForEmptyQueue();
            return array[headIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(T item)
        {
            if (Count == array.Length)
            {
                Grow();
            }

            array[tailIndex] = item;
            MoveNext(ref tailIndex);
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Dequeue()
        {
            if (Count == 0) ThrowForEmptyQueue();

            var removed = array[headIndex];
            MoveNext(ref headIndex);
            Count--;
            return removed;
        }

        public void Insert(int posTo, T item)
        {
            if (Count == array.Length)
            {
                Grow();
            }

            MoveNext(ref tailIndex);
            Count++;

            for (var pos = Count - 1; pos > posTo; pos--)
            {
                var index = (headIndex + pos) % array.Length;
                var indexPrev = index == 0 ? array.Length - 1 : index - 1;
                array[index] = array[indexPrev];
            }
            array[(posTo + headIndex) % array.Length] = item;
        }

        void Grow()
        {
            var newCapacity = (int)((long)array.Length * GrowFactor / 100);
            if (newCapacity < array.Length + MinimumGrow)
            {
                newCapacity = array.Length + MinimumGrow;
            }
            SetCapacity(newCapacity);
        }

        void SetCapacity(int capacity)
        {
            var newArray = new T[capacity];
            if (Count > 0)
            {
                if (headIndex < tailIndex)
                {
                    Array.Copy(array, headIndex, newArray, 0, Count);
                }
                else
                {
                    Array.Copy(array, headIndex, newArray, 0, array.Length - headIndex);
                    Array.Copy(array, 0, newArray, array.Length - headIndex, tailIndex);
                }
            }

            array = newArray;
            headIndex = 0;
            tailIndex = Count == capacity ? 0 : Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void MoveNext(ref int index)
        {
            index = (index + 1) % array.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ThrowForEmptyQueue()
        {
            throw new InvalidOperationException("EmptyQueue");
        }
    }
}

