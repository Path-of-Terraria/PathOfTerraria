using PathOfTerraria.Common;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.SkillPassives.FlameSage;
using PathOfTerraria.Content.Skills.Summon;
using PathOfTerraria.Content.SkillTrees;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.SkillSpecials.FlameSageSpecials;

public class Flamethrower(SkillTree tree) : SkillSpecial(tree)
{
	public class FlamethrowerSentry : FlameSage.FlameSentry
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			NPC.damage = 10;
		}

		public override void AI()
		{
			const int cooldownMax = 10;
			const int range = 200;

			if ((Cooldown = Math.Max(Cooldown - 1, 0)) == 0)
			{
				Terraria.Utilities.NPCUtils.TargetSearchResults results = FindTarget();
				NPC target = results.NearestNPC;

				if (results.FoundNPC && Collision.CanHit(NPC, target) && target.DistanceSQ(NPC.Center) < range * range)
				{
					int baseDamage = NPC.damage;
					float radius = 0.4f;
					int compression = Owner.GetPassiveStrength<FlameSageTree, HeatCompression>();

					if (compression > 0)
					{
						baseDamage = (int)(baseDamage * (1 + HeatCompression.DamageBonus * compression));
						radius /= (compression + 1);
					}

					int damage = (int)Owner.GetDamage(DamageClass.Summon).ApplyTo(baseDamage);
					Vector2 velocity = (NPC.DirectionTo(target.Center) * 4).RotatedByRandom(radius);

					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, ModContent.ProjectileType<Flame>(), damage, 1, Owner.whoAmI);

					Cooldown = cooldownMax;
				}
			}

			if (NPC.Opacity == 0) //Just spawned in
			{
				SoundEngine.PlaySound(SoundID.DD2_WyvernDiveDown, NPC.Center);
			}

			NPC.Opacity = Math.Min(NPC.Opacity + 0.08f, 1);

			int combustion = Owner.GetPassiveStrength<FlameSageTree, CombustiveAura>();
			if (combustion > 0)
			{
				Rectangle hitbox = NPC.Hitbox;
				hitbox.Inflate(100, 100);

				FlamethrowerNPC.DealDoT(hitbox, (int)(NPC.damage * 0.5f * combustion), Owner, static (npc) =>
				{
					int size = (npc.width * npc.height) / 500;
					for (int i = 0; i < size; i++)
					{
						var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Torch, Scale: Main.rand.NextFloat(1, 3));
						dust.noGravity = true;
						dust.velocity = -Vector2.UnitY;
					}

					if (npc.AnyInteractions()) //Apply effects from the Overwhelming Pressure passive
					{
						Player player = Main.player[npc.lastInteraction];
						int overwhelmingPressure = player.GetPassiveStrength<FlameSageTree, OverwhelmingPressure>();

						if (overwhelmingPressure > 0)
						{
							int weakening = player.GetPassiveStrength<FlameSageTree, Weakening>();
							npc.GetGlobalNPC<ElementalNPC>().Container.FireResistance -= ((overwhelmingPressure * OverwhelmingPressure.ResistanceDecrease) + (weakening * Weakening.ResistanceDecrease));
						}
					}
				});
			}
		}
	}

	public class Flame : ModProjectile
	{
		public const int TimeLeftMax = 50;
		public float TimeSpan => 1f - ((float)Projectile.timeLeft / TimeLeftMax);

		public override string Texture => "Terraria/Images/Extra_55";

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 4;
			ProjectileID.Sets.TrailCacheLength[Type] = 8;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.Size = new Vector2(30);
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = TimeLeftMax;
		}

		public override void AI()
		{
			var dust = Dust.NewDustPerfect(Projectile.Center + (Main.rand.NextVector2Unit() * Main.rand.NextFloat() * TimeSpan * 50f), DustID.Torch, Scale: Main.rand.NextFloat() + (TimeSpan * 0.5f));
			dust.noGravity = true;

			Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2 + Main.rand.NextFloat(-0.5f, 0.5f);
			Projectile.velocity *= 0.98f;

			if (Projectile.timeLeft > (TimeLeftMax - 10))
			{
				if (Main.rand.NextBool(2))
				{
					int type = Main.rand.NextFromList(GoreID.Smoke1, GoreID.Smoke2, GoreID.Smoke3);

					var gore = Gore.NewGorePerfect(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, type, (1f - TimeSpan) * Main.rand.NextFloat(0.75f, 1.5f));
					gore.position -= new Vector2(gore.Width / 2, gore.Height / 2);
					gore.alpha = Main.rand.Next(180, 230);
				}
			}

			if (++Projectile.frameCounter >= 4)
			{
				Projectile.frameCounter = 0;
				Projectile.frame = ++Projectile.frame % Main.projFrames[Type];
			}

			Projectile.alpha = (int)(TimeSpan * 255f);
			Projectile.scale = TimeSpan * 1.25f;

			FlamethrowerNPC.DealDoT(Projectile.Hitbox, Projectile.damage, Main.player[Projectile.owner]);
		}

		public override bool? CanDamage()
		{
			return false; //Damage is instead dealt over time in AI
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
			Color unitCol = Color.Lerp(Color.Blue, Color.Orange, TimeSpan) with { A = 0 };

			for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Type]; i++)
			{
				Color color = Projectile.GetAlpha(unitCol) * (1f - (i / (float)ProjectileID.Sets.TrailCacheLength[Type]));
				Vector2 pos = Projectile.oldPos[i] + (Projectile.Size / 2) - Main.screenPosition + new Vector2(0, Projectile.gfxOffY);
				float scale = Projectile.scale + ((float)Math.Sin(Main.timeForVisualEffects / (i * 5f)) * 0.03f);

				Main.EntitySpriteDraw(texture, pos, frame, color, Projectile.oldRot[i], frame.Size() / 2, scale, default);
			}

			return false;
		}
	}
}

internal class FlamethrowerNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	public int DamageOverTime;
	private int _ticksOnFire; //Counts the number of ticks an NPC has been affected by DoT for

	public override void UpdateLifeRegen(NPC npc, ref int damage)
	{
		if (DamageOverTime > 0)
		{
			npc.lifeRegen = Math.Min(npc.lifeRegen, 0);
			float finalDamage = DamageOverTime;

			if (npc.AnyInteractions())
			{
				Player player = Main.player[npc.lastInteraction];

				if (_ticksOnFire > (60 * MeltingPoint.Seconds))
				{
					int melting = player.GetPassiveStrength<FlameSageTree, MeltingPoint>();
					finalDamage *= 1 + MeltingPoint.DamageBonus * melting;
				}

				int enduringFlame = player.GetPassiveStrength<FlameSageTree, EnduringFlame>();
				finalDamage *= 1 + (EnduringFlame.DamageBonus * (_ticksOnFire / 60) * enduringFlame);
			}

			npc.lifeRegen -= (int)(finalDamage * 2); //Translate to damage per second

			DamageOverTime = 0;
			_ticksOnFire++;
		}
	}

	public static void DealDoT(Rectangle hitbox, int damage, Player source, Action<NPC> extraAction = null)
	{
		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.CanBeChasedBy() && npc.Hitbox.Intersects(hitbox) && npc.TryGetGlobalNPC(out FlamethrowerNPC global))
			{
				npc.ApplyInteraction(source.whoAmI);

				extraAction?.Invoke(npc);
				global.DamageOverTime += damage;
			}
		}
	}
}