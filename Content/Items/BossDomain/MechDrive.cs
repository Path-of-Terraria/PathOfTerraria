using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.BossDomain;

/// <summary>
/// Item used solely to gate progression by needing many to craft the <see cref="MechChip"/>.
/// </summary>
internal class MechDrive : ModItem
{
	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.Size = new Vector2(26, 20);
	}

	public override void UpdateInventory(Player player)
	{
		if (SubworldSystem.Current is not DestroyerDomain)
		{
			Item.TurnToAir();
		}
	}
}
