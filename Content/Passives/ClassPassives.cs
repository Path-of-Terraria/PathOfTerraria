using PathOfTerraria.Common.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class MartialMasteryPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Melee) += 0.05f * Level;
	}
}

internal class MarksmanshipMasteryPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Ranged) += 0.05f * Level;
	}
}

internal class ArcaneMasteryPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Magic) += 0.05f * Level;
	}
}

internal class SummoningMasteryPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Summon) += 0.05f * Level;
	}
}