using PathOfTerraria.Common;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.UI.Hotbar;
using PathOfTerraria.Content.Gores.Misc;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.SkillPassives.RainOfArrowsPassives;
using PathOfTerraria.Content.Skills.Ranged.RainOfArrowsVFX;
using PathOfTerraria.Content.SkillSpecials.RainOfArrowsSpecials;
using PathOfTerraria.Content.SkillTrees;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Skills.Ranged;

public class RainOfArrows : Skill
{
	private static readonly HashSet<int> WeaponBlacklist = [ItemID.Harpoon, ItemID.Sandgun];

	public override int MaxLevel => 3;

	public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = 8 * 60;
		ManaCost = 20;
		Duration = 0;
		WeaponType = ItemType.Ranged;
	}

	public override void UseSkill(Player player)
	{
		if (!player.PickAmmo(player.HeldItem, out int projToShoot, out float speed, out int damage, out float knockBack, out int ammo, true))
		{
			return;
		}

		base.UseSkill(player);

		if (player.HeldItem.ModItem is not null)
		{
			Vector2 throwaway = Vector2.Zero;
			ItemLoader.ModifyShootStats(player.HeldItem, player, ref throwaway, ref throwaway, ref projToShoot, ref damage, ref knockBack);
		}

		damage = GetTotalDamage(damage * (1 + Level * 0.15f));

		if (projToShoot <= 0)
		{
			return;
		}

		var src = new EntitySource_ItemUse_WithAmmo(player, player.HeldItem, ammo);

		if (!player.HasSkillSpecialization<RainOfArrows, PiercingPrecision>())
		{
			int repeats = 6 + Level * 2;
			bool shattering = player.HasTreePassive<RainOfArrowsTree, ShatteringArrows>();

			if (shattering)
			{
				repeats *= 2;
				damage = (int)(damage * 0.7f);
				repeats += player.GetPassiveStrength<RainOfArrowsTree, ConcussiveBurst>();
			}

			for (int i = 0; i < repeats; ++i)
			{
				Vector2 pos = player.Center + new Vector2(Main.rand.NextFloat(-16, 16), Main.rand.NextFloat(-10, 10));
				Vector2 velocity = Vector2.UnitY.RotatedByRandom(0.6f) * -10 * Main.rand.NextFloat(0.9f, 1.1f);
				int proj = Projectile.NewProjectile(src, pos, velocity, projToShoot, damage, knockBack, player.whoAmI);

				Projectile projectile = Main.projectile[proj];
				projectile.GetGlobalProjectile<RainProjectile>().SetRainProjectile(Main.projectile[proj], i == 0);

				if (shattering)
				{
					projectile.scale *= 0.5f;
				}
			}
		}
		else
		{
			int type = ModContent.ProjectileType<PiercingPrecision.PrecisionSpawnerProjectile>();
			Projectile.NewProjectile(src, player.Center, Vector2.Zero, type, damage, knockBack, player.whoAmI, projToShoot);
		}
	}

	public override bool CanUseSkill(Player player)
	{
		return base.CanUseSkill(player) && player.HeldItem.CountsAsClass(DamageClass.Ranged) && Main.myPlayer == player.whoAmI && 
			!WeaponBlacklist.Contains(player.HeldItem.type) && !player.HeldItem.consumable;
	}

	public override void ModifyTooltips(List<NewHotbar.SkillTooltip> tooltips)
	{
		tooltips.Remove(tooltips.FirstOrDefault(x => x.Name == "WeaponType"));
		NewHotbar.SkillTooltip tooltip = tooltips.First(x => x.Name == "Description");
		string text = Language.GetText($"Mods.{PoTMod.ModName}.Skills.NeedsWeapon").Format(WeaponType.LocalizeText().ToLower());

		if (Main.LocalPlayer.HeldItem.CountsAsClass(DamageClass.Ranged))
		{
			text = Language.GetText($"Mods.{PoTMod.ModName}.Skills.BaseDamage").Format(Main.LocalPlayer.HeldItem.damage);
		}

		tooltips.Add(new NewHotbar.SkillTooltip("ExtraDamage", text, tooltip.Slot + 0.1f));
	}

	internal class RainProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		internal bool IsRainProjectile = false;
		internal Vector2 RainTarget = Vector2.Zero;
		internal Vector2 PoisonOrientation = Vector2.Zero;

		private short _rainTimer = 0;
		private float _originalMagnitude = 0f;
		private bool _poison = false;
		private bool _isFirst = false;
		private bool _piercingProj = false;
		private bool _ghost = false;

		public override bool PreAI(Projectile projectile)
		{
			if (IsRainProjectile)
			{
				_rainTimer++;

				if (_poison)
				{
					Vector2 vel = projectile.velocity.SafeNormalize(Vector2.Zero);
					PoisonOrientation = NaturesBarrageBatching.GetTargetBasedOnNormalizedVelocity(vel.RotatedBy(projectile.rotation));

					if (Main.rand.NextFloat() < 0.03f && !Main.dedServ)
					{
						Gore.NewGore(projectile.GetSource_FromAI(), projectile.Center, projectile.velocity, ModContent.GoreType<PoisonBubble>());
					}
				}

				if (_rainTimer < 60f)
				{
					projectile.Opacity = 1 - _rainTimer / 60f;
				}
				else if (_rainTimer > 60f)
				{
					projectile.Opacity = MathHelper.Lerp(projectile.Opacity, 1f, 0.1f);

					if (projectile.GetOwner().HasTreePassive<RainOfArrowsTree, TargetLock>())
					{
						TargetLockHoming(projectile);
					}
				}
				else
				{
					if (projectile.GetOwner().HasSkillSpecialization<RainOfArrows, ExplosiveVolley>())
					{
						ExplosiveReposition(projectile);
					}
					else
					{
						NormalReposition(projectile);
					}

					if (_ghost)
					{
						for (int i = 0; i < 9; ++i)
						{
							float scale = Main.rand.NextFloat(1, 2);
							Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Ghost, projectile.velocity.X, 6, 0, Scale: scale);
						}

						SoundEngine.PlaySound(SoundID.DD2_GhastlyGlaiveImpactGhost, projectile.Center);
					}
				}

				if (projectile.Center.Y > RainTarget.Y)
				{
					projectile.tileCollide = true;
				}
			}

			return true;
		}

		private void TargetLockHoming(Projectile projectile)
		{
			NPC closest = null;

			foreach (NPC npc in Main.ActiveNPCs)
			{
				float curDist = npc.DistanceSQ(projectile.Center);

				if (npc.CanBeChasedBy() && curDist < 600 * 600 && (closest is null || curDist < closest.DistanceSQ(projectile.Center)))
				{
					closest = npc;
				}
			}

			if (closest != null)
			{
				projectile.velocity += projectile.DirectionTo(closest.Center) * new Vector2(0.2f, 0.005f);

				if (projectile.velocity.LengthSquared() > _originalMagnitude * _originalMagnitude)
				{
					projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * _originalMagnitude;
				}
			}
		}

		private void ExplosiveReposition(Projectile projectile)
		{
			if (projectile.owner == Main.myPlayer)
			{
				Vector2 offset = -Vector2.UnitY.RotatedByRandom(0.01f);
				projectile.Center = RainTarget + offset * 200;

				float originalLength = projectile.velocity.Length();
				Vector2 velocity = new Vector2(0, WorldGen.genRand.NextFloat(_originalMagnitude * 0.8f, _originalMagnitude * 1.5f)).RotatedByRandom(MathHelper.PiOver2 * 0.8f);
				projectile.velocity = velocity;

				// Stop the projectiles from going through tiles at all
				RainTarget.Y -= 300;

				if (Main.netMode != NetmodeID.SinglePlayer)
				{
					NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile.whoAmI);
				}
			}

			for (int i = 0; i < 3; ++i)
			{
				Dust.NewDust(projectile.Center, 1, 1, DustID.AncientLight, 0, 2);
			}

			if (_isFirst)
			{
				bool coldBlast = projectile.GetOwner().HasTreePassive<RainOfArrowsTree, ColdBlast>();

				if (Main.myPlayer == projectile.owner)
				{
					int dmg = (int)(projectile.damage * 1.5f);
					int type = ModContent.ProjectileType<ExplosionHitboxFriendly>();
					int proj = Projectile.NewProjectile(projectile.GetSource_FromAI(), projectile.Center, Vector2.Zero, type, dmg, 15, Main.myPlayer, 90, 90);

					if (coldBlast)
					{
						Projectile newProj = Main.projectile[proj];
						newProj.localAI[0] = BuffID.Chilled;
						newProj.localAI[1] = 120;
						newProj.GetGlobalProjectile<ElementalProjectile>().ColdDamage.ApplyOverride(0, 1);
					}
				}

				ExplosionHitbox.VFXPackage package = new(1);

				if (coldBlast)
				{
					package = new ExplosionHitbox.VFXPackage(1, 20, 16, true, null, DustID.Smoke, DustID.IceTorch);
				}

				ExplosionHitbox.VFX(projectile, package);
			}
		}

		private void NormalReposition(Projectile projectile)
		{
			if (Main.myPlayer != projectile.owner)
			{
				return;
			}

			Vector2 offset = -Vector2.UnitY.RotatedByRandom(_piercingProj ? 0.2f : 0.6f);
			projectile.Center = RainTarget + offset * 400;
			projectile.velocity = -offset * _originalMagnitude;

			for (int i = 0; i < 3; ++i)
			{
				Vector2 velocity = -projectile.velocity.RotatedByRandom(0.2f) * 0.3f;
				Dust.NewDust(projectile.Center, 1, 1, DustID.AncientLight, velocity.X, velocity.Y);
			}

			projectile.netUpdate = true;
		}

		public override void OnKill(Projectile projectile, int timeLeft)
		{
			if (!IsRainProjectile || !_poison)
			{
				return;
			}

			if (projectile.GetOwner().HasTreePassive<RainOfArrowsTree, FesteringSpores>())
			{
				float count = projectile.GetOwner().GetPassiveStrength<RainOfArrowsTree, MoldColony>() * 0.05f;

				if (Main.rand.NextFloat() < 0.05f + count && Main.myPlayer == projectile.owner)
				{
					int damage = (int)(projectile.damage * 1.5f);
					int type = ModContent.ProjectileType<FesteringSpores.FesteringSporesProj>();
					Projectile.NewProjectile(projectile.GetSource_Death(), projectile.Center, Vector2.Zero, type, damage, 8f, projectile.owner);
				}
			}

			if (projectile.GetOwner().GetPassiveStrength<RainOfArrowsTree, LingeringPoison>() > 0 && Main.myPlayer == projectile.owner)
			{
				int type = ModContent.ProjectileType<LingeringPoison.SporeCloud>();
				Vector2 velocity = Main.rand.NextVector2Circular(0.6f, 0.6f);
				Projectile.NewProjectile(projectile.GetSource_Death(), projectile.Center, velocity, type, projectile.damage, 8f, projectile.owner);
			}
		}

		internal void SetRainProjectile(Projectile projectile, bool isFirst = false, Vector2? overrideTarget = null)
		{
			IsRainProjectile = true;
			RainTarget = overrideTarget ?? Main.MouseWorld;

			_originalMagnitude = projectile.velocity.Length();
			_rainTimer = 0;
			_isFirst = isFirst;

			Player player = projectile.GetOwner();
			SkillSpecial spec = player.GetSkillSpecialization<RainOfArrows>();

			if (spec is NaturesBarrage)
			{
				_poison = true;
			}
			else if (spec is PiercingPrecision)
			{
				_piercingProj = true;
			}

			projectile.tileCollide = false;

			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile.whoAmI);
			}
		}

		public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (_piercingProj)
			{
				modifiers.ScalingArmorPenetration += 0.3f + projectile.GetOwner().GetPassiveStrength<RainOfArrowsTree, SharpenedTips>() * 0.1f;
			}
		}

		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!IsRainProjectile)
			{
				return;
			}

			if (_poison)
			{
				target.AddBuff(BuffID.Poisoned, 60 * 4);
			}

			Player player = Main.player[projectile.owner];
			ref Vector2? vine = ref target.GetGlobalNPC<CreepingVines.VineNPC>().Vined;

			if (player.HasTreePassive<RainOfArrowsTree, SlicingShrapnel>())
			{
				target.AddBuff(BuffID.Bleeding, 60 * 3);
			}

			if (player.HasTreePassive<RainOfArrowsTree, CreepingVines>() && !target.immortal && !vine.HasValue)
			{
				vine = projectile.Center;
			}

			if (player.HasTreePassive<RainOfArrowsTree, Ghostfire>() && !_ghost && Main.rand.NextFloat() < 0.1f)
			{
				Vector2 pos = player.Center + new Vector2(Main.rand.NextFloat(-16, 16), Main.rand.NextFloat(-10, 10));
				Vector2 velocity = Vector2.UnitY.RotatedByRandom(0.6f) * -10 * Main.rand.NextFloat(0.9f, 1.1f);
				int proj = Projectile.NewProjectile(projectile.GetSource_Death(), pos, velocity, projectile.type, projectile.damage, projectile.knockBack, player.whoAmI);
				Projectile newProj = Main.projectile[proj];
				RainProjectile rain = newProj.GetGlobalProjectile<RainProjectile>();
				
				rain.SetRainProjectile(Main.projectile[proj], false, projectile.Center);
				rain._ghost = true;
			}
		}

		public override bool PreDraw(Projectile projectile, ref Color lightColor)
		{
			if (_poison && !NaturesBarrageBatching.Batching)
			{
				NaturesBarrageBatching.AddCache(NaturesBarrageBatching.ArrowRainShaderType.Barrage, projectile.whoAmI);
				return false;
			}

			return true;
		}

		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
		{
			bitWriter.WriteBit(IsRainProjectile);

			if (IsRainProjectile)
			{
				binaryWriter.WriteVector2(RainTarget);
				binaryWriter.Write((Half)_originalMagnitude);
				bitWriter.WriteBit(_poison);
				bitWriter.WriteBit(_isFirst);
				bitWriter.WriteBit(_piercingProj);
				bitWriter.WriteBit(_ghost);
			}
		}

		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
		{
			IsRainProjectile = bitReader.ReadBit();

			if (IsRainProjectile)
			{
				RainTarget = binaryReader.ReadVector2();
				_originalMagnitude = (float)binaryReader.ReadHalf();
				_poison = bitReader.ReadBit();
				_isFirst = bitReader.ReadBit();
				_piercingProj = bitReader.ReadBit();
				_ghost = bitReader.ReadBit();
			}
		}
	}
}