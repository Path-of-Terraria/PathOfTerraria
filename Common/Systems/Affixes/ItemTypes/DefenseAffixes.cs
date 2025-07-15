using PathOfTerraria.Common.Systems.BlockSystem;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal class DefenseItemAffix : ItemAffix
{
	public DefenseItemAffix()
	{
		Round = true;
	}
	
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Defense.Base += Value;
	}
}

internal class EnduranceItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.DamageReduction.Base += Value;
	}
}

internal class ResistanceHelmetAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.DebuffResistance *= 1 + Value / 100f;
	}
}

internal class BuffBoostHelmetAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.BuffBonus *= 1 + Value / 100f;
	}
}

internal class ThornyArmorAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.ReflectedDamageModifier += Value;
	}
}

internal class IncreaseBlockAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<BlockPlayer>().MultiplyBlockChance(1 + Value / 100f);
	}
}