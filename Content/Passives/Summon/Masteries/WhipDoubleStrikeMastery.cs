using Humanizer;
using PathOfTerraria.Common.Events;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class WhipDoubleStrikeMastery : Passive
{
	internal sealed class DelayedStrike : ModProjectile
	{
		public const int DelayTime = 40;
		public override string Texture => "Terraria/Images/Projectile_0";

		public int NPCWhoAmI
		{
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void SetDefaults()
		{
			Projectile.timeLeft = DelayTime;
			Projectile.hide = true;
		}

		public override void AI()
		{
			if (Main.npc[NPCWhoAmI] is NPC victim && victim.active)
			{
				Projectile.Center = victim.Center;
			}
			else
			{
				Projectile.active = false; //Forgo OnKill
			}
		}

		public override void OnKill(int timeLeft)
		{
			if (!Main.dedServ)
			{
				SoundEngine.PlaySound(SoundID.Item153 with { Pitch = 0.3f }, Projectile.Center);
				for (int i = 0; i < 10; i++)
				{
					var dust = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, Main.rand.NextVector2Circular(5, 5) * Main.rand.NextFloat(), Scale: 1.5f);
					dust.noGravity = true;
				}

				float num = 30f;
				float num2 = 0f;
				Vector2 position = Projectile.Center;

				for (float num3 = 0f; num3 < 4f; num3 += 1f)
				{
					PrettySparkleParticle prettySparkleParticle = new();
					Vector2 vector = ((float)Math.PI / 2f * num3 + num2).ToRotationVector2() * 4f;
					prettySparkleParticle.ColorTint = new Color(0.2f, 0.85f, 0.9f, 0.5f);
					prettySparkleParticle.LocalPosition = position;
					prettySparkleParticle.Rotation = vector.ToRotation();
					prettySparkleParticle.Scale = new Vector2((num3 % 2f == 0f) ? 2f : 4f, 0.5f) * 1.1f;
					prettySparkleParticle.FadeInNormalizedTime = 5E-06f;
					prettySparkleParticle.FadeOutNormalizedTime = 0.95f;
					prettySparkleParticle.TimeToLive = num;
					prettySparkleParticle.FadeOutEnd = num;
					prettySparkleParticle.FadeInEnd = num / 2f;
					prettySparkleParticle.FadeOutStart = num / 2f;
					prettySparkleParticle.AdditiveAmount = 0.35f;
					prettySparkleParticle.Velocity = -vector * 0.2f;
					prettySparkleParticle.DrawVerticalAxis = false;
					if (num3 % 2f == 1f)
					{
						prettySparkleParticle.Scale *= 1.5f;
						prettySparkleParticle.Velocity *= 1.5f;
					}

					Main.ParticleSystem_World_OverPlayers.Add(prettySparkleParticle);
				}

				for (float num4 = 0f; num4 < 4f; num4 += 1f)
				{
					PrettySparkleParticle prettySparkleParticle2 = new();
					Vector2 vector2 = ((float)Math.PI / 2f * num4 + num2).ToRotationVector2() * 4f;
					prettySparkleParticle2.ColorTint = new Color(1f, 1f, 0.2f, 1f);
					prettySparkleParticle2.LocalPosition = position;
					prettySparkleParticle2.Rotation = vector2.ToRotation();
					prettySparkleParticle2.Scale = new Vector2((num4 % 2f == 0f) ? 2f : 4f, 0.5f) * 0.7f;
					prettySparkleParticle2.FadeInNormalizedTime = 5E-06f;
					prettySparkleParticle2.FadeOutNormalizedTime = 0.95f;
					prettySparkleParticle2.TimeToLive = num;
					prettySparkleParticle2.FadeOutEnd = num;
					prettySparkleParticle2.FadeInEnd = num / 2f;
					prettySparkleParticle2.FadeOutStart = num / 2f;
					prettySparkleParticle2.Velocity = vector2 * 0.2f;
					prettySparkleParticle2.DrawVerticalAxis = false;
					if (num4 % 2f == 1f)
					{
						prettySparkleParticle2.Scale *= 1.5f;
						prettySparkleParticle2.Velocity *= 1.5f;
					}

					Main.ParticleSystem_World_OverPlayers.Add(prettySparkleParticle2);
					for (int i = 0; i < 1; i++)
					{
						var dust = Dust.NewDustPerfect(position, DustID.IchorTorch, vector2.RotatedBy(Main.rand.NextFloatDirection() * ((float)Math.PI * 2f) * 0.025f) * Main.rand.NextFloat());
						dust.noGravity = true;
						dust.scale = 1.4f;
						var dust2 = Dust.NewDustPerfect(position, DustID.IchorTorch, -vector2.RotatedBy(Main.rand.NextFloatDirection() * ((float)Math.PI * 2f) * 0.025f) * Main.rand.NextFloat());
						dust2.noGravity = true;
						dust2.scale = 1.4f;
					}
				}
			}

			if (Projectile.owner == Main.myPlayer && Main.npc[NPCWhoAmI] is NPC victim && victim.active)
			{
				NPC.HitInfo info = default;
				info.Damage = Projectile.damage;
				info.DamageType = DamageClass.Default;

				Main.player[Projectile.owner].StrikeNPCDirect(victim, info);
			}
		}

		public override bool? CanDamage()
		{
			return false;
		}
	}

	public sealed class WhipDoubleStrikeMasteryPlayer : ModPlayer
	{
		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (ProjectileID.Sets.IsAWhip[proj.type] && Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(Name) != 0)
			{
				Projectile.NewProjectile(Player.GetSource_OnHit(target), target.Center, Vector2.Zero, ModContent.ProjectileType<DelayedStrike>(), (int)(damageDone * DamageMult), 0, Player.whoAmI, target.whoAmI);
			}
		}
	}

	public const float DamageMult = 0.5f;

	public override string DisplayTooltip => Language.GetTextValue($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").FormatWith(MathUtils.Percent(DamageMult));
}