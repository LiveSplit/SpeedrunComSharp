﻿using System.Collections.Generic;

namespace SpeedrunComSharp;

internal static class CacheExtensions
{
    internal static IEnumerable<T> Cache<T>(this IEnumerable<T> enumerable)
    {
        return new CachedEnumerable<T>(enumerable);
    }
}

internal class CachedEnumerable<T> : IEnumerable<T>
{
    private readonly IEnumerable<T> baseEnumerable;
    private IEnumerator<T> baseEnumerator;
    private readonly List<T> cachedElements;

    public CachedEnumerable(IEnumerable<T> baseEnumerable)
    {
        this.baseEnumerable = baseEnumerable;
        cachedElements = [];
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (T element in cachedElements)
        {
            yield return element;
        }

        baseEnumerator ??= baseEnumerable.GetEnumerator();

        while (baseEnumerator.MoveNext())
        {
            T current = baseEnumerator.Current;
            cachedElements.Add(current);
            yield return current;
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
