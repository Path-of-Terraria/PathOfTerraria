using FunnyExperience.Core.Systems;

namespace FunnyExperience.Content.Items.Gear.Affixes.ArmorAffixes
{
	internal class LifeAffix : Affix
	{
		public LifeAffix() : base()
		{
			PossibleTypes = GearType.Helmet | GearType.Chestplate | GearType.Leggings;
		}

		public override string GetTooltip(Player player, Gear gear)
		{
			return $"+{10 + (int)(Value * 30) + gear.Power / 10} Maximum Life";
		}

		public override void BuffPassive(Player player, Gear gear)
		{
			player.statLifeMax2 += 10 + (int)(Value * 30) + gear.Power / 10;
		}
	}

	internal class LifeRegenAffix : Affix
	{
		public LifeRegenAffix() : base()
		{
			PossibleTypes = GearType.Helmet | GearType.Chestplate | GearType.Leggings;
		}

		public override string GetTooltip(Player player, Gear gear)
		{
			return $"+{1 + (int)(Value * 4) + gear.Power / 40} Life Regeneration";
		}

		public override void BuffPassive(Player player, Gear gear)
		{
			player.lifeRegen += 1 + (int)(Value * 4) + gear.Power / 40;
		}
	}

	internal class LifePotionPowerAffix : Affix
	{
		public LifePotionPowerAffix() : base()
		{
			PossibleTypes = GearType.Helmet | GearType.Chestplate | GearType.Leggings;
		}

		public override string GetTooltip(Player player, Gear gear)
		{
			return $"Healing potions restore {10 + (int)(Value * 10) + gear.Power / 20} more life";
		}

		public override void BuffPassive(Player player, Gear gear)
		{
			player.GetModPlayer<PotionSystem>().HealPower += 10 + (int)(Value * 10) + gear.Power / 20;
		}
	}

	internal class LifePotionCapAffix : Affix
	{
		public LifePotionCapAffix() : base()
		{
			PossibleTypes = GearType.Helmet | GearType.Chestplate | GearType.Leggings;
		}

		public override string GetTooltip(Player player, Gear gear)
		{
			return $"You can hold {1 + (int)Math.Round(Value, MidpointRounding.ToEven) + gear.Power / 100} additional healing potions";
		}

		public override void BuffPassive(Player player, Gear gear)
		{
			player.GetModPlayer<PotionSystem>().MaxHealing += 1 + (int)Math.Round(Value, MidpointRounding.ToEven) + gear.Power / 100;
		}
	}

	internal class LifePotionCooldownAffix : Affix
	{
		public LifePotionCooldownAffix() : base()
		{
			PossibleTypes = GearType.Helmet | GearType.Chestplate | GearType.Leggings;
			RequiredInfluence = GearInfluence.Lunar;
		}

		public override string GetTooltip(Player player, Gear gear)
		{
			return $"Healing potions are ready {0.5f + Value * 0.5f} seconds sooner";
		}

		public override void BuffPassive(Player player, Gear gear)
		{
			player.GetModPlayer<PotionSystem>().HealDelay -= (int)(60 * (0.5f + Value * 0.5f));
		}
	}
}
