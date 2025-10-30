using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class ScorchingFlamesMastery : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.Buffer.Add(ModContent.BuffType<ScorchingFlamesDebuff>(), 10 * 60, Value / 100f);
	}
}
