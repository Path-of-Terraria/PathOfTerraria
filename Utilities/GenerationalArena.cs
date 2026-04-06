using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using BitMask = PathOfTerraria.Utilities.BitMask<ulong>;

#nullable enable

namespace PathOfTerraria.Utilities;

/// <summary> A versioned handle. </summary>
public interface IHandle
{
	public uint Index { get; set; }
	public uint Version { get; set; }
}

/// <summary> A non-linear data structure which operates on versioned handles. </summary>
internal class GenerationalArena<THandle, TData> where THandle : unmanaged, IHandle
{
	/// <summary> A pair of a versioned handle and a temporary reference to its data. </summary>
	public ref struct HandleRefPair
	{
		public THandle Handle;
		public ref TData Ref;
	}

	/// <summary> The next index to linearly allocate. </summary>
	private TData[] items = [];
	private uint[] versions = [];
	private BitMask[] presenceMasks = [];

	/// <summary> The amount of active handle & data pairs this arena currently holds. </summary>
	public uint Count { get; private set; }

	/// <summary> The current capacity of the backing arrays. </summary>
	public uint Capacity => (uint)items.Length;

	public GenerationalArena(uint initialCapacity = 64)
	{
		EnsureCapacity(initialCapacity);
	}

#if DEBUG
	static GenerationalArena()
	{
		RuntimeHelpers.RunClassConstructor(typeof(GenArenaTests).TypeHandle);
	}
#endif

	/// <summary> Allocates a new handle and inserts the provided value into its data. </summary>
	public THandle Add(TData value)
	{
		HandleRefPair pair = AllocateHandle();
		pair.Ref = value;
		return pair.Handle;
	}
	/// <summary> Forces a given handle to become valid within this collection, returning a data reference. </summary>
	public ref TData Sync(THandle handle)
	{
		if (handle.Version == 0) { throw new InvalidOperationException("Cannot sync-in a zero-version handle."); }
		if (IsValid(handle)) { return ref Get(handle); }
		
		// Ensure allocation, update version.
		HandleRefPair pair = AllocateHandle(existing: handle);
		return ref pair.Ref;
	}

	/// <summary> Gets a data reference associated with the given handle. Asserts that the handle is valid. </summary>
	public ref TData Get(THandle handle)
	{
		Debug.Assert(IsValid(handle));
		return ref items[handle.Index];
	}

	/// <summary> Ensures that the backing arrays have can hold at least <paramref name="capacity"/> amount of items. </summary>
	public void EnsureCapacity(uint capacity)
	{
		int oldLength = versions.Length;
		if (capacity <= oldLength) { return; }
		int newLength = (int)BitOperations.RoundUpToPowerOf2(capacity);

		Array.Resize(ref items, newLength);
		Array.Resize(ref versions, newLength);
		//Array.Fill(versions, (uint)0, oldLength, newLength - oldLength);

		int oldMaskLength = presenceMasks.Length;
		int newMaskLength = (newLength / BitMask.BitSize) + 1;
		if (newMaskLength > oldMaskLength)
		{
			Array.Resize(ref presenceMasks, newMaskLength);
			//Array.Fill(presenceMasks, default, oldMaskLength, newMaskLength - oldMaskLength);
		}
	}

	/// <summary> Checks if a given handle is valid. </summary>
	public bool IsValid(THandle handle)
	{
		return handle.Version != 0 && handle.Index < versions.Length && versions[handle.Index] == handle.Version;
	}

	/// <summary> Invalidates the given handle, cleaning up its data. Returns whether the handle was valid to begin with. </summary>
	public bool Remove(THandle handle)
	{
		if (InvalidateHandle(handle))
		{
			items[handle.Index] = default!;
			return true;
		}

		return false;
	}

	/// <summary> Invalidates all handles and zeroes out memory. </summary>
	public void Clear()
	{
		// Invalidate handles.
		for (int baseIndex = 0, maskIndex = 0; maskIndex < presenceMasks.Length; maskIndex++, baseIndex += BitMask.BitSize)
		{
			foreach (int bitIndex in presenceMasks[maskIndex])
			{
				int index = baseIndex + bitIndex;
				versions[index]++;
			}
		}

		Array.Fill(items, default);
		Array.Fill(presenceMasks, default);
	}

	private HandleRefPair AllocateHandle(THandle? existing = null)
	{
		uint index;
		if (existing != null) { index = existing.Value.Index; }
		else if (PopFreeIndex() is uint freeIndex) { index = freeIndex; }
		else { index = (uint)versions.Length; }

		EnsureCapacity(index + 1);

		// Mark index as taken.
		(uint div, uint rem) = Math.DivRem(index, BitMask.BitSize);
		if (existing == null || !presenceMasks[div].Get((int)rem))
		{
			presenceMasks[div].Set((int)rem);
			Count++;
		}

		uint version = versions[index] = (existing?.Version ?? Math.Max(1, versions[index]));
		var handle = new THandle { Index = index, Version = version };

		return new HandleRefPair
		{
			Handle = handle,
			Ref = ref items[index],
		};
	}

	public Iterator GetEnumerator()
	{
		return new Iterator(this);
	}

	private bool InvalidateHandle(THandle handle)
	{
		if (handle.Index >= versions.Length) { return false; }

		uint expected = handle.Version;
		uint replacement = handle.Version + 1;
		if (Interlocked.CompareExchange(ref versions[handle.Index], replacement, expected) == expected)
		{
			Count = checked(Count - 1);
			// Mark index as free.
			(uint div, uint rem) = Math.DivRem(handle.Index, BitMask.BitSize);
			presenceMasks[div].Unset((int)rem);

			return true;
		}

		return false;
	}

	private uint? PopFreeIndex()
	{
		for (uint maskIndex = 0; maskIndex < presenceMasks.Length; maskIndex++)
		{
			BitMask mask = presenceMasks[maskIndex];

			foreach (int bit in ~mask)
			{
				return (uint)bit + (maskIndex * BitMask.BitSize);
			}
		}

		return null;
	}

	//TODO: Add IEnumerator<THandle> interface after TML updates to .NET ~10.
	public ref struct Iterator(GenerationalArena<THandle, TData> arena)
	{
		private readonly uint[] versions = arena.versions;
		private readonly BitMask[] presenceMasks = arena.presenceMasks;
		private int maskIndex = -1;
		private BitMask.Iterator inner;

		public THandle Current { get; private set; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			while (!inner.MoveNext())
			{
				if (++maskIndex >= presenceMasks.Length)
				{
					return false;
				}

				inner = new BitMask.Iterator(presenceMasks[maskIndex]);
			}

			uint bitIndex = (uint)inner.Current;
			uint totalIndex = (uint)((maskIndex * BitMask.BitSize) + bitIndex);

			Current = new THandle { Index = totalIndex, Version = versions[totalIndex] };

			return true;
		}
	}
}

#if DEBUG
internal static class GenArenaTests
{
	private record struct Handle(uint Index, uint Version) : IHandle;

	static GenArenaTests()
	{
		var arena = new GenerationalArena<Handle, string>(initialCapacity: 0);
		
		// Initial.
		foreach (Handle handle in arena) { Debug.Fail("Arena expected to be empty."); }
		Debug.Assert(!arena.IsValid(default));
		Debug.Assert(!arena.IsValid(new(2, 0)));
		Debug.Assert(!arena.IsValid(new(2, 1)));
		Debug.Assert(arena.Count == 0);
		Debug.Assert(!arena.GetEnumerator().MoveNext());
		
		// Addition.
		Handle a = arena.Add("a");
		Debug.Assert(arena.Count == 1);
		Handle b = arena.Add("b");
		Debug.Assert(arena.Count == 2);
		Handle c = arena.Add("c");
		Debug.Assert(arena.Count == 3);
		Debug.Assert(arena.IsValid(a));
		Debug.Assert(arena.IsValid(b));
		Debug.Assert(arena.IsValid(c));
		Debug.Assert(!arena.IsValid(a with { Version = 15 }));
		Debug.Assert(a.Index == 0);
		Debug.Assert(b.Index == 1);
		Debug.Assert(c.Index == 2);
		Debug.Assert(a.Version == 1);
		Debug.Assert(b.Version == 1);
		Debug.Assert(c.Version == 1);
		
		// Removal.
		Debug.Assert(arena.Remove(b));
		Debug.Assert(!arena.IsValid(b));
		Debug.Assert(!arena.Remove(b));
		Debug.Assert(arena.Count == 2);
		
		// Sync, or arbitrary handle insertion.
		Handle x = new(10, 5);
		Debug.Assert(!arena.IsValid(x));
		arena.Sync(x) = "x";
		Debug.Assert(arena.IsValid(x));
		Debug.Assert(arena.Count == 3);
		// Removal after sync.
		Debug.Assert(!arena.Remove(x with { Version = x.Version + 1 }));
		Debug.Assert(arena.Remove(x));
		Debug.Assert(!arena.IsValid(x));
		Debug.Assert(!arena.Remove(x));
		Debug.Assert(arena.Count == 2);
		// Readd for iteration test.
		arena.Sync(x) = "x";

		// Iteration.
		GenerationalArena<Handle, string>.Iterator it = arena.GetEnumerator();
		Debug.Assert(it.MoveNext());
		Debug.Assert(arena.Get(it.Current) == "a");
		Debug.Assert(it.MoveNext());
		Debug.Assert(arena.Get(it.Current) == "c");
		Debug.Assert(it.MoveNext());
		Debug.Assert(arena.Get(it.Current) == "x");
		Debug.Assert(!it.MoveNext());
	}
}
#endif
