using System;
using PathOfTerraria.Content.Buffs.ElementalBuffs;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal class HealOnKillingBurningEnemiesAffix : ItemAffix
{
	private sealed class HealOnKillingBurningEnemiesAffixGlobalNpc : GlobalNPC
	{
		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			base.OnHitByProjectile(npc, projectile, hit, damageDone);
			
			if (!npc.HasBuff(BuffID.OnFire) && !npc.HasBuff(BuffID.OnFire3) && !npc.HasBuff(ModContent.BuffType<IgnitedDebuff>()) || npc.life > 0)
			{
				return;
			}

			Player owner = Main.player[projectile.owner];
			float value = owner.GetModPlayer<AffixPlayer>().StrengthOf<HealOnKillingBurningEnemiesAffix>();

			if (value != 0)
			{
				owner.Heal((int)(value * 2));
			}
		}
	}
}

internal class IgniteSpreadAffix : ItemAffix
{
	private sealed class IgniteSpreadAffixGlobalNpc : GlobalNPC
	{
		public override void OnKill(NPC npc)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return; //TODO: Figure this out later
			}

			if (!npc.TryGetGlobalNPC(out IgnitedNPC ignited) || ignited.Stacks.Count == 0)
			{
				return;
			}

			Player player = Main.player[npc.lastInteraction];
			if (player is not { active: true })
			{
				return;
			}

			float spreadTiles = player.GetModPlayer<AffixPlayer>().StrengthOf<IgniteSpreadAffix>();
			if (spreadTiles <= 0f)
			{
				return;
			}

			IgnitedStack sourceStack = ignited.Stacks[0];
			if (sourceStack.BaseDamage <= 0 || sourceStack.Time <= 0)
			{
				return;
			}

			float spreadRadius = spreadTiles * 16f;
			float spreadRadiusSq = spreadRadius * spreadRadius;
			int spreadTime = Math.Max(1, sourceStack.Time - 1);
			int baseDamage = sourceStack.BaseDamage;

			foreach (NPC other in Main.ActiveNPCs)
			{
				if (other.whoAmI == npc.whoAmI || !other.CanBeChasedBy())
				{
					continue;
				}

				if (other.DistanceSQ(npc.Center) > spreadRadiusSq)
				{
					continue;
				}

				IgnitedDebuff.ApplyTo(npc, other, baseDamage, spreadTime);

				if (other.TryGetGlobalNPC(out IgnitedNPC otherIgnited))
				{
					otherIgnited.LastTickCount = ignited.LastTickCount;
				}
			}
		}
	}

	protected override AffixTooltipLine CreateDefaultTooltip(Player player, Item item)
	{
		return base.CreateDefaultTooltip(player, item) with { Value = (int)Math.Round(Value) };
	}
}

internal class LifeOnKillAffix : ItemAffix
{
	private sealed class LifeOnKillGlobalNPC : GlobalNPC
	{
		public override void OnKill(NPC npc)
		{
			Player player = Main.player[npc.lastInteraction];
			
			if (player == null)
			{
				return;
			}
			
			//Just in case there's ever more than 1 of these affixes enabled (right now its only on weapons so there *should* be one.)
			float totalLifeOnKill = player.GetModPlayer<AffixPlayer>().StrengthOf<LifeOnKillAffix>();

			if (totalLifeOnKill == 0)
			{
				return;
			}

			player.Heal((int)Math.Round(totalLifeOnKill));
		}
	}

	protected override AffixTooltipLine CreateDefaultTooltip(Player player, Item item)
	{
		return base.CreateDefaultTooltip(player, item) with { Value = (int)Math.Round(Value) };
	}
}

internal class ManaOnKillAffix : ItemAffix
{
	private sealed class ManaOnKillGlobalNPC : GlobalNPC
	{
		public override void OnKill(NPC npc)
		{
			Player player = Main.player[npc.lastInteraction];
			
			if (player == null)
			{
				return;
			}

			//Just in case there's ever more than 1 of these affixes enabled (right now its only on weapons so there *should* be one.)
			float totalManaOnKill = player.GetModPlayer<AffixPlayer>().StrengthOf<ManaOnKillAffix>();

			if (totalManaOnKill == 0)
			{
				return;
			}

			int manaToRestore = (int)Math.Round(totalManaOnKill);
			player.statMana = Math.Min(player.statMana + manaToRestore, player.statManaMax2);
			
			//Add combat text for mana restoration value
			player.ManaEffect(manaToRestore);
		}
	}

	protected override AffixTooltipLine CreateDefaultTooltip(Player player, Item item)
	{
		return base.CreateDefaultTooltip(player, item) with { Value = (int)Math.Round(Value) };
	}
}

