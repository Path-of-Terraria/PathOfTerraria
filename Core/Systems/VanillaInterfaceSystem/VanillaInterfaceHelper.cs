using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace PathOfTerraria.Core.Systems.VanillaInterfaceSystem;

/// <summary>
///		Provides methods for easily interacting with interfaces that may be
///		applied to vanilla content.
/// </summary>
public static class VanillaInterfaceHelper
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetInterface<TType, TInterface>(this TType instance, [NotNullWhen(returnValue: true)] out TInterface value)
	{
		return AbstractVanillaInterfaceHandler<TType>.Instance.TryGetInterface(instance, out value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void AddItemInterface<TInterface>(int itemType, TInterface value)
	{
		AbstractVanillaInterfaceHandler<Item>.AddInterface(itemType, value);
	}
}
