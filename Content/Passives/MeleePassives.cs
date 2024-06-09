using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class CloseRangePassive : Passive
{
	public CloseRangePassive()
	{
		Name = "Close Combatant";
		Tooltip = "Increases your damage against nearby enemies by 10% per level";
		MaxLevel = 3;
		TreePos = new Vector2(160, 300);
		Classes = [PlayerClass.Melee, PlayerClass.Ranged];
	}
}

internal class BleedPassive : Passive
{
	public BleedPassive()
	{
		Name = "Crimson Dance";
		Tooltip = "Your melee attacks inflict bleeding, dealing 5 damage per second per level";
		MaxLevel = 3;
		TreePos = new Vector2(180, 230);
		Classes = [PlayerClass.Melee];
	}
}