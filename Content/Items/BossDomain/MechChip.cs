using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.BossDomain;

internal class MechChip : ModItem
{
	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Abeemination);
		Item.Size = new Vector2(30, 22);
	}

	public override bool? UseItem(Player player)
	{
		return BossDomainSummonHandler.TrySummon(
			BossDomainSummonHandler.BossDomainSummon.Destroyer,
			player);
	}

	public override void UpdateInventory(Player player)
	{
		if (SubworldSystem.Current is not DestroyerDomain)
		{
			Item.TurnToAir();
		}
	}

	public override void AddRecipes()
	{
		CreateRecipe().AddIngredient<MechDrive>(5).Register();
	}
}
