namespace FunnyExperience.Content.Items.Gear.Affixes.ArmorAffixes
{
	internal class DefenseAffixes
	{
		internal class DefenseAffix : Affix
		{
			public DefenseAffix() : base()
			{
				possibleTypes = GearType.Helmet | GearType.Chestplate | GearType.Leggings;
			}

			public override string GetTooltip(Player player, Gear gear)
			{
				return $"+{1 + (int)(value * 5) + gear.power / 50} Additional Defense";
			}

			public override void BuffPassive(Player player, Gear gear)
			{
				player.statDefense += 1 + (int)(value * 5) + gear.power / 50;
			}
		}

		internal class EnduranceAffix : Affix
		{
			public EnduranceAffix() : base()
			{
				possibleTypes = GearType.Helmet | GearType.Chestplate | GearType.Leggings;
			}

			public override string GetTooltip(Player player, Gear gear)
			{
				return $"+{(float)Math.Truncate((value * 5 + gear.power / 50) * 10) / 10}% Damage Reduction";
			}

			public override void BuffPassive(Player player, Gear gear)
			{
				player.endurance += (float)Math.Truncate((value * 5 + gear.power / 50) * 10) / 10 / 100f;
			}
		}
	}
}
