using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class SpelunkyMastery : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.AddBuff(BuffID.Spelunker, 2);
		player.AddBuff(BuffID.Mining, 2);
	}
}
