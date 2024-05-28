namespace PathOfTerraria.Content.Items.Gear.Affixes.WeaponAffixes;

public class PassiveAffixes
{
	internal class RapidGearAffix : GearAffix
	{
		public RapidGearAffix()
		{
			PossibleTypes = GearType.Weapon;
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
		
	internal class SharpGearAffix : GearAffix
	{
		public SharpGearAffix()
		{
			PossibleTypes = GearType.Melee;
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
		
	internal class ChargedGearAffix : GearAffix
	{
		public ChargedGearAffix()
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