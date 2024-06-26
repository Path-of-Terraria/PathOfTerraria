namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes.WeaponAffixes;

public class PassiveAffixes
{
	internal class RapidItemAffix : ItemAffix
	{
		public override ItemType PossibleTypes => ItemType.Melee;

		public override void ApplyAffix(EntityModifier modifier, PoTItem item)
		{
			modifier.Attackspeed += (Value * 5 + Value * item.ItemLevel / 10f) / 100f;
		}
	}

	internal class SharpItemAffix : ItemAffix
	{
		public override ItemType PossibleTypes => ItemType.Melee;

		public override void ApplyAffix(EntityModifier modifier, PoTItem item)
		{
			modifier.Damage += (Value * 5 + Value * item.ItemLevel / 20) / 100f;
		}
	}

	internal class ChargedItemAffix : ItemAffix
	{
		public override ItemType PossibleTypes => ItemType.Melee;

		public override void ApplyAffix(EntityModifier modifier, PoTItem item)
		{
			modifier.Damage *= 1 + (Value * 5 + Value * item.ItemLevel / 20) / 100f;
		}
	}
}