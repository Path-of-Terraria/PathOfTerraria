using PathOfTerraria.Common;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.Buffs.ElementalBuffs;
using PathOfTerraria.Content.Projectiles.PassiveProjectiles;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.SkillPassives.FireballPassives;
using PathOfTerraria.Content.SkillSpecials.FireballSpecials;
using PathOfTerraria.Content.SkillTrees;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Skills.Magic;

public class Fireball : Skill
{
	public override int MaxLevel => 3;

	public override SkillFunctionalityInfo Functionality => Tree.Specialization is FrostfireMeteor ? new SkillFunctionalityInfo(true, true, SkillCost.ManaDrainPerSecond) 
		: base.Functionality;
	
	public enum FireballType : byte
	{
		Normal,
		Inferno,
		Shadowflame,
		Frostfire,
	}

    public override SkillTags Tags()
    {
	    SkillTags tags = SkillTags.Magic | SkillTags.Projectile | SkillTags.Fire | SkillTags.AreaOfEffect;
        
        // Add additional tags based on specialization
        if (Tree.Specialization is Inferno)
        {
	        tags |= SkillTags.AreaOfEffect;
        }
        else if (Tree.Specialization is ShadowflamePyre)
        {
	        tags |= SkillTags.Chaos;
        }
        else if (Tree.Specialization is FrostfireMeteor)
        {
	        tags |= SkillTags.Cold;
        }
        
        return tags;
    }
    
    private static FireballType GetFireballType(Fireball fireball)
    {
	    SkillSpecial special = fireball.Tree.Specialization;

	    return special switch
	    {
		    Inferno => FireballType.Inferno,
		    ShadowflamePyre => FireballType.Shadowflame,
		    FrostfireMeteor => FireballType.Frostfire,
		    _ => FireballType.Normal
	    };
    }

    public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = 8 * 60;
		ResourceCost = 20 + 10 * Level;
		Duration = 0;
		WeaponType = ItemType.None;
	}

	protected override void InternalUseSkill(Player player)
	{
		if (player.HasSkillSpecialization<Fireball, FrostfireMeteor>())
		{
			return;
		}

		ResourceCost = 20 + 10 * Level;

		int damage = GetTotalDamage((Level - 1) * 20 + 30);
		var source = new EntitySource_UseSkill(player, this);
		float knockback = 2f;
		int type = ModContent.ProjectileType<FireballProj>();
		Vector2 velocity = player.DirectionTo(Main.MouseWorld).RotatedByRandom(0.05f) * 8 * Main.rand.NextFloat(0.9f, 1.1f);
		
		int proj = Projectile.NewProjectile(source, player.Center - new Vector2(0, 12), velocity, type, damage, knockback, player.whoAmI, Level, (float)GetFireballType(this));
		Main.projectile[proj].GetGlobalProjectile<ElementalProjectile>().AddElementalValues((ElementType.Fire, 0, 1));
		SoundEngine.PlaySound(SoundID.Item20 with { PitchRange = (-0.8f, 0.2f) }, player.Center);
	}

	public override void ActiveUse(Player player, ref float drainTimer, float staticTimer)
	{
		ResourceCost = 50;

		if (player.HasSkillSpecialization<Fireball, FrostfireMeteor>() && drainTimer % 60 <= 20 && drainTimer % 4 == 0)
		{
			int type = ModContent.ProjectileType<FrostfireMeteorProjectile>();
			Vector2 pos = new(Main.MouseWorld.X - Main.rand.NextFloat(-200, 200), Main.screenPosition.Y - 100);
			Vector2 vel = pos.DirectionTo(Main.MouseWorld).RotatedByRandom(0.05f) * Main.rand.NextFloat(8, 10);
			int proj = Projectile.NewProjectile(new EntitySource_UseSkill(player, this), pos, vel, type, GetTotalDamage((Level - 1) * 20 + 30), 4, player.whoAmI);
			Main.projectile[proj].GetGlobalProjectile<ElementalProjectile>().AddElementalValues((ElementType.Fire, 0, 1), (ElementType.Cold, 0, 1));
		}

		if (player.HasTreePassive<FireballTree, EverburningFrost>())
		{
			if (drainTimer > 60 - staticTimer / 30)
			{
				drainTimer = 60;
			}
		}

		if (player.HasTreePassive<FireballTree, ColdFocus>(out float strength))
		{
			player.moveSpeed *= 1 + strength * 0.6f;
		}
	}

	protected override void ModifyCooldown(Player player, ref int cooldown)
	{
		if (player.HasSkillSpecialization<Fireball, FrostfireMeteor>())
		{
			cooldown /= 4;
		}
	}

	private class FireballProj : SkillProjectile<Fireball>
	{
		public override string Texture => $"{PoTMod.ModName}/Assets/Skills/" + GetType().Name;

		private Player Owner => Main.player[Projectile.owner];
		private int DustType => Owner.HasSkillSpecialization<Fireball, ShadowflamePyre>() ? DustID.Shadowflame : DustID.Torch;

		private ref float Level => ref Projectile.ai[0];

		private FireballType FireballType => (FireballType)Projectile.ai[1];

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 5;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.timeLeft = 240;
			Projectile.penetrate = 1;
			Projectile.aiStyle = -1;
			Projectile.scale = 0f;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.frame = (int)(Projectile.frameCounter++ / 5f) % 5;
			Projectile.Opacity = (Projectile.velocity.Length() - 2) / 8f * 0.25f + 0.75f;
			Projectile.scale = Math.Min(1, Projectile.scale + 0.025f);
			Projectile.width = (int)(36 * Projectile.scale);
			Projectile.height = (int)(36 * Projectile.scale);

			bool hasShadow = Owner.HasSkillSpecialization<Fireball, ShadowflamePyre>();

			if (Projectile.scale == 1)
			{
				Projectile.velocity.Y += hasShadow ? 0.09f : 0.01f;
			}

			if (hasShadow)
			{
				Projectile.timeLeft++;
			}

			SpawnDust(1, 0.4f);
		}

		private void SpawnDust(int count, float strength = 1f)
		{
			for (int i = 0; i < count; ++i)
			{
				float scale = Main.rand.NextFloat(0.8f, 1.4f);
				Vector2 vel = Projectile.velocity * strength;
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType, vel.X, vel.Y, Scale: scale);
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			float chance = FireballType == FireballType.Inferno ? 1 : 0.05f + Level * 0.1f;

			if (Main.rand.NextFloat() < chance)
			{
				if (Owner.HasTreePassive<FireballTree, Pyroclasm>())
				{
					damageDone = (int)(damageDone * 1.25f);
				}

				if (Owner.HasTreePassive<FireballTree, FireballNova>() && target.HasBuff<IgnitedDebuff>())
				{
					int projType = ModContent.ProjectileType<Nova.NovaProjectile>();
					IEntitySource source = Projectile.GetSource_OnHit(target);
					int proj = Projectile.NewProjectile(source, Projectile.Center, Vector2.Zero, projType, damageDone, 6, Projectile.owner, (int)Nova.NovaType.Fire);
					Main.projectile[proj].scale = 0.6f;
					Main.projectile[proj].GetGlobalProjectile<ElementalProjectile>().AddElementalValues((ElementType.Fire, 0, 1));
				}

				IgnitedDebuff.ApplyTo(target, damageDone, Owner.HasTreePassive<FireballTree, FireballNova>() ? 6 * 60 : 4 * 60);
				SpawnDust(12);
			}

			if (Owner.HasTreePassive<FireballTree, SmolderingFury>() && target.life <= 0)
			{
				int explosionDamage = (int)(damageDone * 1.5f);
				float sizeBuff = 5f;

				if (Owner.HasTreePassive<FireballTree, StrongerSmolderingFury>())
				{
					explosionDamage = (int)(explosionDamage * 1.5f);
				}

				if (Owner.HasTreePassive<FireballTree, LargerSmolderingFury>())
				{
					sizeBuff *= 1.5f;
				}

				int buffType = Owner.HasTreePassive<FireballTree, SlowingSmolderingFury>() ? ModContent.BuffType<SlowburnDebuff>() : 0;
				ExplosionHitbox.VFXPackage vfx = new(4, TorchDustType: DustType);
				ExplosionHitbox.QuickSpawn(target.GetSource_Death(), target, explosionDamage, Projectile.owner, target.Size * sizeBuff, vfx, buffType: buffType, buffLength: 3 * 60);
			}
		}

		public override void OnKill(int timeLeft)
		{
			if (!Main.dedServ)
			{
				SpawnDust(20);
				SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Variants = [1, 2], PitchRange = (0.2f, 0.6f) }, Projectile.Center);

				for (int i = 0; i < 30; ++i)
				{
					float scale = Main.rand.NextFloat(1.2f, 2f);
					Vector2 vel = Projectile.velocity + Main.rand.NextVector2CircularEdge(8, 8) * Main.rand.NextFloat(0.3f, 1f);
					Vector2 pos = Projectile.position + new Vector2(Main.rand.NextFloat(Projectile.width), Main.rand.NextFloat(Projectile.height));
					Dust.NewDustPerfect(pos, DustType, vel, Scale: scale);
				}

				for (int i = 0; i < 8; ++i)
				{
					Vector2 vel = Projectile.velocity + Main.rand.NextVector2CircularEdge(4, 4) * Main.rand.NextFloat(0.3f, 1f);
					Gore.NewGorePerfect(Projectile.GetSource_Death(), Projectile.Center, vel, GoreID.Smoke1 + Main.rand.Next(3));
				}
			}

			int area = Skill.GetTotalAreaOfEffect(160);
			Projectile.Resize(area, area);
			Projectile.Damage();

			if (Owner.HasTreePassive<FireballTree, ScorchedEarth>())
			{
				int type = ModContent.ProjectileType<StickyFlame>();
				int damage = Projectile.damage / (Owner.HasTreePassive<FireballTree, StrongerScorchedEarth>() ? 2 : 4);

				for (int i = 0; i < 8; ++i)
				{
					Vector2 vel = Main.rand.NextVector2Circular(4, 4) + Projectile.velocity * 0.25f;
					float timeExtension = Owner.GetPassiveStrength<FireballTree, LongerScorchedEarth>() is not 0 and int value ? 60 * value : 0;
					int proj = Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, vel, type, damage, 0, Main.myPlayer, 0, timeExtension);
					Main.projectile[proj].frameCounter = Main.rand.Next(800);
					Main.projectile[proj].GetGlobalProjectile<ElementalProjectile>().AddElementalValues((ElementType.Fire, 0, 1));
				}
			}

			if (Owner.HasSkillSpecialization<Fireball, ShadowflamePyre>())
			{
				int type = ModContent.ProjectileType<ShadowflamePyreProjectile>();
				float timeExtension = Owner.GetPassiveStrength<FireballTree, EverburningPyre>() is not 0 and int value ? ShadowflamePyreProjectile.MaxTimeLeft * value * 0.2f : 0;
				Vector2 placePos = Projectile.Center;
				Vector2 projBaseSize = ShadowflamePyreProjectile.BaseSize;
				
				while (Collision.SolidCollision(placePos - projBaseSize / 2, (int)projBaseSize.X, (int)projBaseSize.Y) && placePos.Y > 0)
				{
					placePos.Y--;
				}

				int proj = Projectile.NewProjectile(Projectile.GetSource_Death(), placePos, Vector2.Zero, type, Projectile.damage, 0, Projectile.owner, timeExtension);
				Main.projectile[proj].GetGlobalProjectile<ElementalProjectile>().AddElementalValues((ElementType.Chaos, 0, 1), (ElementType.Fire, 0, 1));
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			int frameHeight = tex.Height / Main.projFrames[Type];
			var src = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
			Color col = Color.White * Projectile.Opacity;
			Vector2 position = Projectile.Center - Main.screenPosition;
			Vector2 origin = src.Size() / new Vector2(1.5f, 2);

			if (Owner.HasSkillSpecialization<Fireball, ShadowflamePyre>())
			{
				col = Color.Purple * Projectile.Opacity;
			}
			
			Main.EntitySpriteDraw(tex, position, src, col * Projectile.scale, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
	}

	private class FrostfireMeteorProjectile : SkillProjectile<Fireball>, HitNPCHooks.IPostHitNPCProjectile
	{
		public override string Texture => $"{PoTMod.ModName}/Assets/Skills/Magic/" + GetType().Name;

		private static readonly ExplosionHitbox.VFXPackage ExplosionVfx = new(0, 14, 14, true, 0.4f, null, DustID.IceTorch, DustID.Torch);

		private Player Owner => Main.player[Projectile.owner];

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.Size = new Vector2(22);
			Projectile.timeLeft = 600;
			Projectile.penetrate = 1;
			Projectile.aiStyle = -1;
			Projectile.scale = 1f;
			Projectile.extraUpdates = 1;
			Projectile.Opacity = 0f;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
			Projectile.Opacity += 0.01f;

			if (Projectile.Opacity > 1)
			{
				Projectile.Opacity = 1f;
			}

			if (Main.rand.NextBool(6))
			{
				SpawnDust(1, 0.4f);
			}
		}

		private void SpawnDust(int count, float strength = 1f, bool deathVfx = false)
		{
			for (int i = 0; i < count; ++i)
			{
				float scale = deathVfx ? Main.rand.NextFloat(1.6f, 2.2f) : Main.rand.NextFloat(0.8f, 1.4f);
				Vector2 vel = Projectile.velocity * strength + (deathVfx ? Main.rand.NextVector2Circular(6, 6) : Vector2.Zero);
				Vector2 position = Projectile.position + new Vector2(Main.rand.NextFloat(Projectile.width), Main.rand.NextFloat(Projectile.height));
				Dust.NewDustPerfect(position, Main.rand.NextBool() ? DustID.IceTorch : DustID.Torch, vel, Scale: scale);
			}
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (Owner.HasTreePassive<FireballTree, AbsoluteZero>() && (target.HasBuff<FreezeDebuff>() || target.HasBuff<IgnitedDebuff>()))
			{
				modifiers.FinalDamage += 0.35f;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Owner.HasTreePassive<FireballTree, ThermalFeedback>())
			{
				Owner.statMana = Math.Min(Owner.statMana + 5, Owner.statManaMax2);
			}
		}

		public override void OnKill(int timeLeft)
		{
			SpawnDust(2, 0.6f, true);

			if (Owner.HasTreePassive<FireballTree, CrystallineImpact>())
			{
				int type = ModContent.ProjectileType<BlastIcicleSmall>();
				int damage = Projectile.damage / 8;

				for (int i = 0; i < 3; ++i)
				{
					Vector2 vel = Main.rand.NextVector2Circular(4, 4) - Projectile.velocity * 0.5f;
					float scale = Owner.GetPassiveStrength<FireballTree, Rime>() * 0.3333f + 1;
					int icicle = Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, vel, type, damage, 0, Main.myPlayer, scale);
					Main.projectile[icicle].GetGlobalProjectile<ElementalProjectile>().AddElementalValues((ElementType.Cold, 0, 1));
				}
			}

			if (Owner.HasTreePassive<FireballTree, FrozenGround>())
			{
				Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ColdAura>(), 0, 0, Projectile.owner);
			}

			int proj = ExplosionHitbox.QuickSpawn(Projectile.GetSource_Death(), Projectile, (int)(Projectile.damage * 0.75f), Projectile.owner, Projectile.Size * 2, ExplosionVfx);
			ElementalProjectile ele = Main.projectile[proj].GetGlobalProjectile<ElementalProjectile>();
			ele.AddElementalValues((ElementType.Fire, 0, 1), (ElementType.Cold, 0, 1));
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			Vector2 position = Projectile.Center - Main.screenPosition;
			Main.spriteBatch.Draw(tex, position, null, Projectile.GetAlpha(lightColor), Projectile.rotation, new Vector2(12, 30), 1f, SpriteEffects.None, 0);
			return false;
		}

		public void PostHitNPC(NPC target, in NPC.HitInfo hit, int damageDone)
		{
			if (target.HasBuff<IgnitedDebuff>() && (target.HasBuff<FreezeDebuff>() || target.HasBuff(BuffID.Chilled)))
			{
				target.DelBuff(target.FindBuffIndex(ModContent.BuffType<IgnitedDebuff>()));

				if (target.FindBuffIndex(ModContent.BuffType<FreezeDebuff>()) is not -1 and int freezeIndex)
				{
					target.DelBuff(freezeIndex);
				}
				else
				{
					target.DelBuff(target.FindBuffIndex(BuffID.Chilled));
				}

				IEntitySource src = Projectile.GetSource_Death();
				int proj = ExplosionHitbox.QuickSpawn(src, Projectile, (int)(Projectile.damage * 4), Projectile.owner, Projectile.Size * 15, ExplosionHitbox.VFXPackage.None);
				ElementalProjectile ele = Main.projectile[proj].GetGlobalProjectile<ElementalProjectile>();
				ele.AddElementalValues((ElementType.Fire, 0, 1), (ElementType.Cold, 0, 1));

				for (int i = 0; i < 28; ++i)
				{
					Vector2 offset = Main.rand.NextVector2Circular(50, 50);
					int id = Main.rand.NextBool() ? DustID.IceTorch : Main.rand.NextBool() ? DustID.Smoke : DustID.Torch;
					Vector2 vel = offset / 12f;

					if (id == DustID.Smoke)
					{
						vel *= Main.rand.NextFloat(1.5f, 2f);
					}

					Dust.NewDustPerfect(target.Center + offset, id, vel, Scale: Main.rand.NextFloat(1, 2));
				}

				for (int i = 0; i < 14; ++i)
				{
					Vector2 offset = Main.rand.NextVector2Circular(50, 50);
					Gore.NewGore(target.GetSource_FromAI(), target.Center + offset, offset / 12f, ModContent.GoreType<ColdAir>(), Main.rand.NextFloat(0.8f, 1.5f));
				}
			}
		}
	}

	private class ShadowflamePyreProjectile : SkillProjectile<Fireball>
	{
		private class ShadowflameParticle(Vector2 position, int timeLeft, Color start, Color end, Vector2 velocity)
		{
			public readonly int MaxTimeLeft = timeLeft;

			public readonly Rectangle Frame = Main.rand.Next(4) switch
			{
				0 => new(0, 0, 8, 8),
				1 => new(10, 0, 10, 10),
				2 => new(22, 0, 12, 10),
				_ => new(36, 0, 10, 12),
			};

			public Color Color => Color.Lerp(StartColor, EndColor, ReverseFactor);
			public float ReverseFactor => 1 - Factor;
			public float Factor => TimeLeft / (float)MaxTimeLeft;
			
			public float Opacity
			{ 
				get
				{
					const int FadeTime = 30;

					if (TimeLeft > MaxTimeLeft - FadeTime)
					{
						return Utils.GetLerpValue(MaxTimeLeft - FadeTime, MaxTimeLeft, TimeLeft);
					}

					return ReverseFactor;
				}
			}

			public Vector2 Position = position;
			public Color StartColor = start;
			public Color EndColor = end;
			public int TimeLeft = timeLeft;
			public Vector2 Velocity = velocity;
			public float RotationSpeed = Main.rand.NextFloat(0.9f, 1.2f);

			public void Update()
			{
				Position += Velocity;
				TimeLeft--;
				Velocity *= 0.97f;
				Velocity.X += Main.windSpeedCurrent * 0.1f;

				Point16 tile = Position.ToTileCoordinates16();
				Velocity.X += Main.instance.TilesRenderer.GetWindGridPush(tile.X, tile.Y, 2, 1.5f);
			}
		}

		internal const int MaxTimeLeft = 5 * 60;

		internal readonly static Vector2 BaseSize = new(96, 220);
		internal readonly static Color[] Colors = [Color.White, new Color(133, 10, 205), new Color(39, 7, 81)];

		internal static Asset<Texture2D> ParticleTexture = null;

		public override string Texture => $"{PoTMod.ModName}/Assets/Skills/Magic/" + GetType().Name;

		private Player Owner => Main.player[Projectile.owner];

		private ref float TimeDelay => ref Projectile.ai[0];
		private ref float Flare => ref Projectile.ai[1];
		private ref float Erupt => ref Projectile.ai[2];

		private readonly List<ShadowflameParticle> _particle = [];

		public override void SetStaticDefaults()
		{
			ParticleTexture = ModContent.Request<Texture2D>(Texture + "Particles");
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.Size = BaseSize;
			Projectile.timeLeft = MaxTimeLeft;
			Projectile.penetrate = -1;
			Projectile.aiStyle = -1;
			Projectile.scale = 1f;
			Projectile.tileCollide = false;
			Projectile.netImportant = true;
		}

		public override void AI()
		{
			const float MaxScale = 2.5f;

			if (TimeDelay > 0)
			{
				TimeDelay--;
				Projectile.timeLeft++;
			}

			foreach (ShadowflameParticle particle in _particle)
			{
				particle.Update();
			}

			if (Projectile.timeLeft <= 2)
			{
				Projectile.timeLeft++;

				if (_particle.Count == 0)
				{
					Projectile.Kill();
				}
			
				return;
			}
			else
			{
				float repeats = 18 + (Projectile.scale - 1) * 16;

				for (int i = 0; i < repeats; ++i)
				{
					AddParticle();
				}
			}

			Erupt--;
			Flare = MathHelper.Lerp(Flare, MathF.Sin((Projectile.timeLeft - TimeDelay) * 0.04f) * 0.1f + 1f, 0.1f);

			if (Owner.HasTreePassive<FireballTree, ConjoinedFlames>() && Main.myPlayer == Projectile.owner) 
			{ 
				for (int i = 0; i < Projectile.whoAmI; ++i)
				{
					Projectile projectile = Main.projectile[i];

					if (projectile.active && projectile.type == Type && projectile.owner == Projectile.owner && projectile.Hitbox.Intersects(Projectile.Hitbox))
					{
						var proj = projectile.ModProjectile as ShadowflamePyreProjectile;
						_particle.AddRange(proj._particle);
						proj._particle.Clear();

						Flare = 1.5f;

						if (Owner.HasTreePassive<FireballTree, ShadowflameEruption>())
						{
							Erupt = 120;
						}

						Projectile.timeLeft += projectile.timeLeft;
						Projectile.damage += (int)(projectile.damage * 0.33f);
						Projectile.scale += projectile.scale * 0.25f;

						if (Projectile.scale > MaxScale)
						{
							Projectile.scale = MaxScale;
							ForceSize();
						}

						Projectile.netUpdate = true;
						projectile.netUpdate = true;
						projectile.Kill();
					}
				}
			}

			if (Owner.HasTreePassive<FireballTree, PainIsPleasure>() && Owner.Hitbox.Intersects(Projectile.Hitbox))
			{
				if (Projectile.timeLeft % 15 == 0)
				{
					Owner.Hurt(new Player.HurtInfo()
					{
						DamageSource = PlayerDeathReason.ByCustomReason(NetworkText.FromKey(this.GetLocalizationKey("DeathReason." + Main.rand.Next(3)), Owner.name)),
						Damage = 5,
						Dodgeable = false,
						HitDirection = 0,
						Knockback = 0,
						SoundDisabled = true,
						CooldownCounter = ImmunityCooldownID.TileContactDamage
					});
				}

				ElementalPlayer plr = Owner.GetModPlayer<ElementalPlayer>();
				plr.Container.AddElementalValues((ElementType.Chaos, 0, 0, 0.25f), (ElementType.Fire, 0, 0, 0.25f));
			}

			if (Owner.HasTreePassive<FireballTree, CrawlingFlame>() && Projectile.scale < MaxScale && Main.myPlayer == Projectile.owner)
			{
				Projectile.scale += 0.001f;
				ForceSize();
			}
		}

		private void ForceSize()
		{
			Vector2 size = BaseSize * Projectile.scale;

			Projectile.position = Projectile.Bottom;
			Projectile.width = (int)size.X;
			Projectile.height = (int)size.Y;
			Projectile.Bottom = Projectile.position;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write((Half)Projectile.scale);
			writer.Write((short)Projectile.timeLeft);
			writer.WriteVector2(Projectile.Size);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			Projectile.scale = (float)reader.ReadHalf();
			Projectile.timeLeft = reader.ReadInt16();
			Projectile.Size = reader.ReadVector2();
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(ModContent.BuffType<EverburningShadowflameDebuff>(), 2);
			target.GetGlobalNPC<EverburningShadowflameDebuff.EverburningShadowflameNPC>().LastPlayerApplied = Owner.whoAmI;

			if (Owner.HasTreePassive<FireballTree, AbyssalHunger>() && Main.rand.NextFloat() < 0.1f)
			{
				int heal = Math.Max(damageDone / 20, 1);

				Owner.Heal(heal);
				Owner.statMana = Math.Min(Owner.statMana, Owner.statManaMax2);
				Owner.ManaEffect(heal);
			}
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (Erupt > 0)
			{
				modifiers.FinalDamage += 0.5f;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			int rot = 0;

			foreach (ShadowflameParticle particle in _particle)
			{
				Color color = particle.Color * (1 - particle.Opacity);
				float scale = MathHelper.Lerp(0, 2, particle.Factor);
				float rotation = rot++ * 0.02f + (Main.GameUpdateCount * 0.3f * particle.RotationSpeed);
				Main.spriteBatch.Draw(ParticleTexture.Value, particle.Position - Main.screenPosition, particle.Frame, color, rotation, new Vector2(4), scale, SpriteEffects.None, 0);
			}

			_particle.RemoveAll(x => x.TimeLeft <= 0);
			return false;
		}

		private void AddParticle()
		{
			float xOffset = Main.rand.NextFloat();
			float distanceFromCenter = Math.Abs(xOffset - 0.5f) * 2;

			if (Main.rand.NextBool(4))
			{
				xOffset -= MathF.Pow(Main.rand.NextFloat(0, 0.5f), 2) * (Main.rand.NextBool() ? -1 : 1);
			}

			Vector2 center = new(Projectile.BottomLeft.X + MathHelper.Lerp(0, Projectile.width, xOffset), Projectile.BottomLeft.Y);

			distanceFromCenter *= Main.rand.NextFloat(1f, 1.5f);
			distanceFromCenter = MathF.Min(1, distanceFromCenter);

			var start = Color.Lerp(Colors[0], Colors[1], distanceFromCenter);
			var end = Color.Lerp(Colors[1], Colors[2], MathHelper.Lerp(distanceFromCenter, 1, Main.rand.NextFloat()));
			float baseYVel = -Main.rand.NextFloat(6f, 8.5f);
			var velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), MathHelper.Lerp(baseYVel, baseYVel + distanceFromCenter * 4, MathF.Pow(Main.rand.NextFloat(), 2)));
			velocity.Y = MathHelper.Lerp(velocity.Y, -3, Main.rand.NextFloat(distanceFromCenter));
			center.Y -= MathF.Pow(distanceFromCenter, 2) * 20;
			center.Y += 20;

			if (Erupt > 0)
			{
				float factor = Erupt / 120f;
				start = Color.Lerp(start, Color.White, distanceFromCenter * factor);
				end = Color.Lerp(end, Color.Red, distanceFromCenter * factor);
			}

			_particle.Add(new ShadowflameParticle(center, 80, start, end, velocity * Projectile.scale * Flare));
		}
	}
}