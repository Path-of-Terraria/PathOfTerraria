namespace PathOfTerraria.Content.Buffs;

internal class DwarvishMastery : ModBuff
{
	public override void Update(Player player, ref int buffIndex)
	{
		player.pickSpeed += 1f;
	}
}
