using FunnyExperience.Core.Systems;

namespace FunnyExperience.Content.Items.Gear.Affixes.ArmorAffixes
{
	internal class ManaAffix : Affix
	{
		public ManaAffix() : base()
		{
			PossibleTypes = GearType.Helmet | GearType.Chestplate | GearType.Leggings;
		}

		public override string GetTooltip(Player player, Gear gear)
		{
			return $"+{5 + (int)(Value * 20) + gear.Power / 20} Maximum Mana";
		}

		public override void BuffPassive(Player player, Gear gear)
		{
			player.statManaMax2 += 5 + (int)(Value * 20) + gear.Power / 20;
		}
	}

	internal class ManaRegenAffix : Affix
	{
		public ManaRegenAffix() : base()
		{
			PossibleTypes = GearType.Helmet | GearType.Chestplate | GearType.Leggings;
		}

		public override string GetTooltip(Player player, Gear gear)
		{
			return $"+{1 + (int)(Value * 4) + gear.Power / 40} Mana Regeneration";
		}

		public override void BuffPassive(Player player, Gear gear)
		{
			player.manaRegen += 1 + (int)(Value * 4) + gear.Power / 40;
		}
	}

	internal class ManaPotionPowerAffix : Affix
	{
		public ManaPotionPowerAffix() : base()
		{
			PossibleTypes = GearType.Helmet | GearType.Chestplate | GearType.Leggings;
		}

		public override string GetTooltip(Player player, Gear gear)
		{
			return $"Mana potions restore {10 + (int)(Value * 10) + gear.Power / 20} more Mana";
		}

		public override void BuffPassive(Player player, Gear gear)
		{
			player.GetModPlayer<PotionSystem>().ManaPower += 10 + (int)(Value * 10) + gear.Power / 20;
		}
	}

	internal class ManaPotionCapAffix : Affix
	{
		public ManaPotionCapAffix() : base()
		{
			PossibleTypes = GearType.Helmet | GearType.Chestplate | GearType.Leggings;
		}

		public override string GetTooltip(Player player, Gear gear)
		{
			return $"You can hold {1 + (int)Math.Round(Value, MidpointRounding.ToEven) + gear.Power / 100} additional mana potions";
		}

		public override void BuffPassive(Player player, Gear gear)
		{
			player.GetModPlayer<PotionSystem>().MaxMana += 1 + (int)Math.Round(Value, MidpointRounding.ToEven) + gear.Power / 100;
		}
	}

	internal class ManaPotionCooldownAffix : Affix
	{
		public ManaPotionCooldownAffix() : base()
		{
			PossibleTypes = GearType.Helmet | GearType.Chestplate | GearType.Leggings;
			RequiredInfluence = GearInfluence.Solar;
		}

		public override string GetTooltip(Player player, Gear gear)
		{
			return $"Mana potions are ready {0.5f + Value * 0.5f} seconds sooner";
		}

		public override void BuffPassive(Player player, Gear gear)
		{
			player.GetModPlayer<PotionSystem>().ManaDelay -= (int)(60 * (0.5f + Value * 0.5f));
		}
	}
}
