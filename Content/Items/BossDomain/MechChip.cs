using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using SubworldLibrary;
using Terraria.DataStructures;
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
		Point16 pos = Main.MouseWorld.ToTileCoordinates16();
		MoonLordDomain.Cloud(pos.X, pos.Y);
		//NPC.SpawnOnPlayer(player.whoAmI, NPCID.TheDestroyer);
		return true;
	}

	public override void UpdateInventory(Player player)
	{
		if (SubworldSystem.Current is not DestroyerDomain)
		{
			//Item.TurnToAir();
		}
	}

	public override void AddRecipes()
	{
		CreateRecipe().AddIngredient<MechDrive>(5).Register();
	}
}
