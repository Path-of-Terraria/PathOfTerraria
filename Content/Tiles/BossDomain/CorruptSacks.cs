using PathOfTerraria.Common.Systems.Networking.Handlers;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class CorruptSacks : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileLighted[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
		TileObjectData.newTile.RandomStyleRange = 3;
		TileObjectData.newTile.DrawYOffset = 2;
		TileObjectData.newTile.Origin = new Point16(1); // This breaks when stuff is placed near it. Too bad! It works in-context.
		TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.EmptyTile | AnchorType.SolidTile | AnchorType.None, 3, 0);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.EmptyTile | AnchorType.SolidTile | AnchorType.None, 3, 0);
		TileObjectData.addTile(Type);

		DustType = DustID.CorruptGibs;

		AddMapEntry(new Color(183, 163, 152));
	}

	public override void NearbyEffects(int i, int j, bool closer)
	{
		if (closer && Main.LocalPlayer.DistanceSQ(new Vector2(i, j).ToWorldCoordinates()) < 250 * 250)
		{
			WorldGen.KillTile(i, j);
		}
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		int type = Main.rand.NextBool(6) ? NPCID.DevourerHead : NPCID.EaterofSouls;
		Vector2 velocity = AwayFromNearestPlayer(i, j) * Main.rand.NextFloat(5, 8);

		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			int npc = NPC.NewNPC(new EntitySource_TileBreak(i, j), (i + 1) * 16, (j + 1) * 16, type, 1);
			Main.npc[npc].velocity = velocity;
			Main.npc[npc].netUpdate = true;
		}
		else
		{
			SpawnNPCOnServerHandler.Send((short)type, new((i + 1) * 16, (j + 1) * 16), velocity);
		}

		for (int k = 0; k < 16; k++)
		{
			Dust.NewDust(new Vector2(i, j + 1).ToWorldCoordinates(0, 0), 32, 32, DustID.CorruptGibs, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-4, -1));
		}
	}

	private static Vector2 AwayFromNearestPlayer(int i, int j)
	{
		Vector2 position = new Vector2(i, j).ToWorldCoordinates();
		Player plr = Main.player[Player.FindClosest(position, 16, 16)];
		return plr.DirectionTo(position);
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		Tile tile = Main.tile[i, j];
		int frameX = tile.TileFrameX / 18 % 3;
		int frameY = tile.TileFrameY / 18 % 3;

		if (frameX == 1 && frameY == 1)
		{
			float sine = MathF.Max(0, 3 * MathF.Sin(i + j + 0.02f * Main.GameUpdateCount)) * 0.36f;
			(r, g, b) = (sine * 0.5f, sine * 0.3f, sine);
		}
	}

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];
		Texture2D tex = TextureAssets.Tile[Type].Value;
		Vector2 offScreen = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
		Vector2 position = new Vector2(i, j).ToWorldCoordinates(0, 0) - Main.screenPosition + offScreen + Vector2.One * 8;
		var source = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);
		float sine = MathF.Max(0, 3 * MathF.Sin(-i - j + 0.05f * Main.GameUpdateCount)) * 0.05f;

		spriteBatch.Draw(tex, position, source, Lighting.GetColor(i, j), 0f, Vector2.One * 8, 1f + sine, SpriteEffects.None, 0);
		return false;
	}
}