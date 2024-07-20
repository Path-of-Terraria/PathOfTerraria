using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class TragicUmbrella : Umbrella
{
	protected override short VanillaItemId => ItemID.TragicUmbrella;

	public override void UseStyle(Player player, Rectangle heldItemFrame)
	{
		base.UseStyle(player, heldItemFrame);
		heldItemFrame.X -= 8;
	}
}
