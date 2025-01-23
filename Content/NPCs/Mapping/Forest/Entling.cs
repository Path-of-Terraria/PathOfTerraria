using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Content.Buffs;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Forest;

[AutoloadBanner]
internal class Entling : ModNPC
{
	private readonly static Dictionary<int, Asset<Texture2D>> GlowsById = [];

	protected virtual int FallFrame => 1;

	public Player Target => Main.player[NPC.target];

	private bool LastOnGround
	{
		get => NPC.ai[0] == 1f;
		set => NPC.ai[0] = value ? 1f : 0f;
	}

	public override void SetStaticDefaults()
	{
		GlowsById.Add(Type, ModContent.Request<Texture2D>(Texture + "_Glow"));
		Main.npcFrameCount[Type] = 7;

		var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
		{
			Velocity = 2f
		};
		NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
	}

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(26, 32);
		NPC.aiStyle = -1;
		NPC.lifeMax = 110;
		NPC.defense = 10;
		NPC.damage = 35;
		NPC.scale = Main.rand.NextFloat(0.8f, 1.2f);

		if (!Main.dedServ)
		{
			NPC.HitSound = new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/GrassHit") with { Volume = 0.6f, PitchRange = (-0.3f, 0.3f) };
		}

		NPC.TryEnableComponent<NPCHitEffects>(
			c =>
			{
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_0", 1, NPCHitEffects.OnDeath));
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_1", 1, NPCHitEffects.OnDeath));
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_2", 2, NPCHitEffects.OnDeath));

				c.AddDust(new NPCHitEffects.DustSpawnParameters(DustID.CorruptionThorns, 3));
				c.AddDust(new NPCHitEffects.DustSpawnParameters(DustID.CorruptionThorns, 12, NPCHitEffects.OnDeath));
				c.AddDust(new NPCHitEffects.DustSpawnParameters(ModContent.DustType<EntDust>(), 2, NPCHitEffects.OnDeath));
			}
		);
	}

	public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
	{
		NPC.life = ModeUtils.ByMode(225, 350, 500);
		NPC.damage = ModeUtils.ByMode(5, 7, 10);
		NPC.knockBackResist = ModeUtils.ByMode(2, 1.94f, 1.9f) - NPC.scale;
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Surface");
	}

	public override void AI()
	{
		NPC.TargetClosest(true);
		NPC.spriteDirection = -NPC.direction;

		float targetXVel = Target.Center.X > NPC.Center.X ? 4 : -4;

		if (NPC.HasBuff(BuffID.Confused))
		{
			targetXVel *= -0.8f;
		}

		targetXVel *= 2 - NPC.scale;
		NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, targetXVel, 0.03f);

		if (!LastOnGround && NPC.velocity.Y == 0 && NPC.oldVelocity.Y > 3)
		{
			for (int i = 0; i < 2; ++i)
			{
				Dust.NewDust(NPC.BottomLeft, NPC.width, 4, DustID.CorruptionThorns, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(3, 6));
			}
		}

		LastOnGround = NPC.velocity.Y == 0;

		if (ShouldJump())
		{
			NPC.velocity.Y = -8;
			SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/MiscJump") with { Volume = 0.2f, PitchRange = (0.1f, 0.5f) });

			for (int i = 0; i < 2; ++i)
			{
				Dust.NewDust(NPC.BottomLeft, NPC.width, 4, DustID.CorruptionThorns, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-6, -3));
			}
		}

		Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
	}

	private bool ShouldJump()
	{
		if (Collision.SolidCollision(NPC.position - new Vector2(0, 16), NPC.width, 16))
		{
			return false;
		}

		int dir = NPC.Center.X < Target.Center.X ? 1 : -1;
		bool wall = dir == -1 ? Collision.SolidCollision(NPC.TopLeft - new Vector2(6, 0), 6, NPC.height - 18) : Collision.SolidCollision(NPC.TopRight, 6, NPC.height - 18);
		bool gap = dir == -1 ? !Collision.SolidCollision(NPC.BottomLeft - new Vector2(16, 0), 14, 14) : !Collision.SolidCollision(NPC.BottomRight + new Vector2(2, 0), 14, 14);
		bool underPlayer = Math.Abs(NPC.Center.X - Target.Center.X) < 40 && NPC.Center.Y > Target.Center.Y;

		if (underPlayer)
		{
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.whoAmI != NPC.whoAmI && npc.ModNPC is Entling && Math.Abs(npc.Hitbox.Y - NPC.Hitbox.Y) < 4 && NPC.Hitbox.Y < npc.Hitbox.Y && npc.Hitbox.Intersects(NPC.Hitbox))
				{
					npc.velocity.Y = 2;
					return true;
				}
			}
		}

		return (wall || gap || underPlayer) && NPC.velocity.Y == 0;
	}

	public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
	{
		target.AddBuff(ModContent.BuffType<RootedDebuff>(), 20);
	}

	public override void FindFrame(int frameHeight)
	{
		if (NPC.velocity.Y == 0 || NPC.IsABestiaryIconDummy)
		{
			NPC.frameCounter += Math.Abs(NPC.velocity.X) / 4f;
			NPC.frame.Y = frameHeight * (int)(NPC.frameCounter / 4f % (Main.npcFrameCount[Type] - 1)) + frameHeight;
		}
		else
		{
			NPC.frameCounter = 0;
			NPC.frame.Y = frameHeight * FallFrame;
		}
	}

	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		SpriteEffects effect = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
		Vector2 position = NPC.Center - screenPos - new Vector2(0, -NPC.gfxOffY);
		Main.EntitySpriteDraw(GlowsById[Type].Value, position, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / 2f, 1f, effect);
	}
}

internal class EntlingAlt : Entling
{
	protected override int FallFrame => 4;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		Main.npcFrameCount[Type] = 8;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		NPC.ModNPC.Banner = ModContent.NPCType<Entling>();
		NPC.ModNPC.BannerItem = NPC.ModNPC.Mod.Find<ModItem>("EntlingBannerItem").Type;
	}
}

internal class ClumsyEntling : Entling
{
	protected override int FallFrame => 15;

	private bool Tripped
	{
		get => NPC.ai[1] == 1f;
		set => NPC.ai[1] = value ? 1f : 0f;
	}

	private ref float Timer => ref NPC.ai[2];
	private ref float TripTimer => ref NPC.ai[3];

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		Main.npcFrameCount[Type] = 16;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		NPC.ModNPC.Banner = ModContent.NPCType<Entling>();
		NPC.ModNPC.BannerItem = NPC.ModNPC.Mod.Find<ModItem>("EntlingBannerItem").Type;
	}

	public override bool PreAI()
	{
		if (Tripped)
		{
			NPC.velocity.X *= 0.86f;

			if (NPC.frameCounter > 50)
			{
				Tripped = false;
				TripTimer = Main.rand.Next(100, 500);
				NPC.frameCounter = 0;
				NPC.netUpdate = true;
			}

			if (NPC.frameCounter == 10 && TripTimer == 0)
			{
				for (int i = 0; i < 12; ++i)
				{
					Dust.NewDust(NPC.BottomLeft, NPC.width, 4, DustID.CorruptionThorns, Main.rand.NextFloat(-4, 4), Main.rand.NextFloat(-8, -5));
				}

				SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/GrassHit") with { Volume = 0.6f, PitchRange = (-0.3f, 0.3f) });
			}
		}
		else
		{
			if (Timer++ > TripTimer && NPC.velocity.Y == 0)
			{
				Tripped = true;
				NPC.frameCounter = 0;
				Timer = 0;
				TripTimer = 0;
			}
		}

		return !Tripped;
	}

	public override void FindFrame(int frameHeight)
	{
		if (Tripped && !NPC.IsABestiaryIconDummy)
		{
			if (NPC.frameCounter == 21)
			{
				TripTimer++;

				if (TripTimer > 15)
				{
					NPC.frameCounter++;
				}
			}
			else
			{
				NPC.frameCounter++;
			}

			NPC.frame.Y = frameHeight * (8 + (int)(NPC.frameCounter / 7));
		}
		else
		{
			if (NPC.velocity.Y == 0 || NPC.IsABestiaryIconDummy)
			{
				NPC.frameCounter += Math.Abs(NPC.velocity.X) / 4f;
				NPC.frame.Y = frameHeight * (int)(NPC.frameCounter / 4f % 7) + frameHeight;
			}
			else
			{
				NPC.frameCounter = 0;
				NPC.frame.Y = frameHeight * FallFrame;
			}
		}
	}
}