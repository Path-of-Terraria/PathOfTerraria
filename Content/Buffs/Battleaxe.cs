namespace PathOfTerraria.Content.Buffs;

public class Battleaxe : ModBuff
{
	public override string Texture => $"{nameof(PathOfTerraria)}/Assets/Buffs/Base";

	public override void Update(Player player, ref int buffIndex)
	{ 
		player.GetDamage(DamageClass.Generic) += 1.5f; 
		
		Lighting.AddLight(player.Center, 1f, 0f, 0f);
	}
}
