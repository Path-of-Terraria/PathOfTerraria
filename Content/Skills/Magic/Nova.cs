using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.UI.Hotbar;
using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.SkillPassives;
using PathOfTerraria.Content.SkillSpecials;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Utilities;

namespace PathOfTerraria.Content.Skills.Magic;

public class Nova : Skill
{
	public enum NovaType : byte
	{
		Normal,
		Fire,
		Ice,
		Lightning,
	}

	public override int MaxLevel => 3;

	private static NovaType GetNovaType(Nova nova)
	{
		SkillSpecial special = nova.Tree.Specialization;

		return special switch
		{
			FireNova => NovaType.Fire,
			IceNova => NovaType.Ice,
			LightningNova => NovaType.Lightning,
			_ => NovaType.Normal
		};
	}

	public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = (15 - Level) * 60;
		ManaCost = 20 + 5 * Level;
		Duration = 0;
		WeaponType = ItemType.Magic;
	}

	public override void UseSkill(Player player)
	{
		base.UseSkill(player);

		int damage = GetTotalDamage(player.HeldItem.damage * (2 + 0.5f * Level));
		var source = new EntitySource_UseSkill(player, this);
		NovaType type = GetNovaType(this);
		float knockback = 2f;

		switch (type)
		{
			case NovaType.Fire:
				knockback = 4f;
				break;
			case NovaType.Lightning:
				{
					WeightedRandom<float> mult = new(Main.rand);
					mult.Add(1f, 1f);
					mult.Add(0.75f, 1f);
					mult.Add(0.5f, 1f);
					mult.Add(1.5f, 1f);
					mult.Add(2f, 1f);

					damage = (int)(damage * mult);
					break;
				}
			case NovaType.Ice:
				damage = (int)(damage * 0.9f);
				break;
		}

		if (Tree.TryGetNode(out VolatileNova vNova) && vNova.Allocated) //Passive synergy
		{
			for (int i = 0; i < 3; i++)
			{
				Vector2 position = player.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(NovaProjectile.AreaOfEffect);

				var smallBlast = Projectile.NewProjectileDirect(source, position, Vector2.Zero, ModContent.ProjectileType<NovaProjectile>(), damage, knockback, player.whoAmI, (int)type);
				smallBlast.scale = Main.rand.NextFloat(0.2f, 0.3f);
				smallBlast.netUpdate = true;
			}
		}
		else
		{
			Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<NovaProjectile>(), damage, knockback, player.whoAmI, (int)type);
		}
	}

	public override bool CanUseSkill(Player player)
	{
		return base.CanUseSkill(player) && player.HeldItem.CountsAsClass(DamageClass.Magic);
	}

	public override void ModifyTooltips(List<NewHotbar.SkillTooltip> tooltips)
	{
		tooltips.Remove(tooltips.FirstOrDefault(x => x.Name == "WeaponType"));
		NewHotbar.SkillTooltip tooltip = tooltips.First(x => x.Name == "Description");
		string text = Language.GetText($"Mods.{PoTMod.ModName}.Skills.NeedsWeapon").Format(WeaponType.LocalizeText().ToLower());

		if (Main.LocalPlayer.HeldItem.CountsAsClass(DamageClass.Magic))
		{
			text = Language.GetText($"Mods.{PoTMod.ModName}.Skills.BaseDamage").Format(Main.LocalPlayer.HeldItem.damage);
		}

		tooltips.Add(new NewHotbar.SkillTooltip("ExtraDamage", text, tooltip.Slot + 0.1f));
	}

	private class NovaProjectile : SkillProjectile<Nova>
	{
		public const int AreaOfEffect = 300;
		public int Spread => (int)((1 - Projectile.timeLeft / 30f) * Skill.GetTotalAreaOfEffect(300f * Projectile.scale));

		public override string Texture => "Terraria/Images/NPC_0";

		private NovaType NovaType => (NovaType)Projectile.ai[0];
		private ref float Timer => ref Projectile.ai[1];
		private ref float DecaySpeed => ref Projectile.ai[2];

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.timeLeft = 30;
			Projectile.penetrate = -1;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			if (DecaySpeed == 0)
			{
				DecaySpeed = 0.075f;
			}

			Timer += 0.04f;
			DecaySpeed *= 0.94f;

			if (NovaType == NovaType.Fire)
			{
				SpamFireDust();
			}
			else if (NovaType == NovaType.Ice)
			{
				SpamIceDust();
			}
			else if (NovaType == NovaType.Lightning)
			{
				SpamLightningDust();
			}
		}

		private void SpamFireDust()
		{
			for (int i = 0; i < 2; ++i)
			{
				Vector2 offset = Main.rand.NextVector2CircularEdge(Spread, Spread);
				Vector2 pos = Projectile.Center + offset;
				var dust = Dust.NewDustPerfect(pos, DustID.Torch, Vector2.Normalize(offset) * Main.rand.NextFloat(2, 4), Scale: Main.rand.NextFloat(1, 2));
				dust.noGravity = true;
			}
		}

		private void SpamIceDust()
		{
			float dustIterations = Main.rand.Next(5, 8);

			if (Projectile.timeLeft % 2 != 0)
			{
				return;
			}

			for (int i = 0; i < dustIterations; ++i)
			{
				Vector2 spread = new Vector2(Spread, 0).RotatedBy(i / dustIterations * MathHelper.TwoPi);
				Vector2 lastSpread = new Vector2(Spread, 0).RotatedBy((i - 1) / dustIterations * MathHelper.TwoPi);
				float scale = Main.rand.NextFloat(0.9f, 1.5f) * (1 - Projectile.timeLeft / 30f);

				for (int j = 0; j < 10; ++j)
				{
					var useSpread = Vector2.Lerp(lastSpread, spread, j / 14f);
					int dustType = Main.rand.NextBool() ? DustID.SnowflakeIce : DustID.SnowSpray;
					var dust = Dust.NewDustPerfect(Projectile.Center + useSpread, dustType, useSpread.DirectionTo(Projectile.Center), Scale: scale);
					dust.noGravity = true;
					dust.fadeIn = 0.3f + j / 14f * 0.8f;
				}
			}
		}

		private void SpamLightningDust()
		{
			float dustIterations = Main.rand.Next(5, 8);

			if (Projectile.timeLeft % 3 <= 1)
			{
				return;
			}

			for (int i = 0; i < dustIterations; ++i)
			{
				Vector2 spread = new Vector2(Spread, 0).RotatedBy(i / dustIterations * MathHelper.TwoPi) * Main.rand.NextFloat(0.8f, 1.2f);
				Vector2 lastSpread = new Vector2(Spread, 0).RotatedBy((i - 1) / dustIterations * MathHelper.TwoPi) * Main.rand.NextFloat(0.8f, 1.2f);
				float scale = Main.rand.NextFloat(0.9f, 1.5f) * (1 - Projectile.timeLeft / 30f);

				for (int j = 0; j < 13; ++j)
				{
					var useSpread = Vector2.Lerp(lastSpread, spread, j / 14f);
					var dust = Dust.NewDustPerfect(Projectile.Center + useSpread, DustID.Electric, useSpread.DirectionTo(Projectile.Center), Scale: scale);
					dust.noGravity = true;
					dust.fadeIn = 0.3f + j / 14f * 1.2f;
				}
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float distanceSq = Projectile.Center.DistanceSQ(targetHitbox.Center());
			return distanceSq > MathF.Pow(Spread - 20, 2) && distanceSq < MathF.Pow(Spread + 20, 2);
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (Skill.Tree.TryGetNode(out ConcurrentBlasts blasts) && blasts.Allocated && target.TryGetGlobalNPC(out ConcurrentNPC cNPC))
			{
				if (cNPC.Vulnerable)
				{
					modifiers.FinalDamage *= ConcurrentBlasts.BonusDamage;
				}

				cNPC.ApplyBlastTimer();
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			//Passive synergies
			if (Skill.Tree.TryGetNode(out IgniteChance ignite) && ignite.Allocated && Main.rand.NextFloat() < 0.02f * ignite.Level)
			{
				target.AddBuff(BuffID.OnFire, 8 * 60);
			}

			if (Skill.Tree.TryGetNode(out ShockChance shock) && shock.Allocated && Main.rand.NextFloat() < 0.02f * shock.Level)
			{
				target.AddBuff(ModContent.BuffType<ShockDebuff>(), 8 * 60);
			}

			if (Skill.Tree.TryGetNode(out ThunderClaps thunderClaps) && thunderClaps.Allocated && Main.rand.NextFloat() < 0.05f * thunderClaps.Level)
			{
				Vector2 position = target.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(AreaOfEffect / 3f);

				var smallBlast = Projectile.NewProjectileDirect(Projectile.GetSource_OnHit(target), position, Vector2.Zero, Type, Projectile.damage, Projectile.knockBack, Projectile.owner, (int)NovaType);
				smallBlast.scale = 0.25f;
				smallBlast.netUpdate = true;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Vector2 position = Projectile.position - Main.screenPosition;
			float timer = Timer * 300f;
			var topLeft = new Vector3(position - new Vector2(timer), 0);
			Color color = Color.White;

			short[] indices = [0, 1, 2, 1, 3, 2];

			VertexPositionColorTexture[] vertices =
			[
				new(topLeft, color, new Vector2(0, 0)),
				new(topLeft + new Vector3(new Vector2(timer * 2, 0), 0), color, new Vector2(1, 0)),
				new(topLeft + new Vector3(new Vector2(0, timer * 2), 0), color, new Vector2(0, 1)),
				new(topLeft + new Vector3(new Vector2(timer * 2), 0), color, new Vector2(1, 1)),
			];

			Effect effect = ModContent.Request<Effect>($"{PoTMod.ModName}/Assets/Effects/RunestoneRing", AssetRequestMode.ImmediateLoad).Value;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			Matrix renderMatrix = view * projection;
			float opacity = MathF.Min(Projectile.timeLeft / 30f, 1);
			float lerpFactor = MathF.Pow(MathF.Sin((float)Main.timeForVisualEffects * 0.5f), 2);
			(Color, Color) colorPair = GetColorPair();
			var drawColor = Vector4.Lerp(colorPair.Item1.ToVector4(), colorPair.Item2.ToVector4(), lerpFactor);
			
			foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				effect.Parameters["baseColor"].SetValue(drawColor * opacity);
				effect.Parameters["uWorldViewProjection"].SetValue(renderMatrix);
				pass.Apply();

				Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2);
			}

			return false;
		}

		private (Color, Color) GetColorPair()
		{
			(Color, Color) pair = NovaType switch
			{
				NovaType.Fire => (new Color(255, 150, 150), new Color(255, 255, 150)),
				NovaType.Ice => (new Color(150, 150, 255), new Color(160, 180, 235)),
				NovaType.Lightning => (Color.LightBlue, Color.SkyBlue),
				_ => (new Color(255, 255, 255), new Color(150, 150, 255))
			};
			
			return pair;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(Projectile.scale);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			Projectile.scale = reader.ReadSingle();
		}
	}
}