using System;
using System.Runtime.CompilerServices;
using VYaml.Parser;

namespace VYaml.Internal
{
    ref struct TokenQueue
    {
        const int MinimumGrow = 4;
        const int GrowFactor = 200;

        TokenType[] array;
        ITokenContent?[] contentArray;

        int headIndex;
        int tailIndex;

        public TokenQueue(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException("capacity");
            array = new TokenType[capacity];
            contentArray = new ITokenContent?[capacity];
            headIndex = tailIndex = Length = 0;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            private set;
        }

        public (TokenType, ITokenContent?) Peek()
        {
            if (Length == 0) ThrowForEmptyQueue();
            return (array[headIndex], contentArray[headIndex]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(TokenType type)
        {
            if (Length >= array.Length)
            {
                Grow();
            }

            array[tailIndex] = type;
            MoveNext(ref tailIndex);
            Length++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(TokenType type, ITokenContent content)
        {
            if (Length >= array.Length)
            {
                Grow();
            }

            array[tailIndex] = type;
            contentArray[tailIndex] = content;
            MoveNext(ref tailIndex);
            Length++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (TokenType type, ITokenContent? content) Dequeue()
        {
            if (Length == 0) ThrowForEmptyQueue();

            var type = array[headIndex];
            var content = contentArray[headIndex];
            MoveNext(ref headIndex);
            Length--;
            return (type, content);
        }

        public void Insert(int posTo, TokenType type, ITokenContent? content = null)
        {
            if (Length >= array.Length)
            {
                Grow();
            }

            MoveNext(ref tailIndex);
            Length++;

            for (var pos = Length - 1; pos > posTo; pos--)
            {
                var index = (headIndex + pos) % array.Length;
                var indexPrev = index == 0 ? array.Length - 1 : index - 1;

                array[index] = array[indexPrev];
                contentArray[index] = contentArray[indexPrev];
            }

            var i = (posTo + headIndex) % array.Length;
            array[i] = type;
            contentArray[i] = content;
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
            var newArray = new TokenType[capacity];
            var newContentArray = new ITokenContent?[capacity];

            if (Length > 0)
            {
                if (headIndex < tailIndex)
                {
                    Array.Copy(array, headIndex, newArray, 0, Length);
                    Array.Copy(contentArray, headIndex, newContentArray, 0, Length);
                }
                else
                {
                    Array.Copy(array, 0, newArray, array.Length - headIndex, tailIndex);
                    Array.Copy(array, headIndex, newArray, 0, array.Length - headIndex);

                    Array.Copy(contentArray, 0, newContentArray, contentArray.Length - headIndex, tailIndex);
                    Array.Copy(contentArray, headIndex, newContentArray, 0, contentArray.Length - headIndex);
                }
            }

            array = newArray;
            contentArray = newContentArray;

            headIndex = 0;
            tailIndex = Length == capacity ? 0 : Length;
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
