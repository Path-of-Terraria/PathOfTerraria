using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Skills.Magic;
using System.Collections.Generic;
using PathOfTerraria.Common.Utilities.Extensions;

namespace PathOfTerraria.Content.Passives.Misc.Masteries;

internal class TitanMastery : Passive { }

internal class ColossusMastery : Passive { }

internal class ArcanistMastery : Passive { }

internal class ConjurerMastery : Passive { }

internal class HunterMastery : Passive { }

internal class DexterityMarksmanMastery : Passive { }

internal class GoliathMastery : Passive { }

internal class ArchmageMastery : Passive
{
	internal const int ManaPerNova = 100;
	private const int BaseNovaDamage = 20;
	private const float NovaKnockback = 2f;

	internal class ArchmagePlayer : ModPlayer
	{
		private int _manaSpent;

		public override void Load()
		{
			On_Player.CheckMana_int_bool_bool += HijackCheckMana;
		}

		private static bool HijackCheckMana(On_Player.orig_CheckMana_int_bool_bool orig, Player self, int amount, bool pay, bool blockQuickMana)
		{
			bool success = orig(self, amount, pay, blockQuickMana);

			if (!success || !pay || amount <= 0)
			{
				return success;
			}

			if (!self.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<ArchmageMastery>(out float threshold))
			{
				return success;
			}

			if (self.GetModPlayer<AttributesPlayer>().Intelligence < threshold)
			{
				return success;
			}

			self.GetModPlayer<ArchmagePlayer>().AddManaSpent(self, amount);

			return success;
		}

		public override void OnConsumeMana(Item item, int manaConsumed)
		{
			if (manaConsumed <= 0)
			{
				return;
			}

			if (!Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<ArchmageMastery>(out float threshold))
			{
				return;
			}

			if (Player.GetModPlayer<AttributesPlayer>().Intelligence < threshold)
			{
				return;
			}

			AddManaSpent(Player, manaConsumed);
		}

		private void AddManaSpent(Player player, int amount)
		{
			_manaSpent += amount;

			while (_manaSpent >= ManaPerNova)
			{
				_manaSpent -= ManaPerNova;

				if (Main.myPlayer == player.whoAmI)
				{
					SpawnNova(player);
				}
			}
		}

		private static void SpawnNova(Player player)
		{
			int baseDamage = player.HeldItem.damage > 0 ? player.HeldItem.damage : BaseNovaDamage;
			int damage = (int)player.GetDamage(DamageClass.Magic).ApplyTo(baseDamage);
			Nova.NovaType novaType = GetNovaType(player);

			int projType = ModContent.ProjectileType<Nova.NovaProjectile>();
			int proj = Projectile.NewProjectile(player.GetSource_Misc("ArchmageNova"), player.Center, Vector2.Zero, projType, damage, NovaKnockback, player.whoAmI, (int)novaType);
			Projectile projectile = Main.projectile[proj];
			projectile.netUpdate = true;

			ElementType elementType = GetElementType(novaType);
			if (elementType != ElementType.None)
			{
				projectile.GetGlobalProjectile<ElementalProjectile>().Container.AddElementalValues((elementType, 0, 1));
			}
		}

		private static Nova.NovaType GetNovaType(Player player)
		{
			if (ElementalWeaponSets.GetElementalProportions(player.HeldItem.type, out Dictionary<ElementType, float> elements))
			{
				ElementType bestType = ElementType.None;
				float bestValue = 0f;

				foreach ((ElementType type, float value) in elements)
				{
					if (value > bestValue && type is ElementType.Fire or ElementType.Cold or ElementType.Lightning)
					{
						bestType = type;
						bestValue = value;
					}
				}

				if (bestType != ElementType.None)
				{
					return bestType switch
					{
						ElementType.Fire => Nova.NovaType.Fire,
						ElementType.Cold => Nova.NovaType.Ice,
						ElementType.Lightning => Nova.NovaType.Lightning,
						_ => Nova.NovaType.Normal
					};
				}
			}

			return Main.rand.Next(3) switch
			{
				0 => Nova.NovaType.Fire,
				1 => Nova.NovaType.Ice,
				_ => Nova.NovaType.Lightning
			};
		}

		private static ElementType GetElementType(Nova.NovaType novaType)
		{
			return novaType switch
			{
				Nova.NovaType.Fire => ElementType.Fire,
				Nova.NovaType.Ice => ElementType.Cold,
				Nova.NovaType.Lightning => ElementType.Lightning,
				_ => ElementType.None
			};
		}
	}
}

internal class TrueshotMastery : Passive
{
	internal class TrueshotProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		private const float HomingRange = 800f;
		private const float HomingTurnSpeed = 0.015f;

		public override void AI(Projectile projectile)
		{
			if (!projectile.friendly || projectile.hostile || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
			{
				return;
			}

			Player player = Main.player[projectile.owner];
			if (!player.active || player.dead)
			{
				return;
			}

			if (!player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<TrueshotMastery>(out float threshold))
			{
				return;
			}

			if (player.GetModPlayer<AttributesPlayer>().Dexterity < threshold)
			{
				return;
			}

			NPC target = FindTarget(projectile);
			if (target is null)
			{
				return;
			}

			float speed = projectile.velocity.Length();
			if (speed <= 0.01f)
			{
				return;
			}

			Vector2 targetVelocity = Vector2.Normalize(target.Center - projectile.Center) * speed;
			if (Vector2.Dot(Vector2.Normalize(projectile.velocity), targetVelocity) > 0)
			{
				projectile.velocity = projectile.velocity.RotateTowards(targetVelocity, HomingTurnSpeed);
			}
		}

		private static NPC FindTarget(Projectile projectile)
		{
			NPC closestNpc = null;
			float closestDistanceSq = HomingRange * HomingRange;

			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (!npc.CanBeChasedBy(projectile))
				{
					continue;
				}

				float distanceSq = projectile.DistanceSQ(npc.Center);
				if (distanceSq < closestDistanceSq)
				{
					closestNpc = npc;
					closestDistanceSq = distanceSq;
				}
			}

			return closestNpc;
		}
	}
}

internal sealed class AttributeMasteryPlayer : ModPlayer
{
	public override void UpdateEquips()
	{
		PassiveTreePlayer passivePlayer = Player.GetModPlayer<PassiveTreePlayer>();
		AttributesPlayer attributes = Player.GetModPlayer<AttributesPlayer>();

		if (passivePlayer.TryGetCumulativeValue<TitanMastery>(out float strengthBonus))
		{
			attributes.Strength *= 1 + strengthBonus / 100f;
		}

		if (passivePlayer.TryGetCumulativeValue<ArcanistMastery>(out float intelligenceBonus))
		{
			attributes.Intelligence *= 1 + intelligenceBonus / 100f;
		}

		if (passivePlayer.TryGetCumulativeValue<HunterMastery>(out float dexterityBonus))
		{
			attributes.Dexterity *= 1 + dexterityBonus / 100f;
		}

		if (passivePlayer.TryGetCumulativeValue<ColossusMastery>(out _))
		{
			float knockbackBonus = attributes.Strength / 1000f;

			if (knockbackBonus > 0f)
			{
				Player.GetKnockback(DamageClass.Generic) += knockbackBonus;
			}
		}

		if (passivePlayer.TryGetCumulativeValue<ConjurerMastery>(out _))
		{
			float areaBonus = attributes.Intelligence / 1000f;

			if (areaBonus > 0f)
			{
				// TODO: Area modifiers for this mastery to work
				//Player.GetModPlayer<SkillCombatPlayer>().GlobalBuff.AreaOfEffect += areaBonus;
			}
		}

		if (passivePlayer.TryGetCumulativeValue<DexterityMarksmanMastery>(out _))
		{
			float speedBonus = attributes.Dexterity / 1000f;

			if (speedBonus > 0f)
			{
				Player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ProjectileSpeed += speedBonus;
			}
		}

		if (passivePlayer.TryGetCumulativeValue<GoliathMastery>(out float goliathThreshold) && attributes.Strength >= goliathThreshold)
		{
			float damageBonus = Player.statLifeMax2 / 10000f;

			if (damageBonus > 0f)
			{
				Player.GetDamage(DamageClass.Melee) += damageBonus;
			}
		}
	}
}