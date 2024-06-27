namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes.ArmorAffixes;

internal class DefenseAffixes
{
	internal class DefenseItemAffix : ItemAffix
	{
		public override void ApplyAffix(EntityModifier modifier, PoTItem item)
		{
			modifier.Defense.Base += 1 + Value * 5f + item.ItemLevel / 50f;
		}
	}

	internal class EnduranceItemAffix : ItemAffix
	{
		public override void ApplyAffix(EntityModifier modifier, PoTItem item)
		{
			modifier.DamageReduction.Base += (float)Math.Truncate((Value * 5 + item.ItemLevel / 50) * 10) / 10;
		}
	}

	internal class ResistanceHelmetAffix : ItemAffix
	{
		public override void ApplyAffix(EntityModifier modifier, PoTItem item)
		{
			modifier.DebuffResistance *= 1 - (float)Math.Truncate((Value * 2 + item.ItemLevel / 450) * 5) / 300f;
		}
	}

	internal class BuffBoostHelmetAffix : ItemAffix
	{
		public override void ApplyAffix(EntityModifier modifier, PoTItem item)
		{
			modifier.BuffBonus *= 1 + (float)Math.Truncate((Value * 2 + item.ItemLevel / 450) * 5) / 300f;
		}
	}

	internal class ThornyArmorAffix : ItemAffix
	{
		public override void ApplyAffix(EntityModifier modifier, PoTItem item)
		{
			modifier.ReflectedDamageModifier += Value * 0.1f;
		}
	}
}