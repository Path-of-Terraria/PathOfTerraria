using FunnyExperience.Core.Systems;

namespace FunnyExperience.Content.Items.Gear.Affixes.ArmorAffixes
{
	internal class ManaAffix : Affix
	{
		public ManaAffix() : base()
		{
			possibleTypes = GearType.Helmet | GearType.Chestplate | GearType.Leggings;
		}

		public override string GetTooltip(Player player, Gear gear)
		{
			return $"+{5 + (int)(value * 20) + gear.power / 20} Maximum Mana";
		}

		public override void BuffPassive(Player player, Gear gear)
		{
			player.statManaMax2 += 5 + (int)(value * 20) + gear.power / 20;
		}
	}

	internal class ManaRegenAffix : Affix
	{
		public ManaRegenAffix() : base()
		{
			possibleTypes = GearType.Helmet | GearType.Chestplate | GearType.Leggings;
		}

		public override string GetTooltip(Player player, Gear gear)
		{
			return $"+{1 + (int)(value * 4) + gear.power / 40} Mana Regeneration";
		}

		public override void BuffPassive(Player player, Gear gear)
		{
			player.manaRegen += 1 + (int)(value * 4) + gear.power / 40;
		}
	}

	internal class ManaPotionPowerAffix : Affix
	{
		public ManaPotionPowerAffix() : base()
		{
			possibleTypes = GearType.Helmet | GearType.Chestplate | GearType.Leggings;
		}

		public override string GetTooltip(Player player, Gear gear)
		{
			return $"Mana potions restore {10 + (int)(value * 10) + gear.power / 20} more Mana";
		}

		public override void BuffPassive(Player player, Gear gear)
		{
			player.GetModPlayer<PotionSystem>().manaPower += 10 + (int)(value * 10) + gear.power / 20;
		}
	}

	internal class ManaPotionCapAffix : Affix
	{
		public ManaPotionCapAffix() : base()
		{
			possibleTypes = GearType.Helmet | GearType.Chestplate | GearType.Leggings;
		}

		public override string GetTooltip(Player player, Gear gear)
		{
			return $"You can hold {1 + (int)Math.Round(value, MidpointRounding.ToEven) + gear.power / 100} additional mana potions";
		}

		public override void BuffPassive(Player player, Gear gear)
		{
			player.GetModPlayer<PotionSystem>().maxMana += 1 + (int)Math.Round(value, MidpointRounding.ToEven) + gear.power / 100;
		}
	}

	internal class ManaPotionCooldownAffix : Affix
	{
		public ManaPotionCooldownAffix() : base()
		{
			possibleTypes = GearType.Helmet | GearType.Chestplate | GearType.Leggings;
			requiredInfluence = GearInfluence.Solar;
		}

		public override string GetTooltip(Player player, Gear gear)
		{
			return $"Mana potions are ready {0.5f + value * 0.5f} seconds sooner";
		}

		public override void BuffPassive(Player player, Gear gear)
		{
			player.GetModPlayer<PotionSystem>().manaDelay -= (int)(60 * (0.5f + value * 0.5f));
		}
	}
}
