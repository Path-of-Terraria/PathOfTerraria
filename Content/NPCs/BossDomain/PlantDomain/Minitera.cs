using NPCUtils;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.PlantDomain;

internal class Minitera : ModNPC
{
	private ref float Timer => ref NPC.ai[0];

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.Plantera);
		NPC.lifeMax = NPC.life = 3000;
		NPC.defense /= 2;
		NPC.aiStyle = -1;

		//NPC.TryEnableComponent<NPCHitEffects>(c =>
		//{
		//	c.AddDust(new(DustID.Blood, 4));
		//	c.AddDust(new(DustID.Blood, 15, NPCHitEffects.OnDeath));

		//	c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_0", 2, NPCHitEffects.OnDeath));
		//	c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_1", 1, NPCHitEffects.OnDeath));
		//	c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_2", 1, NPCHitEffects.OnDeath));
		//});
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Jungle");
	}

	public override void AI()
	{
		NPC.TargetClosest();

		Player player = Main.player[NPC.target];

		NPC.velocity = NPC.DirectionTo(player.Center) * (MathF.Sin(Timer * 0.01f) * 2 + 4);
	}
}
