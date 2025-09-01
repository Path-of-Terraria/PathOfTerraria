using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
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
		NPC.SpawnOnPlayer(player.whoAmI, NPCID.TheDestroyer);
		return true;
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
