using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class ObsidianAdversary : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;

		TileID.Sets.FramesOnKillWall[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.RandomStyleRange = 3;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.Origin = new Point16(1);
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.AnchorWall = true;
		TileObjectData.addTile(Type);

		DustType = DustID.Obsidian;

		AddMapEntry(new Color(87, 81, 173));
	}

	public override bool CanDrop(int i, int j)
	{
		return false;
	}

	public override void HitWire(int i, int j)
	{
		Point16 pos = TileObjectData.TopLeft(i, j);

		for (int x = pos.X; x < pos.X + 2; ++x)
		{
			for (int y = pos.Y; y < pos.Y + 2; ++y)
			{
				Wiring.SkipWire(x, y);
			}
		}

		WorldGen.KillTile(pos.X, pos.Y);

		if (Main.dedServ)
		{
			NetMessage.SendTileSquare(-1, pos.X, pos.Y, 2, 2);
		}

		TeleportPlayers(pos);
	}

	private static void TeleportPlayers(Point16 pos)
	{
		Vector2 world = pos.ToWorldCoordinates();
		int plr = Player.FindClosest(world, 1, 1);
		Vector2 target = Main.player[plr].position;

		foreach (Player player in Main.ActivePlayers)
		{
			if (player.whoAmI != plr)
			{
				if (Main.dedServ)
				{
					NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, (int)target.X, (int)target.Y, -1, 0, 0);
				}
				else
				{
					TeleportEffects(player.position);
					player.Center = target;
					TeleportEffects(player.position);
				}
			}
		}
	}

	private static void TeleportEffects(Vector2 position)
	{
		for (int i = 0; i < 15; ++i)
		{
			Dust.NewDust(position, Player.defaultWidth, Player.defaultHeight, Main.rand.NextBool() ? Main.rand.NextBool() ? DustID.Lava : DustID.Torch : DustID.FlameBurst);
		}

		SoundEngine.PlaySound(SoundID.Item6, position + new Vector2(Player.defaultWidth, Player.defaultHeight) / 2f);
	}
}

internal class ObsidianAdversaryVanity : ObsidianAdversary
{
	public override string Texture => base.Texture.Replace("Vanity", "");

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		RegisterItemDrop(ModContent.ItemType<ObsidianAdversaryItem>());
	}

	public override bool CanDrop(int i, int j)
	{
		return true;
	}

	// Just do nothing with this since it's vanity
	public override void HitWire(int i, int j)
	{
	}
}

/// <summary>
/// This is a debug item only. The item players should interact with outside of cheat tools should be <see cref="ObsidianAdversaryItem"/>.
/// </summary>
internal class ObsidianAdversaryDebug : ModItem
{
	public override string Texture => base.Texture.Replace("Debug", "Item");

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<ObsidianAdversary>());
		Item.value = Item.buyPrice(0, 0, 15, 0);
	}

	public override void HoldItem(Player player)
	{
		player.InfoAccMechShowWires = true;
	}
}

internal class ObsidianAdversaryItem : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<ObsidianAdversaryVanity>());
		Item.value = Item.buyPrice(0, 0, 15, 0);
	}
}