using PathOfTerraria.Content.Items.Gear;

namespace PathOfTerraria.Core.Systems.Affixes.Affixes.GearTypes.ArmorAffixes;

internal class DefenseAffixes
{
	internal class DefenseGearAffix : GearAffix
	{
		public override GearType PossibleTypes => GearType.Armor;
		public override ModifierType ModifierType => ModifierType.Added;
		public override string Tooltip => "# Defense";
		public override bool Round => true;

		protected override float InternalModifierCalculation(Gear gear)
		{
			return 1 + Value * 5f + gear.ItemLevel / 50f;
		}

		public override void BuffPassive(Player player, Gear gear)
		{
			player.statDefense += GetModifierIValue(gear);
		}
	}

	internal class EnduranceGearAffix : GearAffix
	{
		public override GearType PossibleTypes => GearType.Armor;
		public override ModifierType ModifierType => ModifierType.Added;
		public override bool IsFlat => false;
		public override string Tooltip => "# Damage Reduction";

		protected override float InternalModifierCalculation(Gear gear)
		{
			return 1 + (float)Math.Truncate((Value * 5 + gear.ItemLevel / 50) * 10) / 10;
		}

		public override void BuffPassive(Player player, Gear gear)
		{
			player.endurance += GetModifierValue(gear) / 100f;
		}
	}
}