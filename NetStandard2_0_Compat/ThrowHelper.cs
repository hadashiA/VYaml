using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    internal enum ExceptionArgument
    {
        length,
        start,
        minimumBufferSize,
        elementIndex,
        comparable,
        comparer,
        destination,
        offset,
        startSegment,
        endSegment,
        startIndex,
        endIndex,
        array,
        culture,
        manager,
        count
    }

    internal class ThrowHelper
    {
        public static void ThrowArgumentOutOfRangeException(ExceptionArgument argument)
        {

        }

        public static void ThrowArgumentOutOfRangeException_OffsetOutOfRange()
        {

        }
    }
}
