using System.Collections.Generic;
using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.PassiveProjectiles;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives.Misc.Masteries;

internal sealed class MassDebilitationMastery : Passive
{
	public static readonly HashSet<int> ChaosDebuffs = [];

	public static bool RegisterChaosDebuff(int buffType)
	{
		if (buffType <= 0)
		{
			return false;
		}

		return ChaosDebuffs.Add(buffType);
	}

	public static bool RegisterChaosDebuff<T>() where T : ModBuff
	{
		return RegisterChaosDebuff(ModContent.BuffType<T>());
	}

	internal sealed class MassDebilitationPlayer : ModPlayer
	{
		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (HitWasChaosDamage(Player, target, item))
			{
				TrySpreadDebuffs(target);
			}
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (HitWasChaosDamage(target, proj))
			{
				TrySpreadDebuffs(target);
			}
		}

		private static bool HitWasChaosDamage(Player player, NPC target, Item item)
		{
			if (!target.TryGetGlobalNPC(out ElementalNPC elementalNpc))
			{
				return false;
			}

			ElementalContainer container = player.GetModPlayer<ElementalPlayer>().Container;
			return ElementalPlayer.DealsElementalDamage(ElementType.Chaos, container, elementalNpc.Container, item);
		}

		private static bool HitWasChaosDamage(NPC target, Projectile projectile)
		{
			if (!target.TryGetGlobalNPC(out ElementalNPC elementalNpc) || !projectile.TryGetGlobalProjectile(out ElementalProjectile elementalProjectile))
			{
				return false;
			}

			Item sourceItem = elementalProjectile.SourceItem > ItemID.None
				? ContentSamples.ItemsByType[elementalProjectile.SourceItem]
				: ContentSamples.ItemsByType[ItemID.None];
			return ElementalPlayer.DealsElementalDamage(ElementType.Chaos, elementalProjectile.Container, elementalNpc.Container, sourceItem);
		}

		private void TrySpreadDebuffs(NPC target)
		{
			if (target.life > 0 || target.lifeMax <= 5 || ChaosDebuffs.Count == 0 || !Player.GetModPlayer<PassiveTreePlayer>().HasNode<MassDebilitationMastery>())
			{
				return;
			}

			List<(int buff, int time)> debuffsToSpread = [];

			for (int i = 0; i < NPC.maxBuffs; ++i)
			{
				int buffType = target.buffType[i];
				int buffTime = target.buffTime[i];

				if (buffType > 0 && buffTime > 0 && ChaosDebuffs.Contains(buffType))
				{
					debuffsToSpread.Add((buffType, buffTime));
				}
			}

			if (debuffsToSpread.Count == 0)
			{
				return;
			}

			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.whoAmI == target.whoAmI || !npc.CanBeChasedBy() || npc.DistanceSQ(target.Center) > PoTMod.NearbyDistanceSq)
				{
					continue;
				}

				foreach ((int buff, int time) in debuffsToSpread)
				{
					npc.AddBuff(buff, time);
				}
			}
		}
	}
}

internal sealed class EmbraceChaosMastery : Passive
{
	private const float ReferenceMaximumLife = 400f;

	public override void BuffPlayer(Player player)
	{
		float currentLifeMissingRatio = 1f - Math.Clamp(player.statLife / (float)Math.Max(1, player.statLifeMax2), 0f, 1f);
		float maximumLifeMissingRatio = 1f - Math.Clamp(player.statLifeMax2 / ReferenceMaximumLife, 0f, 1f);
		float effectiveLowLifeRatio = Math.Max(currentLifeMissingRatio, maximumLifeMissingRatio);

		player.GetModPlayer<ElementalPlayer>().Container[ElementType.Chaos].Multiplier *= 1f + (Value / 100f) * effectiveLowLifeRatio;
	}
}

internal sealed class ChaosChaosChaosMastery : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<ChaosChaosChaosPlayer>().Enabled = true;
	}

	internal sealed class ChaosChaosChaosPlayer : ModPlayer
	{
		private const int CombatTimeout = 6 * 60;
		private const int RollInterval = 5 * 60;
		private const int RollBuffDuration = 4 * 60;
		private const int RollDebuffDuration = 3 * 60;
		private const float PositiveEffectMagnitude = 0.4f;

		public bool Enabled;

		private int _combatTimer;
		private int _rollTimer;
		private int _chaosDamageTimer;
		private int _speedTimer;
		private int _spinTimer;
		private int _gravityTimer;
		private int _invertedControlsTimer;

		public override void ResetEffects()
		{
			Enabled = false;

			if (_chaosDamageTimer > 0)
			{
				Player.GetModPlayer<ElementalPlayer>().Container[ElementType.Chaos].Multiplier *= 1f + PositiveEffectMagnitude;
			}

			if (_speedTimer > 0)
			{
				Player.moveSpeed += PositiveEffectMagnitude;
				Player.GetAttackSpeed(DamageClass.Generic) += PositiveEffectMagnitude;
			}

			if (_spinTimer > 0)
			{
				Player.fullRotation = MathHelper.WrapAngle(Player.fullRotation + 0.3f);
				Player.fullRotationOrigin = Player.Size * 0.5f;
			}
			else
			{
				Player.fullRotation = 0f;
			}

			if (_gravityTimer > 0)
			{
				Player.AddBuff(BuffID.Featherfall, 2);
				Player.AddBuff(BuffID.Gravitation, 2);
			}
		}

		public override void SetControls()
		{
			if (_invertedControlsTimer <= 0)
			{
				return;
			}

			(bool left, bool right) = (Player.controlLeft, Player.controlRight);
			(bool up, bool down) = (Player.controlUp, Player.controlDown);

			Player.controlLeft = right;
			Player.controlRight = left;
			Player.controlUp = down;
			Player.controlDown = up;
		}

		public override void OnHitAnything(float x, float y, Entity victim)
		{
			EnterCombat();
		}

		public override void OnHurt(Player.HurtInfo info)
		{
			EnterCombat();
		}

		public override void PostUpdate()
		{
			_combatTimer = Math.Max(0, _combatTimer - 1);
			_chaosDamageTimer = Math.Max(0, _chaosDamageTimer - 1);
			_speedTimer = Math.Max(0, _speedTimer - 1);
			_spinTimer = Math.Max(0, _spinTimer - 1);
			_gravityTimer = Math.Max(0, _gravityTimer - 1);
			_invertedControlsTimer = Math.Max(0, _invertedControlsTimer - 1);

			if (!Enabled)
			{
				_rollTimer = 0;
				return;
			}

			if (_combatTimer <= 0)
			{
				_rollTimer = 0;
				return;
			}

			_rollTimer--;

			if (_rollTimer <= 0)
			{
				RollChaosEffect();
				_rollTimer = RollInterval;
			}
		}

		private void EnterCombat()
		{
			bool enteringCombat = _combatTimer <= 0;
			_combatTimer = CombatTimeout;

			if (enteringCombat && Enabled)
			{
				RollChaosEffect();
				_rollTimer = RollInterval;
			}
		}

		private void RollChaosEffect()
		{
			int roll = Main.rand.Next(1, 7);
			string text = GetRollText(roll);

			switch (roll)
			{
				case 6:
					_chaosDamageTimer = Math.Max(_chaosDamageTimer, RollBuffDuration);
					break;
				case 5:
					_speedTimer = Math.Max(_speedTimer, RollBuffDuration);
					break;
				case 4:
					FireDevilsknives();
					break;
				case 3:
					_spinTimer = Math.Max(_spinTimer, RollDebuffDuration);
					break;
				case 2:
					_gravityTimer = Math.Max(_gravityTimer, RollDebuffDuration);
					break;
				default:
					_invertedControlsTimer = Math.Max(_invertedControlsTimer, RollDebuffDuration);
					break;
			}

			CombatText.NewText(Player.Hitbox, new Color(214, 90, 255), text);
		}

		private static string GetRollText(int roll)
		{
			return roll switch
			{
				6 => "I Can Do Anything!",
				5 => "The World Revolves",
				4 => "Devilsknife",
				3 => "Carousel",
				2 => "Gravity Games",
				_ => "CHAOS, CHAOS!"
			};
		}

		private void FireDevilsknives()
		{
			if (Main.myPlayer != Player.whoAmI)
			{
				return;
			}

			const int scytheCount = 8;
			const float scytheSpeed = 6;
			// Baseline projectile damage before player damage multipliers are applied.
			int damage = (int)Player.GetDamage(DamageClass.Generic).ApplyTo(50f);

			for (int i = 0; i < scytheCount; ++i)
			{
				float angle = MathHelper.TwoPi * i / scytheCount;
				Vector2 velocity = angle.ToRotationVector2() * scytheSpeed;

				Projectile.NewProjectile(Player.GetSource_Misc("ChaosChaosChaosMastery"), Player.Center, velocity, ModContent.ProjectileType<ChaosDevilsknifeScytheProjectile>(),
					damage, 1f, Player.whoAmI);
			}
		}
	}
}

internal sealed class ChaosDevilsknifeScytheProjectile : ModProjectile
{
	//TODO: Temp asset. Replace in the future
	public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.DemonScythe}";

	public override void SetDefaults()
	{
		Projectile.CloneDefaults(ProjectileID.DemonScythe);
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.penetrate = -1;
		Projectile.tileCollide = false;
		Projectile.timeLeft = 4 * 60;
		Projectile.DamageType = DamageClass.Generic;

		const float scytheScale = 1.5f;

		Projectile.scale = scytheScale;
		Projectile.Resize(
			(int)(Projectile.width * scytheScale),
			(int)(Projectile.height * scytheScale)
		);
	}

	public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
	{
		if (Projectile.TryGetGlobalProjectile(out ElementalProjectile elementalProjectile))
		{
			elementalProjectile.AddElementalValues((ElementType.Chaos, 0, 1f));
		}
	}
}
