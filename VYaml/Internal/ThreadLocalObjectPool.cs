#nullable enable
using System;
using System.Collections.Generic;

namespace VYaml.Internal
{
    // A per-thread free list of reusable working buffers.
    //
    // Each Rent() hands out a distinct instance, so a nested (same-thread) operation never
    // shares buffers with the operation it is nested inside — which is the corruption that a
    // single shared [ThreadStatic] instance caused. At the same time, completed operations
    // Return() their buffers so the steady state (non-nested calls) reuses them instead of
    // allocating, just like the previous thread-static design.
    //
    // Being [ThreadStatic], it needs no synchronization: rent and return for a given operation
    // always happen on the same thread.
    static class ThreadLocalObjectPool<T> where T : class
    {
        [ThreadStatic]
        static Stack<T>? freeList;

        public static T Rent(Func<T> factory)
        {
            var list = freeList;
            if (list is { Count: > 0 })
            {
                return list.Pop();
            }
            return factory();
        }

        public static void Return(T item)
        {
            (freeList ??= new Stack<T>(4)).Push(item);
        }
    }
}
