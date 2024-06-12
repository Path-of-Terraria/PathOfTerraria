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

internal class DamageReductionPassive : Passive
{
	public DamageReductionPassive()
	{
		Name = "Iron Will";
		Tooltip = "Gain 0.25% damage reduction per level.";
		MaxLevel = 4;
		TreePos = new Vector2(260, 180);
		Classes = [PlayerClass.Melee];
	}

	public override void BuffPlayer(Player player)
	{
		player.endurance += 0.025f * Level;
	}
}