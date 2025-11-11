using PathOfTerraria.Common;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.World.Utilities;
using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.Buffs.ElementalBuffs;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.SkillPassives.RainOfArrowsPassives;
using PathOfTerraria.Content.SkillPassives.SwarmPassives;
using PathOfTerraria.Content.SkillSpecials.PestSwarmSpecials;
using PathOfTerraria.Content.SkillTrees;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Skills.Summon;

public class Swarm : Skill
{
	public override int MaxLevel => 3;

	public override SkillTags Tags()
	{
		SkillTags tags = SkillTags.Summon | SkillTags.Projectile;

		if (Tree.Specialization is GlacialAntlions)
		{
			tags |= SkillTags.Cold;
		}

        foreach (SkillNode node in Tree.Nodes)
        {
            if (node is VolatileInsects vola && vola.Allocated)
            {
                tags |= SkillTags.Fire;
            }

            if (node is ShockingEmergence shock && shock.Allocated)
            {
                tags |= SkillTags.Lightning;
            }
        }

        return tags;
    }

	public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = 2 * 60;
		ManaCost = 10 - Level * 3;
		Duration = SentryNPC.DefaultSentryDuration;
		WeaponType = ItemType.Summoner;
	}									

	public override bool CanUseSkill(Player player, ref SkillFailure failReason, bool justChecking = true)
	{
		if (!player.HeldItem.CountsAsClass(DamageClass.Summon))
		{
			failReason = new SkillFailure(SkillFailReason.NeedsSummon);
			return false;
		}

		if (Tree.Specialization is not LocustBrood && Collision.SolidCollision(GetTarget(player) - new Vector2(12, 12), 24, 24))
		{
			failReason = new SkillFailure(SkillFailReason.Other, "Blocked");
			return false;
		}

		return base.CanUseSkill(player, ref failReason, justChecking);
	}

	public override void UseSkill(Player player)
	{
		base.UseSkill(player);

		Vector2 pos = GetTarget(player);

		if (Tree.Specialization is LocustBrood)
		{
			SpawnBrood(player);
		}
		else
		{
			int type = ModContent.ProjectileType<LocustSpawnCircle>();
			int damage = (int)player.GetDamage(DamageClass.Summon).ApplyTo(10 * Level);

			if (Tree.Specialization is AntlionSwarm)
			{
			    damage = (int)player.GetDamage(DamageClass.Summon).ApplyTo(6 * Level);
			    
			    // Spawn two Antlion Swarmers
			    Vector2 offset1 = new Vector2(-24, 0);
			    Vector2 offset2 = new Vector2(24, 0);
			    
			    int proj1 = Projectile.NewProjectile(new EntitySource_UseSkill(player, this), pos + offset1, Vector2.Zero, type, damage, 0, player.whoAmI, TotalDuration);
			    int proj2 = Projectile.NewProjectile(new EntitySource_UseSkill(player, this), pos + offset2, Vector2.Zero, type, damage, 0, player.whoAmI, TotalDuration);
			}
			else
			{
			    int proj = Projectile.NewProjectile(new EntitySource_UseSkill(player, this), pos, Vector2.Zero, type, damage, 0, player.whoAmI, TotalDuration);

			    if (Tree.Specialization is GlacialAntlions)
			    {
				    int bonusDamage = player.HasTreePassive<PestSwarmTree, FrostbiteMandibles>() ? 10 + player.GetPassiveStrength<PestSwarmTree, AggressiveChill>() * 3 : 0;

				    if (bonusDamage > 0)
				    {
					    Main.projectile[proj].damage += bonusDamage;
				    }

					ElementalProjectile elementalProj = Main.projectile[proj].GetGlobalProjectile<ElementalProjectile>();
				    elementalProj.Container[ElementType.Cold].DamageModifier = elementalProj.Container[ElementType.Cold].DamageModifier.AddModifiers(bonusDamage, 1);
			    }
			}
		}
	}

	private void SpawnBrood(Player player)
	{
		const int Range = 14;

		HashSet<Point16> points = [];
		Point16 center = player.Center.ToTileCoordinates16();
		int tries = 500;
		int cap = 10 + player.GetPassiveStrength<PestSwarmTree, BiggerBrood>();
		// Exclusive upper bounds.
		int xMin = Math.Max(0, center.X - Range);
		int yMin = Math.Max(1, center.Y - Range);
		int xMax = Math.Min(Main.maxTilesX, center.X + Range);
		int yMax = Math.Min(Main.maxTilesY, center.Y + Range);

		while (points.Count < cap && --tries > 0)
		{
			Point16 point = new(Main.rand.Next(xMin, xMax), Main.rand.Next(yMin, yMax));
			Tile cur = Main.tile[point];
			Tile above = Main.tile[point.X, point.Y - 1];

			if ((WorldUtils.SolidTile(cur) || TileID.Sets.Platforms[cur.TileType]) && !WorldUtils.SolidTile(above))
			{
				points.Add(point);
			}
		}

		int type = ModContent.ProjectileType<LocustEgg>();
		var src = new EntitySource_UseSkill(player, this);
		int damage = (int)player.GetDamage(DamageClass.Summon).ApplyTo(6 * Level);

		foreach (Point16 point in points)
		{
			float duration = 30 * Main.rand.NextFloat(0.9f, 2.5f) * (1 - player.GetPassiveStrength<PestSwarmTree, QuickerHatching>() * 0.2f);
			Projectile.NewProjectile(src, point.ToWorldCoordinates(8, -22), Vector2.Zero, type, damage, 0, player.whoAmI, duration, 0, TotalDuration);
		}
	}

	private static Vector2 GetTarget(Player player)
	{
		return player.Center + player.DirectionTo(Main.MouseWorld) * MathF.Min(player.Distance(Main.MouseWorld), 120);
	}

	public class LocustSpawnCircle : SkillProjectile<Swarm>
	{
		const float MaxTime = 24;

		private ref float Duration => ref Projectile.ai[0];
		private ref float TimeLeft => ref Projectile.ai[1];

		public override void SetDefaults()
		{
			Projectile.timeLeft = 120;
			Projectile.Opacity = 0;
			Projectile.Size = new Vector2(20);
			Projectile.hide = true;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			overPlayers.Add(index);
		}

		public override bool? CanDamage()
		{
			return false;
		}

		public override void AI()
		{
			if (Projectile.timeLeft == 120)
			{
				Projectile.scale = 0f;
				TimeLeft = MaxTime;
			}

			Projectile.scale += 1 / (MaxTime + 5);

			if (TimeLeft > MaxTime / 2)
			{
				Projectile.Opacity += 1 / (MaxTime / 2f);
			}
			else
			{
				Projectile.Opacity -= 1 / (MaxTime / 2f);
			}

			TimeLeft--;

			if (TimeLeft == (int)MaxTime / 2 && Main.myPlayer == Projectile.owner)
			{
				int type = ModContent.ProjectileType<AntlionSummon>();

				if (Skill.Tree.Specialization is AntlionSwarm)
				{
					type = ModContent.ProjectileType<AntlionSwarmerSummon>();
				}

				var src = new EntitySource_UseSkill(Main.player[Projectile.owner], Skill);
				int proj = Projectile.NewProjectile(src, Projectile.Center, new Vector2(0, -1), type, Projectile.damage, 0, Projectile.owner, 0, 0, Duration);
				Main.projectile[proj].localAI[1] = Skill.Tree.Specialization is GlacialAntlions ? 1 : 0;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			Color color = lightColor * Projectile.Opacity * 0.75f;
			Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, color, 0f, tex.Size() / 2f, Projectile.scale * 1.25f, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, color * 0.33f, 0f, tex.Size() / 2f, Projectile.scale * 1.65f, SpriteEffects.None, 0);
			return true;
		}
	}

	public class SimpleLocust : ModProjectile
	{
		private bool Initialized
		{
			get => Projectile.ai[0] == 1;
			set => Projectile.ai[0] = value ? 1 : 0;
		}

		private int Target
		{
			get => (int)Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}

		private ref float TimeLeft => ref Projectile.ai[2];

		private ref float Timer => ref Projectile.localAI[0];

		public override void SetDefaults()
		{
			Projectile.Size = new(26, 18);
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.minion = true;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
			Projectile.Opacity = 0f;
			Projectile.ArmorPenetration = 999;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override void AI()
		{
			if (!Initialized)
			{
				Initialized = true;
				Target = -1;

				FindTarget();
			}
			else if (Target == -1 || InvalidTarget())
			{
				Target = -1;
				FindTarget();
			}

			if (Target != -1)
			{
				Chase();
			}
			else
			{
				Idle();
			}

			if (TimeLeft-- <= 0)
			{
				Projectile.Kill();
				return;
			}

			Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.05f);
			Projectile.velocity.Y += 0.2f;
			Projectile.rotation = Projectile.velocity.X * 0.05f;

			if (Math.Abs(Projectile.velocity.X) > 0.1f)
			{
				Projectile.spriteDirection = Projectile.direction = Math.Sign(Projectile.velocity.X);
			}
		}

		private bool InvalidTarget()
		{
			NPC npc = Main.npc[Target];
			return !npc.CanBeChasedBy() || npc.DistanceSQ(Projectile.Center) > 600 * 600;
		}

		private void Idle()
		{
			if (Projectile.velocity.Y == 0)
			{
				Timer++;
				Projectile.velocity.X = 0;

				if (Timer > 45)
				{
					Projectile.velocity.X = Main.rand.NextFloat(-1, 1);
					Projectile.velocity.Y = -2;
					Projectile.netUpdate = true;
					
					Timer = 0;
				}
			}
		}

		private void Chase()
		{
			NPC target = Main.npc[Target];
			float targetSpeedX = MathF.Sign(target.Center.X - Projectile.Center.X) * 2.5f;

			if (Projectile.velocity.Y == 0)
			{
				Timer++;
				Projectile.velocity.X = 0;

				if (Timer > 15)
				{
					bool inHitbox = target.Hitbox.Intersects(Projectile.Hitbox);

					Projectile.velocity.X = inHitbox ? 0 : targetSpeedX;
					Projectile.velocity.Y = inHitbox ? -3 : -4;

					Timer = 0;
				}
			}
			else
			{
				Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, targetSpeedX, 0.01f);
			}
		}

		private void FindTarget()
		{
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (!npc.CanBeChasedBy())
				{
					continue;
				}

				float dist = npc.DistanceSQ(Projectile.Center);

				if (dist < 400 * 400 && (Target == -1 || Main.npc[Target].DistanceSQ(Projectile.Center) > dist))
				{
					Target = npc.whoAmI;
					Timer = 0;
				}
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			fallThrough = false;
			return true;
		}

		public override Color? GetAlpha(Color lightColor)
		{
			if (TimeLeft < 60)
			{
				return Color.Lerp(lightColor, Color.Red, MathF.Sin(TimeLeft * 0.2f) * 0.25f + 0.25f);
			}

			return null;
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 17; ++i)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, i < 8 ? DustID.Blood : ModContent.DustType<LocustDust>());
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Player plr = Main.player[Projectile.owner];
			float maxChance = 0.1f + plr.GetPassiveStrength<PestSwarmTree, CarnivorousLarvae>() * 0.02f;

			if (target.life <= 0 && Main.rand.NextFloat() < maxChance && plr.GetModPlayer<SkillCombatPlayer>().TryGetSkill<Swarm>(out Skill swarm))
			{
				float duration = 30 * Main.rand.NextFloat(0.9f, 1.1f) * (1 - plr.GetPassiveStrength<PestSwarmTree, QuickerHatching>() * 0.2f);
				int type = ModContent.ProjectileType<LocustEgg>();
				Projectile.NewProjectile(target.GetSource_Death(), target.Center, Vector2.Zero, type, 15 * swarm.Level, 0, plr.whoAmI, duration, 0, swarm.TotalDuration);
			}
		}
	}

	public class AntlionSwarmerSummon : SkillProjectile<Swarm>
	{
		private bool IsExplosive => Main.player[Projectile.owner].HasTreePassive<PestSwarmTree, VolatileInsects>();

		private bool Initialized
		{
			get => Projectile.ai[0] == 1;
			set => Projectile.ai[0] = value ? 1 : 0;
		}

		private int Target
		{
			get => (int)Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}

		private ref float TimeLeft => ref Projectile.ai[2];

		private ref float Timer => ref Projectile.localAI[0];

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.Size = new(42, 24);
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.minion = true;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
			Projectile.Opacity = 0f;
			Projectile.ArmorPenetration = 999;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override void AI()
		{
			if (!Initialized)
			{
				Initialized = true;
				Target = -1;

				FindTarget();
			}
			else if (Target == -1 || InvalidTarget())
			{
				Target = -1;
				FindTarget();
			}

			if (Target != -1)
			{
				Chase();
			}
			else
			{
				Idle();
			}

			if (TimeLeft-- <= 0)
			{
				Projectile.Kill();
				return;
			}

			Projectile.frame = (int)((Projectile.frameCounter++ * 0.4f) % 2);
			Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.05f);
			Projectile.rotation = Projectile.velocity.X * 0.05f;

			if (Math.Abs(Projectile.velocity.X) > 0.001f)
			{
				Projectile.spriteDirection = Projectile.direction = Math.Sign(Projectile.velocity.X);
			}
		}

		private bool InvalidTarget()
		{
			NPC npc = Main.npc[Target];
			return !npc.CanBeChasedBy() || npc.DistanceSQ(Projectile.Center) > 600 * 600;
		}

		private void Idle()
		{
			Projectile.velocity *= 0.95f;
			Timer++;

			if (Timer > 45 && Main.myPlayer == Projectile.owner)
			{
				Projectile.velocity = Main.rand.NextVector2Circular(2, 2);
				Projectile.netUpdate = true;

				Timer = 0;
			}
		}

		private void Chase()
		{
			NPC target = Main.npc[Target];

			Projectile.velocity += Projectile.DirectionTo(target.Center) * 0.4f;

			if (Projectile.velocity.LengthSquared() > 8 * 8)
			{
				Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 8;
			}
		}

		private void FindTarget()
		{
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (!npc.CanBeChasedBy())
				{
					continue;
				}

				float dist = npc.DistanceSQ(Projectile.Center);

				if (dist < 400 * 400 && (Target == -1 || Main.npc[Target].DistanceSQ(Projectile.Center) > dist))
				{
					Target = npc.whoAmI;
					Timer = 0;
				}
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		public override Color? GetAlpha(Color lightColor)
		{
			if (TimeLeft < 60)
			{
				return Color.Lerp(lightColor, Color.Red, MathF.Sin(TimeLeft * (IsExplosive ? 0.6f : 0.2f)) * 0.25f + 0.25f);
			}

			return null;
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 17; ++i)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, i < 8 ? DustID.Blood : ModContent.DustType<LocustDust>());
			}

			if (IsExplosive)
			{
				int exp = ModContent.ProjectileType<AntlionExplosion>();
				int proj = Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, exp, Projectile.damage * 5, 8, Projectile.owner, 60, 60);
				Main.projectile[proj].GetGlobalProjectile<ElementalProjectile>().Container[ElementType.Fire].DamageModifier.AddModifiers(0, 1f);

				ExplosionHitbox.VFX(Projectile, new ExplosionHitbox.VFXPackage(2, 8, 4, true, 0.4f, null));
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Player plr = Main.player[Projectile.owner];

			if (plr.HasTreePassive<PestSwarmTree, ViciousBites>())
			{
				BleedDebuff.Apply(plr, target, 5 * 60, damageDone);
			}

			if (plr.HasTreePassive<PestSwarmTree, OverheatingBugs>())
			{
				float mod = 1 + plr.GetPassiveStrength<PestSwarmTree, SuperheatedBugs>() * 0.15f;
				IgnitedDebuff.ApplyTo(target, (int)(damageDone / 4f * mod));
			}

			if (plr.HasTreePassive<PestSwarmTree, CarapaceCracker>())
			{
				target.AddBuff(ModContent.BuffType<CarapaceCracker.CrackedCarapaceDebuff>(), 4 * 60);
			}

			if (plr.HasTreePassive<PestSwarmTree, VolatileInsects>())
			{
				TimeLeft = 5;
			}
		}
	}

	public class LocustEgg : SkillProjectile<Swarm>
	{
		private ref float LifeTime => ref Projectile.ai[0];
		private ref float MaxLifeTime => ref Projectile.ai[1];
		private ref float Duration => ref Projectile.ai[2];

		private ref float AttachedNPC => ref Projectile.localAI[0];

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 3;
		}

		public override void SetDefaults()
		{
			Projectile.tileCollide = true;
			Projectile.Size = new(16, 20);
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.scale = 0f;
			Projectile.frame = Main.rand.Next(3);
		}

		public override bool? CanDamage()
		{
			return false;
		}

		public override void AI()
		{
			if (MaxLifeTime <= 5)
			{
				MaxLifeTime = LifeTime;
				AttachedNPC = -1;
				Projectile.Size = new Vector2(16, 20);
			}

			if (AttachedNPC == -1)
			{
				Projectile.velocity.Y += 0.1f;

				foreach (NPC npc in Main.ActiveNPCs)
				{
					if (npc.CanBeChasedBy() && npc.Hitbox.Intersects(Projectile.Hitbox))
					{
						AttachedNPC = npc.whoAmI;
						Projectile.tileCollide = false;
						Projectile.netUpdate = true;
						break;
					}
				}
			}
			else
			{
				Projectile.velocity = Main.npc[(int)AttachedNPC].velocity;
			}

			Projectile.scale = MathF.Min(Projectile.scale + (1 / MaxLifeTime), 1);

			LifeTime--;

			if (LifeTime < 0)
			{
				if (Projectile.alpha == 0)
				{
					PopFunctionality();
				}

				Projectile.alpha += 10;

				if (Projectile.alpha > 250)
				{
					Projectile.Kill();
				}
			}
		}

		private void PopFunctionality()
		{
			Player player = Main.player[Projectile.owner];
			bool has = player.HasTreePassive<PestSwarmTree, Eggsplosion>();

			if (Main.myPlayer == Projectile.owner)
			{
				Vector2 vel = new Vector2(0, -Main.rand.NextFloat(5, 9)).RotatedByRandom(MathHelper.PiOver2 * 0.9f);
				int type = ModContent.ProjectileType<SimpleLocust>();
				Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, vel, type, Projectile.damage, 1f, Projectile.owner, 0, 0, Duration);

				if (has)
				{
					int exp = ModContent.ProjectileType<ExplosionHitboxFriendly>();
					int proj = Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, exp, Projectile.damage, 8, Projectile.owner, 60, 60);
					Main.projectile[proj].GetGlobalProjectile<ElementalProjectile>().Container[ElementType.Fire].DamageModifier.AddModifiers(0, 0.33f);
				}
			}

			if (has)
			{
				ExplosionHitbox.VFX(Projectile, new ExplosionHitbox.VFXPackage(1, 4, 2, true, 0.4f, null));
				Projectile.Kill();

				for (int i = 0; i < 8; ++i)
				{
					Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.MothronEgg);
				}
			}
			else
			{
				SoundEngine.PlaySound(SoundID.Item50 with { Volume = Main.rand.NextFloat(0.1f, 0.25f), PitchRange = (0.7f, 1f) }, Projectile.Center);

				for (int i = 0; i < 3; ++i)
				{
					Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.MothronEgg, 0, -6);
				}
			}

			if (player.HasTreePassive<PestSwarmTree, ShockingEmergence>())
			{
				foreach (NPC npc in Main.ActiveNPCs)
				{
					if (npc.CanBeChasedBy() && npc.DistanceSQ(Projectile.Center) < 80 * 80)
					{
						npc.AddBuff(ModContent.BuffType<ShockDebuff>(), 5 * 60);
					}
				}

				for (int i = 0; i < 15; ++i)
				{
					Dust.NewDust(Projectile.Center + new Vector2(Main.rand.NextFloat(-80, 80), Main.rand.NextFloat(-80, 80)), 1, 1, DustID.Electric, 0, 0);
				}
			}
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write((short)AttachedNPC);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			AttachedNPC = reader.ReadInt16();
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			fallThrough = false;
			return true;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.height / 2);
			int frameWidth = tex.Width / 3;
			int frameHeight = tex.Height / Main.projFrames[Type];
			Rectangle frame = new(frameWidth * (int)(Projectile.alpha / 90f), Projectile.frame * frameHeight, frameWidth, frameHeight);
			var scale = new Vector2(2 - Projectile.scale, Projectile.scale);
			Color color = Color.Lerp(lightColor, Color.White, 0.3f) * Projectile.Opacity;
			
			Main.spriteBatch.Draw(tex, position, frame, color, Projectile.rotation, frame.Size() * new Vector2(0.5f, 1), scale, SpriteEffects.None, 0);
			return false;
		}
	}

	public class AntlionSummon : SkillProjectile<Swarm>
	{
		private static Asset<Texture2D> GlacierTexture = null;

		private bool Initialized
		{
			get => Projectile.ai[0] == 1;
			set => Projectile.ai[0] = value ? 1 : 0;
		}

		private int Target
		{
			get => (int)Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}

		private ref float TimeLeft => ref Projectile.ai[2];

		private ref float Timer => ref Projectile.localAI[0];

		private bool Glacier => Projectile.localAI[1] == 1;

		public override void SetStaticDefaults()
		{
			GlacierTexture = ModContent.Request<Texture2D>(Texture.Replace("AntlionSummon", "GlacialAntlionSummon"));
		}

		public override void SetDefaults()
		{
			Projectile.Size = new(26, 18);
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.minion = true;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
			Projectile.Opacity = 0f;
			Projectile.ArmorPenetration = 999; 
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override void AI()
		{
			if (!Initialized)
			{
				Initialized = true;
				Target = -1;

				FindTarget();

				Projectile.netUpdate = true;
			}
			else if (Target == -1 || InvalidTarget())
			{
				Target = -1;
				FindTarget();
			}

			if (Target != -1)
			{
				Chase();
			}
			else
			{
				Idle();
			}

			Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);

			if (TimeLeft-- <= 0)
			{
				Projectile.Kill();
				return;
			}

			Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.05f);
			Projectile.velocity.Y += 0.2f;
			Projectile.rotation = Projectile.velocity.X * 0.05f;

			if (Math.Abs(Projectile.velocity.X) > 0.1f)
			{
				Projectile.spriteDirection = Projectile.direction = Math.Sign(Projectile.velocity.X);
			}
		}

		private bool InvalidTarget()
		{
			NPC npc = Main.npc[Target];
			return !npc.CanBeChasedBy() || npc.DistanceSQ(Projectile.Center) > 600 * 600;
		}

		private void Idle()
		{
			if (Projectile.velocity.Y == 0)
			{
				Timer++;
				Projectile.velocity.X *= 0.95f;

				if (Timer > 45 && Main.myPlayer == Projectile.owner)
				{
					Projectile.velocity.X = Main.rand.NextFloat(2, 3) * (Main.rand.NextBool() ? -1 : 1);
					Projectile.netUpdate = true;

					Timer = 0;
				}
			}
		}

		private void Chase()
		{
			NPC target = Main.npc[Target];
			float targetSpeedX = MathF.Sign(target.Center.X - Projectile.Center.X) * 5f;
			Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, targetSpeedX, 0.035f);
		}

		private void FindTarget()
		{
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (!npc.CanBeChasedBy())
				{
					continue;
				}

				float dist = npc.DistanceSQ(Projectile.Center);

				if (dist < 400 * 400 && (Target == -1 || Main.npc[Target].DistanceSQ(Projectile.Center) > dist))
				{
					Target = npc.whoAmI;
					Timer = 0;
				}
			}
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(Timer);
			writer.Write(Glacier);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			Timer = reader.ReadSingle();
			Projectile.localAI[1] = reader.ReadBoolean() ? 1 : 0;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			fallThrough = false;
			return true;
		}

		public override Color? GetAlpha(Color lightColor)
		{
			if (TimeLeft < 60)
			{
				return Color.Lerp(lightColor, Glacier ? Color.MediumBlue : Color.Red, MathF.Sin(TimeLeft * 0.2f) * 0.25f + 0.25f);
			}

			return null;
		}

		public override void OnKill(int timeLeft)
		{
			int mainDust = Glacier ? DustID.Ice : ModContent.DustType<LocustDust>();

			for (int i = 0; i < 17; ++i)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, i < 8 ? DustID.Blood : mainDust);
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Player plr = Main.player[Projectile.owner];
			float maxChance = 0.1f + plr.GetPassiveStrength<PestSwarmTree, CarnivorousLarvae>() * 0.02f;

			if (target.life <= 0)
			{
				if (Main.rand.NextFloat() < maxChance && plr.HasTreePassive<PestSwarmTree, Gestation>())
				{
					float duration = 30 * Main.rand.NextFloat(0.9f, 1.1f) * (1 - plr.GetPassiveStrength<PestSwarmTree, QuickerHatching>() * 0.2f);
					int type = ModContent.ProjectileType<LocustEgg>();
					Projectile.NewProjectile(target.GetSource_Death(), target.Center, Vector2.Zero, type, 15 * Skill.Level, 0, plr.whoAmI, duration, 0, Skill.TotalDuration);
				}

				if (Glacier)
				{
					if (plr.HasTreePassive<PestSwarmTree, ColdBlooded>())
					{
						TimeLeft += 60 + (30 * plr.GetPassiveStrength<PestSwarmTree, ThermalConversion>());
					}

					if (plr.HasTreePassive<PestSwarmTree, ShatteringCarapace>())
					{
						int type = ModContent.ProjectileType<IceShards>();
						Vector2 pos = target.Top;

						while (Collision.SolidCollision(pos - new Vector2(10), 20, 20))
						{
							pos.Y -= 4;
						}

						Projectile.NewProjectile(target.GetSource_Death(), pos, target.velocity, type, target.lifeMax / 5, 0, plr.whoAmI);
					}
				}
			}

			if (plr.HasTreePassive<PestSwarmTree, FrostbiteMandibles>() && Main.rand.NextFloat() < 0.1f + plr.GetPassiveStrength<PestSwarmTree, IceVenom>() * 0.1f)
			{
				target.AddBuff(BuffID.Chilled, 3 * 60);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = Glacier ? GlacierTexture.Value : TextureAssets.Projectile[Type].Value;
			SpriteEffects effect = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY);
			
			Main.EntitySpriteDraw(tex, position, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, 1f, effect, 0);
			return false;
		}
	}
}

public class LocustDust : ModDust
{
	public override void OnSpawn(Dust dust)
	{
		dust.alpha = 0;
	}

	public override Color? GetAlpha(Dust dust, Color lightColor)
	{
		return lightColor;
	}
}