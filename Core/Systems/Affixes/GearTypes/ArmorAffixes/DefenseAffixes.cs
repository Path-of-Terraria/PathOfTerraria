using PathOfTerraria.Content.Items.Gear;

namespace PathOfTerraria.Core.Systems.Affixes.Affixes.GearTypes.ArmorAffixes;

internal class DefenseAffixes
{
	internal class DefenseGearAffix : GearAffix
	{
		public override GearType PossibleTypes => GearType.Armor;
		public override void ApplyAffix(EntityModifier modifier, Gear gear)
		{
			modifier.Defense.Base += 1 + Value * 5f + gear.ItemLevel / 50f;
		}
	}

	internal class EnduranceGearAffix : GearAffix
	{ // this seems to need some modification
		public override GearType PossibleTypes => GearType.Armor;
		public override void ApplyAffix(EntityModifier modifier, Gear gear)
		{
			modifier.DamageReduction.Base += (float)Math.Truncate((Value * 5 + gear.ItemLevel / 50) * 10) / 10;
		}
	}
}