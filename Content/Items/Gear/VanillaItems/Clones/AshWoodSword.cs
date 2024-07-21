using PathOfTerraria.Core;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones;

internal class AshWoodSword : VanillaClone
{
	protected override short VanillaItemId => ItemID.AshWoodSword;
	
	public override void Defaults()
	{
		ItemType = ItemType.Sword;
		base.Defaults();
	}
}