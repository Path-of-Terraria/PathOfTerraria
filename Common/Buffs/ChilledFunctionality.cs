using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.ModPlayers;
using Terraria.ID;

namespace PathOfTerraria.Common.Buffs;

internal class ChilledFunctionality : GlobalBuff
{
	public override void Update(int type, NPC npc, ref int buffIndex)
	{
		if (type == BuffID.Chilled && npc.TryGetGlobalNPC(out ChilledNPC chilled) && chilled.BossChilledCooldown <= 0)
		{
			float modifier = 0.2f;

			if (npc.lastInteraction != 255)
			{
				modifier = Main.player[npc.lastInteraction].GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ChilledEffectiveness.ApplyTo(0.2f);
			}

			if (npc.boss)
			{
				modifier *= 0.33f;
			}

			npc.GetGlobalNPC<SlowDownNPC>().SpeedModifier += modifier;

			if (Main.rand.NextBool(20))
			{
				Dust.NewDust(npc.position, npc.width, npc.height, DustID.Ice, npc.velocity.X, npc.velocity.Y);
			}

			if (npc.buffTime[buffIndex] == 2 && npc.boss)
			{
				chilled.BossChilledCooldown = 10 * 60;
			}
		}
	}

	private class ChilledNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		internal int BossChilledCooldown = 0;

		public override Color? GetAlpha(NPC npc, Color drawColor)
		{
			if (npc.HasBuff(BuffID.Chilled))
			{
				return Color.Lerp(drawColor, Color.LightBlue, 0.8f);
			}

			return null;
		}
	}
}
