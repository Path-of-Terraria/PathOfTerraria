using PathOfTerraria.Common.Systems.ElementalDamage;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal abstract class ResistItemAffix : ItemAffix
{
	
}

internal class FireResistItemAffix : ResistItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<ElementalPlayer>().FireResistance += Value * 0.01f;
	}
}

internal class ColdResistItemAffix : ResistItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<ElementalPlayer>().ColdResistance += Value * 0.01f;
	}
}

internal class LightningResistItemAffix : ResistItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<ElementalPlayer>().LightningResistance += Value * 0.01f;
	}
}
