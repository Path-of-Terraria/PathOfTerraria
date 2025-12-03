using PathOfTerraria.Common.Systems.ModPlayers;

namespace PathOfTerraria.Content.Buffs;

internal class BloodroundBuff : ModBuff
{
	public override void Update(Player player, ref int buffIndex)
	{
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.AmmoReservationChance += 0.5f;
	}
}
