using PathOfTerraria.Content.Tiles.Furniture;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Quest;

internal class AncientEvilBook : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<EvilBook>());
		Item.Size = new Vector2(30, 36);
		Item.rare = ItemRarityID.Quest;
		Item.questItem = true;
	}

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		if (NPC.downedBoss3)
		{
			tooltips.RemoveAll(x => x.Name.StartsWith("Tooltip"));

			tooltips.Add(new TooltipLine(Mod, "Tooltip0", Language.GetTextValue($"Mods.{PoTMod.ModName}.Items.{Name}.PostSkeletronTooltip.0")));
			tooltips.Add(new TooltipLine(Mod, "Tooltip1", Language.GetTextValue($"Mods.{PoTMod.ModName}.Items.{Name}.PostSkeletronTooltip.1")));
		}
	}
}
