using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Content.Passives.Misc;

internal class IgnitesDealDamageFasterPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		float modifier = Math.Max(0f, 1f - (Value / 100f));
		player.GetModPlayer<IgnitedPlayer>().IgniteTickRate *= modifier;
	}
}

internal class IgniteDurationPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<IgnitedPlayer>().IgniteDuration += Value / 100f;
	}
}

internal class IgniteDamagePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<IgnitedPlayer>().IgniteDamage += Value / 100f;
	}
}
