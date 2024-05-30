using PathOfTerraria.Content.Items.Gear;

namespace PathOfTerraria.Core.Systems.Affixes.Affixes.GearTypes.WeaponAffixes;

public class PassiveAffixes
{
	internal class RapidGearAffix : GearAffix
	{
		public override GearType PossibleTypes => GearType.Melee;
		public override ModifierType ModifierType => ModifierType.Added;
		public override bool IsFlat => false;
		public override string Tooltip => "# Attack Speed";
		protected override float InternalModifierCalculation(Gear gear)
		{
			return 1 + (int)(Value * 5) + gear.ItemLevel / 50;
		}
	}

	internal class SharpGearAffix : GearAffix
	{
		public override GearType PossibleTypes => GearType.Melee;
		public override ModifierType ModifierType => ModifierType.Added;
		public override string Tooltip => "# Additional Damage";

		protected override float InternalModifierCalculation(Gear gear)
		{
			return 1 + (int)(Value * 5) + gear.ItemLevel / 50;
		}
	}

	internal class ChargedGearAffix : GearAffix
	{
		public override GearType PossibleTypes => GearType.Melee;
		public override ModifierType ModifierType => ModifierType.Added;
		public override string Tooltip => "# Additional Damage";

		protected override float InternalModifierCalculation(Gear gear)
		{
			return 1 + (int)(Value * 5) + gear.ItemLevel / 50;
		}
	}
}