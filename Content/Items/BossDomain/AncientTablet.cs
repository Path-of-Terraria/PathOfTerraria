using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
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
		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			NPC.SpawnBoss((int)player.Center.X, (int)player.Center.Y - 120, NPCID.CultistBoss, player.whoAmI);
		}

		return true;
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
