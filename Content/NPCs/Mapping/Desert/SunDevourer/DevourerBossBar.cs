using Terraria.DataStructures;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;

internal class DevourerBossBar : ModBossBar
{
	public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
	{
		return npc.ai[2] > 0;
	}
}
