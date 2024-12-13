using PathOfTerraria.Common.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AnchorPassive : Passive
{
}

internal class IncreasedMeleeDamagePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Melee) += 0.05f * Level;
	}
}

internal class IncreasedRangedDamagePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Ranged) += 0.05f * Level;
	}
}

internal class IncreasedMagicDamagePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Magic) += 0.05f * Level;
	}
}

internal class IncreasedSummonDamagePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Summon) += 0.05f * Level;
	}
}