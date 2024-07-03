namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes;

internal class MovementSpeed : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.MovementSpeed += Value / 100;
	}
}