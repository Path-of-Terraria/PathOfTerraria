namespace PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;

internal class GrovetenderSystem : ModSystem
{
	internal int GrovetenderWhoAmI = -1;

	public override void PreUpdateTime()
	{
		GrovetenderWhoAmI = NPC.FindFirstNPC(ModContent.NPCType<Grovetender>());
	}
}
