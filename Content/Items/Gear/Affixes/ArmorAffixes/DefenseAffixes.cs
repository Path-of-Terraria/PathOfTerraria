namespace PathOfTerraria.Content.Items.Gear.Affixes.ArmorAffixes;

internal class DefenseAffixes
{
	internal class DefenseGearAffix : GearAffix
	{
		public DefenseGearAffix()
		{
			PossibleTypes = GearType.Helmet | GearType.Chestplate | GearType.Leggings;
		}

		public override float GetModifierValue(Gear gear)
		{
			return 1 + (int)(Value * 5) + gear.ItemLevel / 50;
		}

		public override string GetTooltip(Player player, Gear gear)
		{
			return $"+{GetModifierValue(gear)} Additional Defense";
		}

		public override void BuffPassive(Player player, Gear gear)
		{
			player.statDefense += 1 + (int)(Value * 5) + gear.ItemLevel / 50;
		}
	}

	internal class EnduranceGearAffix : GearAffix
	{
		public EnduranceGearAffix()
		{
			PossibleTypes = GearType.Helmet | GearType.Chestplate | GearType.Leggings;
		}

		public override float GetModifierValue(Gear gear)
		{
			return 1 + (float)Math.Truncate((Value * 5 + gear.ItemLevel / 50) * 10)  / 10;
		}
			
		public override string GetTooltip(Player player, Gear gear)
		{
			return $"+{GetModifierValue(gear)}% Damage Reduction";
		}

		public override void BuffPassive(Player player, Gear gear)
		{
			player.endurance += (float)Math.Truncate((Value * 5 + gear.ItemLevel / 50) * 10) / 10 / 100f;
		}
	}
}