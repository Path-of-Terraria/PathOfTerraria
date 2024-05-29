using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Systems;

namespace PathOfTerraria.Core.Systems.Affixes.Affixes.GearTypes.ArmorAffixes;

internal class MovementSpeed : GearAffix
{
	public override GearType PossibleTypes => GearType.Leggings;
	public override ModifierType ModifierType => ModifierType.Added;
	public override bool IsFlat => false;
	public override string Tooltip => "# Movement Speed";
	protected override float internalModifierCalculation(Gear gear)
	{
		return gear.ItemLevel / 10f * (0.6f + 0.4f * Value); // ranges from 60% to 100%
	}

	public override void BuffPassive(Player player, Gear gear)
	{
		player.moveSpeed += GetModifierValue(gear) / 100f;
	}
}