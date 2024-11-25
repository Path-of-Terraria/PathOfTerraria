using PathOfTerraria.Common.Subworlds.BossDomains.DeerDomain;
using PathOfTerraria.Common.Systems.DrawLayers;
using PathOfTerraria.Common.Systems.RealtimeGen;
using PathOfTerraria.Content.Tiles.BossDomain;
using PathOfTerraria.Core.Items;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Quest;

internal class SimpleCompass : ModItem
{
	private static Asset<Texture2D> _arrowTex = null;

	public override void Load()
	{
		_arrowTex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CompassArrow");

		MiscDrawLayer.OnDraw += DrawCompassPoint;
	}

	private void DrawCompassPoint(ref PlayerDrawSet drawInfo)
	{
		if (drawInfo.shadow == 0 && drawInfo.drawPlayer.HeldItem.type == ModContent.ItemType<SimpleCompass>())
		{
			Vector2 dir = drawInfo.Center.DirectionTo(ModContent.GetInstance<DeerclopsSystem>().AntlerLocation.ToWorldCoordinates());

			Main.spriteBatch.Draw(_arrowTex.Value, drawInfo.Center - Main.screenPosition + dir * 40, null, Color.White,
				dir.ToRotation() + MathHelper.PiOver2, new Vector2(11, 38), 1f, SpriteEffects.None, 0);
		}
	}

	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 1;
	}

	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.rare = ItemRarityID.Quest;
		Item.Size = new Vector2(24, 18);
		Item.maxStack = 1;
	}

	public override void HoldItem(Player player)
	{
		Point16 antlers = ModContent.GetInstance<DeerclopsSystem>().AntlerLocation;

		if (player.DistanceSQ(antlers.ToWorldCoordinates()) < MathF.Pow(16 * 35, 2))
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				List<RealtimeStep> actions = [];

				for (int i = 0; i < 7; ++i)
				{
					int x = antlers.X;
					int y = antlers.Y - 4;

					for (int j = 0; j < 50; ++j)
					{
						Tile tile = Main.tile[x + j, y + i];

						if (tile.HasTile && tile.TileType == ModContent.TileType<AncientLeadBrick>() || tile.TileType == TileID.BlueDungeonBrick)
						{
							actions.Add(new RealtimeStep((x, y) =>
							{
								Tile tile = Main.tile[x, y];
								tile.HasTile = false;
								Collision.HitTiles(new Vector2(x, y).ToWorldCoordinates(0, 0), Vector2.Zero, 16, 16);
								WorldGen.SquareTileFrame(x, y);
								return true;
							}, new Point16(x + j, y + i)));
						}
					}

					x = antlers.X;
					y = antlers.Y - 4;

					for (int j = 0; j > -50; --j)
					{
						Tile tile = Main.tile[x + j, y + i];

						if (tile.HasTile && tile.TileType == ModContent.TileType<AncientLeadBrick>() || tile.TileType == TileID.BlueDungeonBrick)
						{
							actions.Add(new RealtimeStep((x, y) =>
							{
								Tile tile = Main.tile[x, y];
								tile.HasTile = false;
								Collision.HitTiles(new Vector2(x, y).ToWorldCoordinates(0, 0), Vector2.Zero, 16, 16);
								WorldGen.SquareTileFrame(x, y);
								return true;
							}, new Point16(x + j, y + i)));
						}
					}

					x = antlers.X - 4;
					y = antlers.Y;

					for (int j = 0; j > -50; --j)
					{
						Tile tile = Main.tile[x + i, y + j];

						if (tile.HasTile && tile.TileType == ModContent.TileType<AncientLeadBrick>() || tile.TileType == TileID.BlueDungeonBrick)
						{
							actions.Add(new RealtimeStep((x, y) =>
							{
								Tile tile = Main.tile[x, y];
								tile.HasTile = false;
								Collision.HitTiles(new Vector2(x, y).ToWorldCoordinates(0, 0), Vector2.Zero, 16, 16);
								WorldGen.SquareTileFrame(x, y);
								return true;
							}, new Point16(x + i, y + j)));
						}
					}
				}

				RealtimeGenerationSystem.AddAction(new RealtimeGenerationAction(actions, 0.06f));
			}

			Item.TurnToAir();
			PoTItemHelper.SetMouseItemToHeldItem(player);

			SoundEngine.PlaySound(SoundID.Shatter, player.Center);
		}
	}
}
