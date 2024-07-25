using PathOfTerraria.Common.Enums;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Flails;

internal class GolemFist : VanillaClone
{
	protected override short VanillaItemId => ItemID.GolemFist;

	public override void Defaults()
	{
		ItemType = ItemType.Melee;
		base.Defaults();
	}

	public override Vector2? HoldoutOffset()
	{
		return new Vector2(-8, 0);
	}
}
