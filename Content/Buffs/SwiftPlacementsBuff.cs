using PathOfTerraria.Common.Systems.ModPlayers;

namespace PathOfTerraria.Content.Buffs;

public class SwiftPlacementsBuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.buffNoSave[Type] = true;
		Main.buffNoTimeDisplay[Type] = false;
	}

	public override void Update(Player player, ref int buffIndex)
	{
		player.moveSpeed += 0.25f; // +25% movement speed
		player.GetDamage(DamageClass.Summon) +=  0.15f; // 15% summon dmg
	}
}