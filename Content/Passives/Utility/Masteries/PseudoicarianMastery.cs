using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class PseudoicarianMastery : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.jumpSpeedBoost += Value * 0.01f * Level;
		player.AddBuff(BuffID.Featherfall, 2);
	}
}
