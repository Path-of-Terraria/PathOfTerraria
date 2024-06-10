using PathOfTerraria.Content.Items.Gear;

namespace PathOfTerraria.Core.Systems.Affixes.Affixes.GearTypes.WeaponAffixes;

public class PassiveAffixes
{
	internal class RapidGearAffix : GearAffix
	{
		public override GearType PossibleTypes => GearType.Melee;
		public override void ApplyAffix(EntityModifier modifier, Gear gear)
		{
			modifier.Attackspeed += (Value * 5 + Value * gear.ItemLevel / 10f) / 100f;
		}
	}

	internal class SharpGearAffix : GearAffix
	{
		public override GearType PossibleTypes => GearType.Melee;
		public override void ApplyAffix(EntityModifier modifier, Gear gear)
		{
			modifier.Damage += (Value * 5 + Value * gear.ItemLevel / 20) / 100f;
		}
	}

	internal class ChargedGearAffix : GearAffix
	{
		public override GearType PossibleTypes => GearType.Melee;
		public override void ApplyAffix(EntityModifier modifier, Gear gear)
		{
			modifier.Damage *= 1 + (Value * 5 + Value * gear.ItemLevel / 20) / 100f;
		}
	}
}