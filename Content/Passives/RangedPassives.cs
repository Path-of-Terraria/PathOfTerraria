using PathOfTerraria.Core.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AmmoPassive : Passive
{
	public override int Id => 9;
	public override string Name => "Secret Compartment";
	public override string Tooltip => "5% chance to not consume ammo per level";
}