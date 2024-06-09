using PathOfTerraria.Core.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class MinionPassive : Passive
{
	public MinionPassive()
	{
		Name = "Empowered Horde";
		Tooltip = "Increases your minions' damage by 10% per level";
		MaxLevel = 3;
		TreePositions = [new Vector2(640, 300)];
	}
}

internal class SentryPassive : Passive
{
	public SentryPassive()
	{
		Name = "Steadfast Sentries";
		Tooltip = "Increases your sentries' damage by 10% per level";
		MaxLevel = 3;
		TreePositions = [new Vector2(620, 230)];
	}
}