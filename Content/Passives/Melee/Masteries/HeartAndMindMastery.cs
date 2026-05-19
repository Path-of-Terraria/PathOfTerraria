using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.VanillaModifications;

namespace PathOfTerraria.Content.Passives;

internal class HeartAndMindMastery : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<ManaRegenRework.ManaRegenPlayer>().ManaRegen.Flat += player.lifeRegen * Value / 100f;
	}
}
