using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VYaml.SourceGenerator;

// A value-equatable wrapper over an array, so models that contain it get correct
// structural equality (T[] only has reference equality, which breaks incremental caching).
readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IReadOnlyList<T>
    where T : IEquatable<T>
{
    public static readonly EquatableArray<T> Empty = new(Array.Empty<T>());

    readonly T[] array;

    public EquatableArray(T[] array)
    {
        this.array = array;
    }

    public int Count => array?.Length ?? 0;

    public T this[int index] => array[index];

    public bool Equals(EquatableArray<T> other)
    {
        if (array is null) return other.array is null || other.array.Length == 0;
        if (other.array is null) return array.Length == 0;
        return array.AsSpan().SequenceEqual(other.array.AsSpan());
    }

    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        if (array is null) return 0;
        var hash = 17;
        foreach (var item in array)
        {
            hash = unchecked(hash * 31 + (item?.GetHashCode() ?? 0));
        }
        return hash;
    }

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)(array ?? Array.Empty<T>())).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

static class EquatableArrayExtensions
{
    public static EquatableArray<T> ToEquatableArray<T>(this IEnumerable<T> source)
        where T : IEquatable<T>
        => new(source as T[] ?? source.ToArray());
}
