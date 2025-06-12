using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.BossDomain;

internal class AncientTabletOne : ModItem
{
	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.Size = new Vector2(48, 30);
	}

	public override void UpdateInventory(Player player)
	{
		if (SubworldSystem.Current is not CultistDomain)
		{
			Item.TurnToAir();
		}
	}
}

internal class AncientTabletTwo : AncientTabletOne
{
	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.Size = new Vector2(38, 30);
	}
}

internal class AncientTabletThree : AncientTabletOne
{
	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.Size = new Vector2(30, 32);
	}
}

internal class AncientTabletFour : AncientTabletOne
{
	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.Size = new Vector2(36, 26);
	}
}
