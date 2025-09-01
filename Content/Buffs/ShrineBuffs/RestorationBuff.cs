using PathOfTerraria.Content.Tiles.Maps;
using Terraria.ID;

namespace PathOfTerraria.Content.Buffs.ShrineBuffs;

internal class RestorationBuff : ShrineBuff
{
	public override int AoEType => ModContent.ProjectileType<RestorationAoE>();

	public override void Update(NPC npc, ref int buffIndex)
	{
		if (npc.life < npc.lifeMax)
		{
			npc.lifeRegen = (int)(npc.lifeMax / 7f);

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
