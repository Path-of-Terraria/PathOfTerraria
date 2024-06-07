using PathOfTerraria.Content.Items.Gear;

namespace PathOfTerraria.Core.Systems.Affixes.Affixes.GearTypes.ArmorAffixes;

internal class MovementSpeed : GearAffix
{
	public override GearType PossibleTypes => GearType.Leggings;
	public override void ApplyAffix(EntityModifier modifier, Gear gear)
	{
		modifier.MovementSpeed += gear.ItemLevel / 10f * (0.6f + 0.4f * Value) / 100f;
	}
}