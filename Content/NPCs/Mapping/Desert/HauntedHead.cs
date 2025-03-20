using NPCUtils;
using PathOfTerraria.Common.NPCs;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert;

//[AutoloadBanner]
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
		NPC.knockBackResist = 0;
		NPC.HitSound = SoundID.NPCHit30;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.hide = true;
		NPC.color = Color.White;

		//NPC.TryEnableComponent<NPCHitEffects>(
		//	c =>
		//	{
		//		c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_0", 1, NPCHitEffects.OnDeath));
		//		c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_1", 2, NPCHitEffects.OnDeath));
		//		c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_2", 1, NPCHitEffects.OnDeath));

		//		c.AddDust(new NPCHitEffects.DustSpawnParameters(DustID.CorruptionThorns, 5));
		//		c.AddDust(new NPCHitEffects.DustSpawnParameters(ModContent.DustType<EntDust>(), 1));
		//		c.AddDust(new NPCHitEffects.DustSpawnParameters(DustID.CorruptionThorns, 20, NPCHitEffects.OnDeath));
		//		c.AddDust(new NPCHitEffects.DustSpawnParameters(ModContent.DustType<EntDust>(), 10, NPCHitEffects.OnDeath));
		//	}
		//);
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
		if (Main.rand.NextBool(2, 5))
		{
			Vector2 dustPos = NPC.BottomRight - new Vector2(4 + Main.rand.NextFloat(NPC.width - 8), 6);
			Vector2 velocity = NPC.velocity * 0.5f + new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), Main.rand.NextFloat(1, 4));
			Dust.NewDustPerfect(dustPos, ModContent.DustType<GhostDust>(), velocity);
		}

		Timer++;

		NPC.TargetClosest();
		NPC.velocity += NPC.DirectionTo(Main.player[NPC.target].Center).RotatedBy(MathF.Sin(Timer * 0.02f) * 0.5f);

		if (NPC.velocity.LengthSquared() > 14 * 14)
		{
			NPC.velocity = Vector2.Normalize(NPC.velocity) * 14;
		}

		NPC.velocity *= 0.99f;
	}

	public override void FindFrame(int frameHeight)
	{
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

public class GhostDust : ModDust
{
	public override void OnSpawn(Dust dust)
	{
		dust.velocity *= 0.4f;
		dust.noGravity = true;
		dust.noLight = true;
		dust.scale *= 1.5f;
		dust.color.A = (byte)Main.rand.Next(20, 230);
		dust.frame = new Rectangle(0, 8 * Main.rand.Next(3), 6, 6);
	}

	public override bool Update(Dust dust)
	{
		dust.position += dust.velocity;
		dust.rotation += dust.velocity.X * 0.15f;
		dust.scale *= 0.98f;

		if (Collision.SolidCollision(dust.position, 6, 6))
		{
			dust.scale *= 0.7f;
		}

		if (dust.scale < 0.3f)
		{
			dust.active = false;
		}

		return false;
	}
}
