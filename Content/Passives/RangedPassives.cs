using PathOfTerraria.Core.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AmmoPassive : Passive
{
	public AmmoPassive()
	{
		Name = "Secret Compartment";
		Tooltip = "5% chance to not consume ammo per level";
		MaxLevel = 3;
		TreePositions = [new Vector2(300, 180)];
	}
}