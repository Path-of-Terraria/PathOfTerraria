namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes.ArmorAffixes;

internal class DefenseAffixes
{
	internal class DefenseItemAffix : ItemAffix
	{
		public override ItemType PossibleTypes => ItemType.Armor;
		public override void ApplyAffix(EntityModifier modifier, PoTItem item)
		{
			modifier.Defense.Base += 1 + Value * 5f + item.ItemLevel / 50f;
		}
	}

	internal class EnduranceItemAffix : ItemAffix
	{ // this seems to need some modification
		public override ItemType PossibleTypes => ItemType.Armor;
		public override void ApplyAffix(EntityModifier modifier, PoTItem item)
		{
			modifier.DamageReduction.Base += (float)Math.Truncate((Value * 5 + item.ItemLevel / 50) * 10) / 10;
		}
	}
}