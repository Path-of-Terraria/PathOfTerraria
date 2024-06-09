using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class ManaPassive : Passive
{
	public ManaPassive()
	{
		Name = "Open Mind";
		Tooltip = "Increases your maximum mana by 20 per level";
		MaxLevel = 3;
		TreePos = new Vector2(500, 180);
		Classes = [PlayerClass.Magic];
	}
}