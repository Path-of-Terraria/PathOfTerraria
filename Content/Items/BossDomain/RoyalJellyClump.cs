using Terraria.ID;

namespace PathOfTerraria.Content.Items.BossDomain;

internal class RoyalJellyClump : ModItem
{
	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.Size = new Vector2(30, 22);
	}
}
