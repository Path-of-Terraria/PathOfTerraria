using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.VanillaModifications;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedManaRegenerationRatePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<ManaRegenRework.ManaRegenPlayer>().ManaRegen += Value * Level / 100f;
	}
}
