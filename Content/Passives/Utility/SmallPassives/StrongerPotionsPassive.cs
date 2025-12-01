using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Utilities;

namespace PathOfTerraria.Content.Passives;

internal class StrongerPotionsPassive : Passive
{
	private static bool CanBuffPotion = false;

	public override void OnLoad()
	{
		On_Player.QuickBuff += SetBuffFlag_QuickBuff;
		On_Player.QuickHeal += SetBuffFlag_QuickHeal;
		On_Player.ItemCheck_Inner += SetBuffFlag_ItemCheck_Inner;
		On_Player.AddBuff += BuffBuffTime;
	}

	private void BuffBuffTime(On_Player.orig_AddBuff orig, Player self, int type, int timeToAdd, bool quiet, bool foodHack)
	{
		if (self.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<StrongerPotionsPassive>(out float str) && !Main.debuff[type])
		{
			timeToAdd = (int)(timeToAdd * (1 + str / 100f));
		}

		orig(self, type, timeToAdd, quiet, foodHack);
	}

	private void SetBuffFlag_ItemCheck_Inner(On_Player.orig_ItemCheck_Inner orig, Player self)
	{
		using var _ = ValueOverride.Create(ref CanBuffPotion, true);
		orig(self);
	}

	private void SetBuffFlag_QuickHeal(On_Player.orig_QuickHeal orig, Player self)
	{
		using var _ = ValueOverride.Create(ref CanBuffPotion, true);
		orig(self);
	}

	private void SetBuffFlag_QuickBuff(On_Player.orig_QuickBuff orig, Player self)
	{
		using var _ = ValueOverride.Create(ref CanBuffPotion, true);
		orig(self);
	}
}
