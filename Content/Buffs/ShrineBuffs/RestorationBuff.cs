using Terraria.ID;

namespace PathOfTerraria.Content.Buffs.ShrineBuffs;

internal class RestorationBuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		// This allows for otherwise buff immune NPCs to have this effect
		BuffID.Sets.IsATagBuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		if (npc.life < npc.lifeMax)
		{
			npc.lifeRegen = (int)(20 * (1 + (npc.lifeMax / 300f * 0.5f + 0.5f)));

			if (Main.rand.NextBool(20))
			{
				Dust.NewDust(npc.position, npc.width, npc.height, DustID.HealingPlus, Scale: Main.rand.NextFloat(1, 2));
			}
		}
	}

	public class RestorationPlayer : ModPlayer
	{
		public override void UpdateLifeRegen()
		{
			if (Player.HasBuff<RestorationBuff>())
			{
				Player.lifeRegen += 20;

				if (Main.rand.NextBool(20))
				{
					Dust.NewDust(Player.position, Player.width, Player.height, DustID.HealingPlus, Scale: Main.rand.NextFloat(1, 2));
				}
			}
		}
	}
}
