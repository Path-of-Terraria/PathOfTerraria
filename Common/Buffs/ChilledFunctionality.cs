using PathOfTerraria.Common.Systems;
using Terraria.ID;

namespace PathOfTerraria.Common.Buffs;

internal class ChilledFunctionality : GlobalBuff
{
	public override void Update(int type, NPC npc, ref int buffIndex)
	{
		if (type == BuffID.Chilled)
		{
			npc.GetGlobalNPC<SlowDownNPC>().SlowDown += 0.2f;

			if (Main.rand.NextBool(20))
			{
				Dust.NewDust(npc.position, npc.width, npc.height, DustID.Ice, npc.velocity.X, npc.velocity.Y);
			}
		}
	}

	private class ChilledNPC : GlobalNPC
	{
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
