namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes.ArmorAffixes;

internal class MovementSpeed : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.MovementSpeed += item.ItemLevel / 10f * (0.6f + 0.4f * Value) / 100f;
	}
}