using PathOfTerraria.Common.AccessorySlots;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.EnergyShield;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class EnergyShieldDamageMastery : Passive
{
	internal class EnergyShieldDamagePlayer : ModPlayer
	{
		public override void PostUpdateEquips()
		{
			if (!Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<EnergyShieldDamageMastery>(out float value))
			{
				return;
			}

			EnergyShieldPlayer shieldPlayer = Player.GetModPlayer<EnergyShieldPlayer>();
			if (shieldPlayer.CurrentEnergyShield <= 0)
			{
				return;
			}

			float damageBonus = shieldPlayer.CurrentEnergyShield * value / 10000f;
			if (damageBonus > 0f)
			{
				Player.GetDamage(DamageClass.Generic) += damageBonus;
			}
		}
	}
}

internal class EnergyShieldManaCostMastery : Passive
{
	internal class EnergyShieldManaCostPlayer : ModPlayer
	{
		public override void Load()
		{
			On_Player.CheckMana_int_bool_bool += HijackCheckMana;
		}

		private static bool HijackCheckMana(On_Player.orig_CheckMana_int_bool_bool orig, Player self, int amount, bool pay, bool blockQuickMana)
		{
			if (amount <= 0)
			{
				return orig(self, amount, pay, blockQuickMana);
			}

			if (!self.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<EnergyShieldManaCostMastery>(out float value))
			{
				return orig(self, amount, pay, blockQuickMana);
			}

			float shieldFraction = MathHelper.Clamp(value / 100f, 0f, 1f);
			if (shieldFraction <= 0f)
			{
				return orig(self, amount, pay, blockQuickMana);
			}

			EnergyShieldPlayer shieldPlayer = self.GetModPlayer<EnergyShieldPlayer>();
			int shieldCost = CalculateShieldCost(shieldPlayer, amount, shieldFraction);
			int remainingManaCost = amount - shieldCost;

			bool success = orig(self, remainingManaCost, pay, blockQuickMana);
			if (pay && success && shieldCost > 0)
			{
				shieldPlayer.ConsumeEnergyShield(shieldCost);
			}

			return success;
		}

		private static int CalculateShieldCost(EnergyShieldPlayer shieldPlayer, int amount, float shieldFraction)
		{
			if (shieldPlayer.CurrentEnergyShield <= 0)
			{
				return 0;
			}

			int desiredCost = (int)MathF.Floor(amount * shieldFraction);
			if (desiredCost <= 0)
			{
				return 0;
			}

			int available = (int)MathF.Floor(shieldPlayer.CurrentEnergyShield);
			return Math.Min(desiredCost, available);
		}
	}
}

internal class UniqueEnergyShieldMastery : Passive
{
	public override void BuffPlayer(Player player)
	{
		if (!player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<UniqueEnergyShieldMastery>(out float value))
		{
			return;
		}

		int uniqueCount = CountUniqueEquipped(player);
		if (uniqueCount <= 0 || value <= 0)
		{
			return;
		}

		player.GetModPlayer<EnergyShieldPlayer>().AddGlobalIncreasedEnergyShield(uniqueCount * value);
	}

	private static int CountUniqueEquipped(Player player)
	{
		int count = 0;

		for (int i = 0; i <= (int)VanillaEquipSlots.Accessory7; i++)
		{
			if (IsUniqueItem(player.armor[i]))
			{
				count++;
			}
		}

		if (IsUniqueItem(player.inventory[0]))
		{
			count++;
		}

		return count;
	}

	private static bool IsUniqueItem(Item item)
	{
		return item is { IsAir: false } && (item.GetStaticData().IsUnique || item.GetInstanceData().Rarity == ItemRarity.Unique);
	}
}
