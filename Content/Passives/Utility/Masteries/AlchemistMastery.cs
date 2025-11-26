using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class AlchemistMastery : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.PotionHealDelay *= 1 - Value / 100f;
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.PotionManaDelay *= 1 - Value / 100f;
	}
}
