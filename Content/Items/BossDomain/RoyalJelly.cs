using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.BossDomain;

internal class RoyalJelly : ModItem
{
	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Abeemination);
		Item.Size = new Vector2(48, 36);
	}

	public override bool? UseItem(Player player)
	{
		return BossDomainSummonHandler.TrySummon(
			BossDomainSummonHandler.BossDomainSummon.QueenBee,
			player);
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient<RoyalJellyClump>(2)
			.Register();
	}
}
