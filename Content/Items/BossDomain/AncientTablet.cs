using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.BossDomain;

internal class AncientTablet : ModItem
{
	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.SuspiciousLookingEye);
		Item.Size = new Vector2(54);
		Item.consumable = true;
	}

	public override bool? UseItem(Player player)
	{
		return BossDomainSummonHandler.TrySummon(
			BossDomainSummonHandler.BossDomainSummon.Cultist,
			player);
	}

	public override void UpdateInventory(Player player)
	{
		if (SubworldSystem.Current is not CultistDomain)
		{
			Item.TurnToAir();
		}
	}

	public override void AddRecipes()
	{
		CreateRecipe().AddIngredient<AncientTabletOne>().AddIngredient<AncientTabletTwo>().AddIngredient<AncientTabletThree>().AddIngredient<AncientTabletFour>().Register();
	}
}
