using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class MinionPassive : Passive
{
	public MinionPassive()
	{
		Name = "Empowered Horde";
		Tooltip = "Increases your minions' damage by 10% per level";
		MaxLevel = 3;
		TreePos = new Vector2(640, 300);
		Classes = [PlayerClass.Summoner];
	}
}

internal class SentryPassive : Passive
{
	public SentryPassive()
	{
		Name = "Steadfast Sentries";
		Tooltip = "Increases your sentries' damage by 10% per level";
		MaxLevel = 3;
		TreePos = new Vector2(620, 230);
		Classes = [PlayerClass.Summoner];
	}
}