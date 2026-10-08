using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Corona.SourceGeneration;

/// <summary>An immutable sequence with structural equality for incremental pipeline models.</summary>
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IReadOnlyList<T>
{
    private readonly ImmutableArray<T> items;
    public EquatableArray(IEnumerable<T> items) => this.items = ImmutableArray.CreateRange(items);
    public int Count => items.IsDefault ? 0 : items.Length;
    public T this[int index] => items[index];
    public bool Equals(EquatableArray<T> other)
    {
        if (Count != other.Count) return false;
        for (var i = 0; i < Count; i++)
            if (!EqualityComparer<T>.Default.Equals(this[i], other[i])) return false;
        return true;
    }
    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);
    public override int GetHashCode()
    {
        var hash = 17;
        for (var i = 0; i < Count; i++)
            hash = unchecked(hash * 31 + (this[i] is null ? 0 : EqualityComparer<T>.Default.GetHashCode(this[i]!)));
        return hash;
    }
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)(items.IsDefault ? ImmutableArray<T>.Empty : items)).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);
    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);
}
