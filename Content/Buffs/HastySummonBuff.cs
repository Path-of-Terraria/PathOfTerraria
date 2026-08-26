namespace PathOfTerraria.Content.Buffs;

internal class HastySummonBuff : ModBuff
{
	public override void Update(Player player, ref int buffIndex)
	{
		player.moveSpeed += 0.5f;
	}
}
