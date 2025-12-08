using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Content.Passives;
using Terraria.Localization;

namespace PathOfTerraria.Content.Buffs;

internal class BloodroundBuff : ModBuff
{
	public override LocalizedText Description => base.Description.WithFormatArgs(BloodroundMastery.AmmoConsumptionChance);

	public override void Update(Player player, ref int buffIndex)
	{
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.AmmoReservationChance += BloodroundMastery.AmmoConsumptionChance / 100f;
	}
}
