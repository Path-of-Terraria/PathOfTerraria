namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal class MovementSpeed : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.MovementSpeed += Value / 100;
	}
}