using PathOfTerraria.Common.Events;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Buffs;

public class Battleaxe : ModBuff
{
	public override void Update(Player player, ref int buffIndex)
	{ 
		player.GetDamage(DamageClass.Generic) += 1.5f; 
		
		Lighting.AddLight(player.Center, 1f, 0f, 0f);
	}
}
