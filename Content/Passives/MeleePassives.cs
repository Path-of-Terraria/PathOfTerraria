using PathOfTerraria.Core.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class CloseRangePassive : Passive
{
	public override string InternalIdentifier => "IncreasedCloseDamage";
	public override string Name => "Close Combatant";
	public override string Tooltip => "Increases your damage against nearby enemies by 10% per level";
}

internal class BleedPassive : Passive
{
	public override string InternalIdentifier => "BleedingDamageOverTime";
	public override string Name => "Crimson Dance";
	public override string Tooltip => "Your melee attacks inflict bleeding, dealing 5 damage per second per level";
}