using System;
using System.Runtime.CompilerServices;

namespace VYaml.Internal
{
    class InsertionQueue<T>
    {
        const int MinimumGrow = 4;
        const int GrowFactor = 200;

        T[] array;
        int headIndex;
        int tailIndex;
        int queueSize;

        public InsertionQueue(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity");
            array = new T[capacity];
            headIndex = tailIndex = queueSize = 0;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => queueSize;
        }

        public T Peek()
        {
            if (queueSize == 0) ThrowForEmptyQueue();
            return array[headIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(T item)
        {
            if (queueSize == array.Length)
            {
                Grow();
            }

            array[tailIndex] = item;
            MoveNext(ref tailIndex);
            queueSize++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Dequeue()
        {
            if (queueSize == 0) ThrowForEmptyQueue();

            var removed = array[headIndex];
            MoveNext(ref headIndex);
            queueSize--;
            return removed;
        }

        public void Insert(int posTo, T item)
        {
            if (queueSize == array.Length)
            {
                Grow();
            }
            
            MoveNext(ref tailIndex);
            queueSize++;
            
            for (var pos = queueSize - 1; pos > posTo; pos--)
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
            if (queueSize > 0)
            {
                if (headIndex < tailIndex)
                {
                    Array.Copy(array, headIndex, newArray, 0, queueSize);
                }
                else
                {
                    Array.Copy(array, headIndex, newArray, 0, array.Length - headIndex);
                    Array.Copy(array, 0, newArray, array.Length - headIndex, tailIndex);
                }
            }

            array = newArray;
            headIndex = 0;
            tailIndex = queueSize == capacity ? 0 : queueSize;
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
