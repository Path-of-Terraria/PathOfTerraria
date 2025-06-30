using Terraria.ID;

namespace PathOfTerraria.Content.Buffs;

public sealed class FrostflameDebuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		if (Main.rand.NextBool(35))
		{
			var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.IceTorch);
			d.noGravity = true;
			d.fadeIn = 1.2f;
		}

		npc.lifeRegen = Math.Min(npc.lifeRegen, 0);
		npc.lifeRegen -= 8;
	}

	public override void Update(Player player, ref int buffIndex)
	{
		if (Main.rand.NextBool(35))
		{
			var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.IceTorch);
			d.noGravity = true;
			d.fadeIn = 1.2f;
		}

		player.lifeRegen = Math.Min(player.lifeRegen, 0);
		player.lifeRegen -= 8;
		player.slow = true;
	}
}