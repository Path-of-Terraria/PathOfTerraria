namespace PathOfTerraria.Content.Buffs;

internal class AvariceBuff : ModBuff
{
	public override void Update(Player player, ref int buffIndex)
	{
		player.moveSpeed += 1.2f;
	}
}
