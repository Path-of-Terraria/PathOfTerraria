using PathOfTerraria.Core.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class LifePassive : Passive
{
	public override string InternalIdentifier => "AddedLife";
	public override string Name => "Empowered Flesh";
	public override string Tooltip => "Increases your maximum life by 20 per level";
	
	public override void BuffPlayer(Player player)
	{
		player.statLifeMax2 += 20 * Level;
	}
}