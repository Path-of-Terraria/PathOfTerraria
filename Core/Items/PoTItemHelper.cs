using System.Runtime.CompilerServices;

namespace PathOfTerraria.Core.Items;

/// <summary>
///		Utility for interacting with custom item data introduced by Path of
///		Terraria.
/// </summary>
public static class PoTItemHelper
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PoTInstanceItemData GetInstanceData(this Item item)
	{
		return item.GetGlobalItem<PoTInstanceItemData>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PoTInstanceItemData GetInstanceData(this ModItem item)
	{
		return item.Item.GetInstanceData();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PoTStaticItemData GetStaticData(this Item item)
	{
		return PoTGlobalItem.GetStaticData(item.type);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PoTStaticItemData GetStaticData(this ModItem item)
	{
		return item.Item.GetStaticData();
	}
}
