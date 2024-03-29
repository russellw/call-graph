﻿// In theory, this is O(N) per item
// but in practice on modern hardware
// it is fast for the anticipated range of N
sealed class OrderedSet<T>: List<T> {
	public new void Add(T item) { if (!Contains(item)) base.Add(item);
}
}
