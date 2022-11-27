using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace VYaml.Internal
{
    class ExpandBuffer<T> : IDisposable
    {
        const int MinimumGrow = 4;
        const int GrowFactor = 200;

        T[] buffer;
        bool disposed;

        public ExpandBuffer(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            buffer = ArrayPool<T>.Shared.Rent(capacity);
            Length = 0;
            disposed = false;
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref buffer[index];
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => buffer.Length;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set;
        }

        public void Dispose()
        {
            if (disposed) return;
            ArrayPool<T>.Shared.Return(buffer);
            disposed = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => buffer.AsSpan(0, Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length) => buffer.AsSpan(start, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Length = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Pop()
        {
            if (Length == 0)
                throw new InvalidOperationException("Cannot pop the empty buffer");
            return buffer[--Length];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T value)
        {
            if (Length == 0)
            {
                value = default!;
                return false;
            }
            value = Pop();
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            if (Length == buffer.Length)
            {
                Grow();
            }

            buffer[Length++] = item;
        }

        void Grow()
        {
            var newCapacity = buffer.Length * GrowFactor / 100;
            if (newCapacity < buffer.Length + MinimumGrow)
            {
                newCapacity = buffer.Length + MinimumGrow;
            }
            SetCapacity(newCapacity);
        }

        void SetCapacity(int newCapacity)
        {
            if (Capacity >= newCapacity) return;

            var newBuffer = ArrayPool<T>.Shared.Rent(newCapacity);
            Array.Copy(buffer, 0, newBuffer, 0, Length);
            ArrayPool<T>.Shared.Return(buffer);
            buffer = newBuffer;
        }
    }
}
