using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.VanillaModifications;
using PathOfTerraria.Content.Buffs;

namespace PathOfTerraria.Content.Passives;

internal sealed class MinionAuraPlayer : ModPlayer
{
	public override void PostUpdateEquips()
	{
		// Player stat modifiers are reset before projectile AI, so aura bonuses must be resolved here.
		float damageAura = 0f;
		float manaRegenAura = 0f;

		foreach (Projectile projectile in Main.ActiveProjectiles)
		{
			if (!projectile.minion
				|| CustomProjectileSets.MultisegmentMinionProjectiles[projectile.type]
				|| Player.DistanceSQ(projectile.Center) >= PoTMod.NearbyDistanceSq
				|| !projectile.TryGetOwner(out Player owner))
			{
				continue;
			}

			PassiveTreePlayer passiveTree = owner.GetModPlayer<PassiveTreePlayer>();
			// Aura nodes do not stack; use the strongest nearby source instead of projectile update order.
			damageAura = Math.Max(damageAura, passiveTree.GetCumulativeValue<MinionDamageAuraPassive>());
			manaRegenAura = Math.Max(manaRegenAura, passiveTree.GetCumulativeValue<MinionManaRegenAuraPassive>());
		}

		if (damageAura > 0f)
		{
			Player.GetDamage(DamageClass.Generic) += damageAura / 100f;
			Player.AddBuff(ModContent.BuffType<MinionDamageAuraBuff>(), 2);
		}

		if (manaRegenAura > 0f)
		{
			Player.GetModPlayer<ManaRegenRework.ManaRegenPlayer>().ManaRegen.Flat += ManaRegenRework.ManaPerSecondToManaRegen(manaRegenAura);
			Player.AddBuff(ModContent.BuffType<MinionManaRegenAuraBuff>(), 2);
		}
	}
}
