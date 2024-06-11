using PathOfTerraria.Core.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class CloseRangePassive : Passive
{
	public override int Id => 7;
	public override string Name => "Close Combatant";
	public override string Tooltip => "Increases your damage against nearby enemies by 10% per level";
}

internal class BleedPassive : Passive
{
	public override int Id => 8;
	public override string Name => "Crimson Dance";
	public override string Tooltip => "Your melee attacks inflict bleeding, dealing 5 damage per second per level";
}