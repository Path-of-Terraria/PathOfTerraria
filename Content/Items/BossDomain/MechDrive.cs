using Terraria.ID;

namespace PathOfTerraria.Content.Items.BossDomain;

internal class MechDrive : ModItem
{
	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.Size = new Vector2(26, 20);
	}
}
