namespace PathOfTerraria.Content.Items.Gear.Affixes.WeaponAffixes;

public class ModifyHitAffixes
{
	internal class PiercingAffix : Affix
	{
		public override GearType PossibleTypes => GearType.Melee;
		public override ModifierType ModifierType => ModifierType.Added;
		public override string Tooltip => "# Armor Penetration";

		protected override float internalModifierCalculation(Gear gear)
		{
			return 1 + (int)(Value * 5) + gear.ItemLevel / 50;
		}
	}

	internal class AddedKnockbackAffix : Affix
	{
		public override GearType PossibleTypes => GearType.Weapon;
		public override ModifierType ModifierType => ModifierType.Added;
		public override string Tooltip => "# Added Knockback";

		protected override float internalModifierCalculation(Gear gear)
		{
			return 1 + (int)(Value * 5) + gear.ItemLevel / 50;
		}
	}

	internal class IncreasedKnockbackAffix : Affix
	{
		public override GearType PossibleTypes => GearType.Weapon;
		public override ModifierType ModifierType => ModifierType.Multiplier;
		public override string Tooltip => "# Increased Knockback";
		protected override float internalModifierCalculation(Gear gear)
		{
			return (1 + (int)(Value * 5) + gear.ItemLevel / 50) * 0.01f;
		}
	}
}