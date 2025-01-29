using Terraria.DataStructures;

namespace PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;

public class GrovetenderBossBar : ModBossBar
{
	public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
	{
		return npc.ai[0] != 0;
	}
}