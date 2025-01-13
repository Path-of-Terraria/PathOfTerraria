namespace PathOfTerraria.Common.UI.Armor;

public sealed class AccessorySlotPlayer : ILoadable
{
	void ILoadable.Load(Mod mod)
	{
		On_Player.IsItemSlotUnlockedAndUsable += Player_IsItemSlotUnlockedAndUsable_Hook;
	}
	
	void ILoadable.Unload() { }

	private static bool Player_IsItemSlotUnlockedAndUsable_Hook(On_Player.orig_IsItemSlotUnlockedAndUsable orig, Player self, int slot)
	{
		return true;
	}
}