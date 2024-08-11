using System.Collections.Generic;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using Terraria.ID;

namespace PathOfTerraria.Content.Skills.Ranged;

public class FetidCarapace : Skill
{
	public override int MaxLevel => 3;
	public override List<SkillPassive> Passives => [];

	public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = 30 * 60;
		Timer = 0;
		ManaCost = 20;
		Duration = (6 + Level * 4) * 60;
		WeaponType = ItemType.Ranged;
	}

	public override void UseSkill(Player player)
	{
		// Level to the strength of all FetidCarapaceAffix
		LevelTo((byte)player.GetModPlayer<AffixPlayer>().StrengthOf<FetidCarapaceAffix>());

		int damage = (int)(30 * (1f - (3 - Level) * 0.2f));
		int max = 1 + Level;
		int type = ModContent.ProjectileType<CarapaceChunk>();

		for (int i = 0; i < max; ++i)
		{
			int proj = Projectile.NewProjectile(player.GetSource_FromAI(), player.Center, Vector2.Zero, type, damage, 8f, player.whoAmI, 0, max, i);
			Main.projectile[proj].timeLeft = Duration;
		}

		Timer = Cooldown;
	}

	public override bool CanUseSkill(Player player)
	{
		bool canUse = base.CanUseSkill(player);

		if (!canUse) // If we can't use the skill, attempt to shoot the projectiles
		{
			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<CarapaceChunk>() && proj.ai[0] == 0)
				{
					proj.ai[0] = 1;
					proj.velocity = player.DirectionTo(Main.MouseWorld) * 14;
				}
			}
		}

		return canUse;
	}

	public override bool CanEquipSkill(Player player)
	{
		// TODO: If this needs to be equippable without the affix, figure out that system
		return player.GetModPlayer<AffixPlayer>().StrengthOf<FetidCarapaceAffix>() > 0;
	}

	internal class CarapaceChunk : ModProjectile
	{
		public override string Texture => $"{PoTMod.ModName}/Assets/Items/Gear/Weapons/Javelins/{GetType().Name}";

		public bool SentOut
		{
			get => Projectile.ai[0] == 1;
			set => Projectile.ai[0] = value ? 1 : 0;
		}

		private ref float MaxChunks => ref Projectile.ai[1];
		private ref float ChunkIndex => ref Projectile.ai[2];

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 3;
		}

		public override void SetDefaults()
		{
			// Note that timeLeft is set in FetidCarapace.UseSkill since it's variable
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.Size = new Vector2(18);
			Projectile.penetrate = 1;
			Projectile.aiStyle = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			if (Projectile.frameCounter == 0)
			{
				Projectile.frame = Main.rand.Next(3);
				Projectile.frameCounter = 1;
			}

			if (!SentOut)
			{
				Player owner = Main.player[Projectile.owner];
				float rotation = ChunkIndex / MaxChunks * MathHelper.TwoPi + Projectile.timeLeft * 0.075f;

				Projectile.Center = owner.Center + new Vector2(60, 0).RotatedBy(rotation);
				Projectile.rotation = rotation - MathHelper.PiOver4;
			}
			else
			{
				Projectile.velocity.Y += 0.1f;
				Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation() + MathHelper.PiOver4, 0.3f);
				Projectile.timeLeft++;

				if (!Projectile.tileCollide && !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
				{
					Projectile.tileCollide = true;
				}
			}

			if (Main.rand.NextBool(20))
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Demonite);
			}

			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.whoAmI != Projectile.whoAmI && proj.Hitbox.Intersects(Projectile.Hitbox) && proj.hostile)
				{
					if (ProjectileLoader.OnTileCollide(proj, proj.velocity)) // If the projectile collides normally with tiles, kill it
					{
						proj.Kill();
					}
					else // Otherwise, reflect it
					{
						proj.velocity *= -1;
					}

					Projectile.Kill();
				}
			}
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 8; ++i)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Demonite, Projectile.velocity.X, Projectile.velocity.Y);
			}
		}
	}
}
