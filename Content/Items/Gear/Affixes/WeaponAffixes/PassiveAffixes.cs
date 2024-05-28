namespace PathOfTerraria.Content.Items.Gear.Affixes.WeaponAffixes;

public class PassiveAffixes
{
	internal class RapidAffix : Affix
	{
		public override GearType PossibleTypes => GearType.Melee;
		public override ModifierType ModifierType => ModifierType.Added;
		public override bool IsFlat => false;
		public override string Tooltip => "# Attack Speed";
		protected override float internalModifierCalculation(Gear gear)
		{
			return 1 + (int)(Value * 5) + gear.ItemLevel / 50;
		}
	}
		
	internal class SharpAffix : Affix
	{
		public override GearType PossibleTypes => GearType.Melee;
		public override ModifierType ModifierType => ModifierType.Added;
		public override string Tooltip => "# Additional Damage";

		protected override float internalModifierCalculation(Gear gear)
		{
			return 1 + (int)(Value * 5) + gear.ItemLevel / 50;
		}
	}
		
	internal class ChargedAffix : Affix
	{
		public override GearType PossibleTypes => GearType.Melee;
		public override ModifierType ModifierType => ModifierType.Added;
		public override string Tooltip => "# Additional Damage";

		protected override float internalModifierCalculation(Gear gear)
		{
			return 1 + (int)(Value * 5) + gear.ItemLevel / 50;
		}
	}
}