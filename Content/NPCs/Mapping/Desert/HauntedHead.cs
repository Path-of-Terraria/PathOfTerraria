using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert;

[AutoloadBanner]
internal class HauntedHead : ModNPC
{
	public enum AIState
	{
		Chase,
		SwingProj,
		Slam
	}

	public Player Target => Main.player[NPC.target];

	private ref float Timer => ref NPC.ai[0];

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 1;

		var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
		{
			Velocity = 2f
		};
		NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
	}

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(34, 30);
		NPC.aiStyle = -1;
		NPC.lifeMax = 300;
		NPC.defense = 0;
		NPC.damage = 50;
		NPC.HitSound = SoundID.NPCHit30;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.hide = true;
		NPC.color = Color.White;
		NPC.knockBackResist = 1.8f;

		NPC.TryEnableComponent<NPCHitEffects>(
			c =>
			{
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/MummyPaper_0", 2, NPCHitEffects.OnDeath));
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/MummyPaper_1", 2, NPCHitEffects.OnDeath));
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/MummyPaper_2", 2, NPCHitEffects.OnDeath));

				c.AddDust(new NPCHitEffects.DustSpawnParameters(ModContent.DustType<GhostDust>(), 5));
				c.AddDust(new NPCHitEffects.DustSpawnParameters(ModContent.DustType<GhostDust>(), 20, NPCHitEffects.OnDeath));
			}
		);
	}

	public override void DrawBehind(int index)
	{
		Main.instance.DrawCacheNPCsMoonMoon.Add(index);
	}

	public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
	{
		NPC.lifeMax = ModeUtils.ByMode(250, 300, 400);
		NPC.damage = ModeUtils.ByMode(50, 80, 120);
		NPC.defense = ModeUtils.ByMode(0, 0, 10, 20);
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Desert");
	}

	public override void AI()
	{
		const float MaxSpeed = 12;

		if (Main.rand.NextBool(2, 5))
		{
			Vector2 dustPos = NPC.BottomRight - new Vector2(4 + Main.rand.NextFloat(NPC.width - 8), 6);
			Vector2 velocity = NPC.velocity * 0.5f + new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), Main.rand.NextFloat(1, 4));
			Dust.NewDustPerfect(dustPos, ModContent.DustType<GhostDust>(), velocity);
		}

		Timer++;

		NPC.TargetClosest();
		NPC.spriteDirection = Math.Sign(NPC.velocity.X);
		NPC.velocity += NPC.DirectionTo(Main.player[NPC.target].Center).RotatedBy(MathF.Sin(Timer * 0.02f) * 0.5f) * 0.35f;

		if (NPC.velocity.LengthSquared() > MaxSpeed * MaxSpeed)
		{
			NPC.velocity = Vector2.Normalize(NPC.velocity) * MaxSpeed;
		}

		NPC.velocity *= 0.98f;
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Texture2D tex = TextureAssets.Npc[Type].Value;
		SpriteEffects effect = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		Rectangle frame = NPC.frame with { Width = 34 };
		Color color = NPC.GetAlpha(Lighting.GetColor(NPC.Center.ToTileCoordinates(), NPC.color));
		spriteBatch.Draw(tex, NPC.Center - screenPos, frame, color, 0f, frame.Size() / 2f, 1f, effect, 0);
		spriteBatch.Draw(tex, NPC.Center - screenPos, frame with { X = 36 }, color, 0f, frame.Size() / 2f, 1f, effect, 0);

		return false;
	}
}
