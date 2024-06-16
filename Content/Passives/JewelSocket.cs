using PathOfTerraria.Core.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class JewelSocket : Passive
{
	public Jewel Socketed;
	public override string InternalIdentifier => "JewelSocket";
	public override void BuffPlayer(Player player)
	{
		Socketed?.BuffPlayer(player);
	}
}