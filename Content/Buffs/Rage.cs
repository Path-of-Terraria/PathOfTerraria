namespace PathOfTerraria.Content.Buffs;

public class Rage : ModBuff
{
	public override void Update(Player player, ref int buffIndex)
	{ 
		player.GetDamage(DamageClass.Generic) += 1.5f;
	}
}