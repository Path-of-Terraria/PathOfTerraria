using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.BrainDomain;

public sealed class Minera : ModNPC
{
	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 2;
	}

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.Crimera);
		NPC.lifeMax = NPC.life = 10;
		NPC.defense /= 2;

		AnimationType = NPCID.Crimera;
		AIType = NPCID.Crimera;

		NPC.TryEnableComponent<NPCDeathEffects>(c => c.AddDust(DustID.Blood, 12));
	}
}