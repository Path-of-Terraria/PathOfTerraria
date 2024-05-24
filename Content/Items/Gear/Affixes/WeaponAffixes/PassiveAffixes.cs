namespace PathOfTerraria.Content.Items.Gear.Affixes.WeaponAffixes
{
	public class PassiveAffixes
	{
		internal class RapidAffix : Affix
		{
			public RapidAffix()
			{
				PossibleTypes = GearType.Sword | GearType.Staff;
			}

			public override float GetModifierValue(Gear gear)
			{
				return 1 + (int)(Value * 5) + gear.ItemLevel / 50;
			}
		
			public override string GetTooltip(Player player, Gear gear)
			{
				return $"+{GetModifierValue(gear)} Attack Speed";
			}
		}
		
		internal class SharpAffix : Affix
		{
			public SharpAffix()
			{
				PossibleTypes = GearType.Sword;
				ModifierType = ModifierType.Added;
			}

			public override float GetModifierValue(Gear gear)
			{
				return 1 + (int)(Value * 5) + gear.ItemLevel / 50;
			}
		
			public override string GetTooltip(Player player, Gear gear)
			{
				return $"+{GetModifierValue(gear)} Additional Damage";
			}
		}
		
		internal class ChargedAffix : Affix
		{
			public ChargedAffix()
			{
				PossibleTypes = GearType.Staff;
				ModifierType = ModifierType.Added;
			}

			public override float GetModifierValue(Gear gear)
			{
				return 1 + (int)(Value * 5) + gear.ItemLevel / 50;
			}
		
			public override string GetTooltip(Player player, Gear gear)
			{
				return $"+{GetModifierValue(gear)} Additional Damage";
			}
		}
	}
}