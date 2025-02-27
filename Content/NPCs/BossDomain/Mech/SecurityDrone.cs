using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.NPCs.Pathfinding;
using PathOfTerraria.Content.Scenes;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.Mech;

[AutoloadBanner]
internal class SecurityDrone : ModNPC
{
	private static Asset<Texture2D> Glow = null;

	private ref float Timer => ref NPC.ai[0];

	private readonly Pathfinder _pathfinder = new(5);

	public override void SetStaticDefaults()
	{
		Glow = ModContent.Request<Texture2D>(Texture + "_Glow");
	}

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.Slimer);
		NPC.Opacity = 1;
		NPC.Size = new Vector2(36);
		NPC.aiStyle = -1;
		NPC.lifeMax = 200;
		NPC.defense = 40;
		NPC.damage = 50;
		NPC.HitSound = SoundID.NPCHit4;
		NPC.noGravity = true;
		NPC.knockBackResist = 0;
		NPC.scale = 1;
		NPC.noTileCollide = true;

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddDust(new(DustID.MinecartSpark, 6, null, static x => x.scale = 10));
			c.AddDust(new(DustID.MinecartSpark, 20, NPCHitEffects.OnDeath, static x => x.scale = 10));

			c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_0", 1, NPCHitEffects.OnDeath));
			c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_1", 1, NPCHitEffects.OnDeath));
			c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_2", 1, NPCHitEffects.OnDeath));
			c.AddGore(new NPCHitEffects.GoreSpawnParameters(GoreID.Smoke1, 1, NPCHitEffects.OnDeath));
			c.AddGore(new NPCHitEffects.GoreSpawnParameters(GoreID.Smoke2, 1, NPCHitEffects.OnDeath));
			c.AddGore(new NPCHitEffects.GoreSpawnParameters(GoreID.Smoke3, 1, NPCHitEffects.OnDeath));
		});

		SpawnModBiomes = [ModContent.GetInstance<MechBiome>().Type];
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "");
	}

	public override bool? CanFallThroughPlatforms()
	{
		return true;
	}

	public override void AI()
	{
		NPC.TargetClosest(false);
		NPC.rotation = MathHelper.Lerp(NPC.rotation, NPC.velocity.X * 0.1f, 0.2f);

		Player target = Main.player[NPC.target];
		_pathfinder.CheckDrawPath(NPC.Center.ToTileCoordinates16(), target.Center.ToTileCoordinates16(), 
			new Vector2(NPC.width / 16f, NPC.height / 16f - 1f), null, new(-NPC.width / 2, -10));

		bool canPath = _pathfinder.HasPath && _pathfinder.Path.Count > 0;

		if (canPath)
		{
			List<Pathfinder.FoundPoint> checkPoints = _pathfinder.Path[^(Math.Min(_pathfinder.Path.Count, 6))..];
			Vector2 direction = -Vector2.Normalize(AveragePathDirection(checkPoints)) * 5;
			NPC.velocity = direction;
		}

		if (Collision.CanHit(NPC, target))
		{
			Timer++;

			if (Timer > 480)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					int damage = ModeUtils.ProjectileDamage(30, 45, 80);
					Vector2 vel = NPC.DirectionTo(target.Center) * 8;

					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + vel * 7, vel, ProjectileID.PinkLaser, damage, 0, Main.myPlayer);
				}

				Timer = 0;
			}
			else if (Timer > 400)
			{
				Vector2 vel = NPC.DirectionTo(target.Center) * 28;
				var dust = Dust.NewDustPerfect(NPC.Center, DustID.RedMoss, vel);
				dust.noGravity = true;
			}
		}
	}

	private static Vector2 AveragePathDirection(List<Pathfinder.FoundPoint> foundPoints)
	{
		Vector2 dir = Vector2.Zero;

		foreach (Pathfinder.FoundPoint point in foundPoints)
		{
			dir += Pathfinder.ToVector2(point.Direction);
		}

		return dir / foundPoints.Count;
	}

	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Vector2 position = NPC.Center - screenPos + new Vector2(0, 6);
		spriteBatch.Draw(Glow.Value, position, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / 2f, 1f, SpriteEffects.None, 0);
	}
}
