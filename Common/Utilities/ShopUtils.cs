namespace PathOfTerraria.Common.Utilities;

internal static class ShopUtils
{
	public static bool TryCloneNpcShop(string shopName, int npcTypeToCloneTo)
	{
		if (!NPCShopDatabase.TryGetNPCShop(shopName, out AbstractNPCShop shop))
		{
			return false;
		}

		var cloneShop = new NPCShop(npcTypeToCloneTo);

		foreach (AbstractNPCShop.Entry entry in shop.ActiveEntries)
		{
			cloneShop.Add(new NPCShop.Entry(entry.Item, [.. entry.Conditions]));
		}

		cloneShop.Register();
		return true;
	}
}
