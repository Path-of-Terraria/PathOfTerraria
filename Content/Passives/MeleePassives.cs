using PathOfTerraria.Core;
using PathOfTerraria.Core.Systems.ModPlayers;
using PathOfTerraria.Core.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class CloseRangePassive : Passive
{
	public override string InternalIdentifier => "IncreasedCloseDamage";
}

internal class BleedPassive : Passive
{
	public override string InternalIdentifier => "BleedingDamageOverTime";
}

internal class DamageReductionPassive : Passive
{
	public override string InternalIdentifier => "IncreasedDamageReduction";

	public override void BuffPlayer(Player player)
	{
		player.endurance += 0.025f * Level;
	}
}

internal class DamageReflectionPassive : Passive
{
	public override string InternalIdentifier => "AddedContactDamageReflection";

	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ReflectedDamageModifier.Base += 10 * Level;
	}
}