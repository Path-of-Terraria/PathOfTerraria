using System;
using System.Collections;
using System.Collections.Generic;
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

	private uint nextIndex = 0;
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

	/// <summary> Allocates a new handle and inserts the provided value into its data. </summary>
	public THandle Put(TData value)
	{
		HandleRefPair pair = AllocateHandle();
		pair.Ref = value;
		return pair.Handle;
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
		return handle.Index < nextIndex && versions[handle.Index] == handle.Version;
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

				if (index >= nextIndex) { continue; }

				versions[index]++;
			}
		}

		Array.Fill(items, default);
		Array.Fill(presenceMasks, default);
	}

	private HandleRefPair AllocateHandle()
	{
		uint index;
		if (PopFreeIndex() is uint freeIndex && freeIndex < nextIndex)
		{
			index = freeIndex;
		}
		else
		{
			index = nextIndex++;
		}

		// Mark index as taken.
		(uint div, uint rem) = Math.DivRem(index, BitMask.BitSize);
		presenceMasks[div].Set((int)rem);

		Count++;

		uint version = versions[index] = Math.Max(1, versions[index]);
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
		if (handle.Index >= nextIndex) { return false; }

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
		foreach (BitMask<ulong> mask in presenceMasks)
		{
			foreach (int bit in ~mask)
			{
				return (uint)bit;
			}
		}

		return null;
	}

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
