using Terraria.ID;

namespace PathOfTerraria.Content.Buffs;

internal class SwampAlgaeBuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
		Main.buffNoTimeDisplay[Type] = true;

		BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
	}

	public override void Update(Player player, ref int buffIndex)
	{
		player.buffTime[buffIndex] = 2;
		player.ignoreWater = true;
		player.breathEffectiveness += 5;
		
		if (player.wet)
		{
			Lighting.AddLight(player.Top + new Vector2(0, 16), new Vector3(0.3f, 0.3f, 0.1f) * 2);
		}
	}
}
