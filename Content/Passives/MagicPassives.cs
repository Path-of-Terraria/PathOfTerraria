using PathOfTerraria.Core.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class ManaPassive : Passive
{
	public override string InternalIdentifier => "AddedMana";
	public override string Name => "Open Mind";
	public override string Tooltip => "Increases your maximum mana by 20 per level";
}