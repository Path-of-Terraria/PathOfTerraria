#if DEBUG
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Placeable.Debugging;

internal class OffsetTool : ModItem
{
	private readonly List<Point16> all = [];

	private Point16 Anchor = Point16.Zero;
	private int middleClickTimer = 0;

	public override void SetDefaults()
	{
		Item.width = 16;
		Item.height = 16;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.useTime = 30;
		Item.useAnimation = 30;
		Item.noMelee = true;
	}

	public override bool AltFunctionUse(Player player)
	{
		return true;
	}

	public override bool? UseItem(Player player)
	{
		if (player.altFunctionUse == 2)
		{
			Anchor = Main.MouseWorld.ToTileCoordinates16();
			Main.NewText("Set anchor.");
		}
		else
		{
			Point16 pos = Main.MouseWorld.ToTileCoordinates16();
			Point16 rel = new Point16(pos.X - Anchor.X, pos.Y - Anchor.Y);
			all.Add(rel);
			Main.NewText(rel);
		}

		return true;
	}

	public override void HoldItem(Player player)
	{
		middleClickTimer--;

		if (Main.mouseMiddle && middleClickTimer < 30)
		{
			if (middleClickTimer > 0)
			{
				all.Clear();
				Main.NewText("Cleared list.");
			}
			else
			{
				string text = "";

				foreach (Point16 p in all)
				{
					text += $"new EngageTimerInfo(new({p.X}, {p.Y}), 0), ";
				}

				Mod.Logger.Debug(text);

				Main.NewText("Posted offsets to log.");
			}

			middleClickTimer = 60;
		}
	}
}
#endif