namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes;

internal class NoFallDamageAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, PoTItem gear)
	{
		if (player != null)
		{
			player.noFallDmg = true;
		}
	}
}
