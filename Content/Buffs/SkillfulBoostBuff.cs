using PathOfTerraria.Common.Systems.ModPlayers;

namespace PathOfTerraria.Content.Buffs;

public sealed class SkillfulBoostBuff : ModBuff
{
	public override void Update(Player player, ref int buffIndex)
	{
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.CriticalDamage += 0.45f;
		player.moveSpeed += 0.5f;
	}
}