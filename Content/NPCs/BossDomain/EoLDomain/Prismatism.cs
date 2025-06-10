using NPCUtils;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using ReLogic.Content;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.EoLDomain;

internal class Prismatism : ModNPC
{
	private static Asset<Texture2D> CapeTex = null;
	private static Asset<Texture2D> ShineTex = null;

	private Player Target => Main.player[NPC.target];

	private ref float Timer => ref NPC.ai[0];

	public override void SetStaticDefaults()
	{
		CapeTex = ModContent.Request<Texture2D>(Texture + "_Cape");
		ShineTex = ModContent.Request<Texture2D>(Texture + "_Shine");
	}

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(42);
		NPC.damage = 0;
		NPC.aiStyle = -1;
		NPC.defense = 50;
		NPC.lifeMax = 1500;
		NPC.noGravity = true;
		NPC.knockBackResist = 0f;

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddDust(new(DustID.Pixie, 4));
			c.AddDust(new(DustID.Pixie, 16, NPCHitEffects.OnDeath));

			////for (int i = 0; i < 4; ++i) 
			////{ 
			////	c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_" + i, 1, NPCHitEffects.OnDeath));
			////}
		});
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "TheHallow");
	}

	public override void AI()
	{
		NPC.TargetClosest(false);

		NPC.rotation += 0.08f;
	}

	public override void FindFrame(int frameHeight)
	{
		NPC.frameCounter++;
	}

	public override Color? GetAlpha(Color drawColor)
	{
		return NPC.color == Color.Transparent ? Color.White : NPC.color;
	}

	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		float rot = NPC.velocity.X * 0.2f;
		Vector2 basePosition = NPC.Center - screenPos;
		Main.EntitySpriteDraw(ShineTex.Value, basePosition + new Vector2(0, 6), null, NPC.GetAlpha(Color.White), rot, ShineTex.Size() / 2f, 1f, SpriteEffects.None, 0);

		Vector2 position = basePosition + new Vector2(0, 14);
		Main.EntitySpriteDraw(CapeTex.Value, position, null, NPC.GetAlpha(Color.White), rot, CapeTex.Size() / 2f, 1f, SpriteEffects.None, 0);
	}
}
