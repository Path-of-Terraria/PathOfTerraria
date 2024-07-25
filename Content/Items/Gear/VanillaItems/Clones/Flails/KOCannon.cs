using PathOfTerraria.Common.Enums;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Flails;

internal class KOCannon : VanillaClone
{
	protected override short VanillaItemId => ItemID.KOCannon;

	public override void Defaults()
	{
		ItemType = ItemType.Melee;
		base.Defaults();
	}

	public override Vector2? HoldoutOffset()
	{
		return new Vector2(4, 0);
	}
}
