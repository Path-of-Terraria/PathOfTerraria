using NPCUtils;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.BrainDomain;

[AutoloadBanner]
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

		NPC.TryEnableComponent<NPCHitEffects>(c => 
		{
			c.AddDust(new(DustID.Blood, 4));
			c.AddDust(new(DustID.Blood, 15, NPCHitEffects.OnDeath));

			c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_0", 2, NPCHitEffects.OnDeath));
			c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_1", 1, NPCHitEffects.OnDeath));
			c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_2", 1, NPCHitEffects.OnDeath));
		});
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "TheCrimson");
	}
}