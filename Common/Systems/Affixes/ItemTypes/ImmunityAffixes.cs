using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

/// <summary> The player becomes immune to the Chilled debuff. </summary>
internal class CannotBeChilledAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.buffImmune[BuffID.Chilled] = true;
	}
}
