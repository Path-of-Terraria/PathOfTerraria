namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes.ArmorAffixes;

internal class MovementSpeed : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.MovementSpeed += Value;
	}
}