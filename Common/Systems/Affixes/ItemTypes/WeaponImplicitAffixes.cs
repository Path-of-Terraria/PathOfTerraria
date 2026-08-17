using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Core.Items;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal static class WeaponImplicitStrength
{
	public static float Get<T>(Item item) where T : ItemAffix
	{
		if (item is null || item.IsAir || !item.TryGetGlobalItem(out PoTInstanceItemData data))
		{
			return 0f;
		}

		float result = 0f;
		foreach (ItemAffix affix in data.Affixes)
		{
			if (affix is T)
			{
				result += affix.Value;
			}
		}

		return result;
	}
}

internal sealed class SwordCriticalMultiplierImplicitAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.CriticalMultiplier *= 1f + Value / 100f;
	}
}

internal sealed class BattleaxeExecuteDamageImplicitAffix : ItemAffix
{
	private sealed class ExecuteDamagePlayer : ModPlayer
	{
		public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
		{
			float strength = WeaponImplicitStrength.Get<BattleaxeExecuteDamageImplicitAffix>(item);
			if (strength > 0f && target.life * 2 <= target.lifeMax)
			{
				modifiers.FinalDamage *= 1f + strength / 100f;
			}
		}
	}
}

internal sealed class BowChargedShotDamageImplicitAffix : ItemAffix;

internal sealed class BoomerangReturnDamageImplicitAffix : ItemAffix;

internal sealed class JavelinDashDamageImplicitAffix : ItemAffix;

internal sealed class StaffChargeSpeedImplicitAffix : ItemAffix;

internal sealed class TomeManaOnKillImplicitAffix : ItemAffix
{
	public TomeManaOnKillImplicitAffix()
	{
		Round = true;
	}

	private sealed class TomeManaOnKillGlobalProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		private int _manaOnKill;

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_ItemUse_WithAmmo { Item: Item item })
			{
				_manaOnKill = (int)MathF.Round(WeaponImplicitStrength.Get<TomeManaOnKillImplicitAffix>(item));
			}
		}

		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (_manaOnKill <= 0 || target.life > 0 || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
			{
				return;
			}

			Player player = Main.player[projectile.owner];
			if (!player.active)
			{
				return;
			}

			int restored = Math.Min(_manaOnKill, player.statManaMax2 - player.statMana);
			if (restored <= 0)
			{
				return;
			}

			player.statMana += restored;
			player.ManaEffect(restored);
		}
	}
}

internal sealed class WandFlurryDamageImplicitAffix : ItemAffix;

internal sealed class WarShieldCounterDamageImplicitAffix : ItemAffix;

internal sealed class WhipTipDamageImplicitAffix : ItemAffix;
