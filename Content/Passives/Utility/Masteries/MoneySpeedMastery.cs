using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class MoneySpeedMastery : Passive
{
	internal class MoneySpeedPlayer : ModPlayer
	{
		public override bool OnPickup(Item item)
		{
			if (item.IsACoin && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<MoneySpeedMastery>(out float value))
			{
				Player.AddBuff(ModContent.BuffType<AvariceBuff>(), (int)value * 60);
			}

			return true;
		}
	}
}
