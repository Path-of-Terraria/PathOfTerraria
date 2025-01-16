using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.Tiles;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.Maps.Forest;

internal class Runestone : ModTile
{
	private static Asset<Texture2D> Glow = null;

	public override void Load()
	{
		Glow = ModContent.Request<Texture2D>(Texture + "_Glow");
	}

	public override void SetStaticDefaults()
	{
		Main.tileBrick[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileBlendAll[Type] = true;

		TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;

		AddMapEntry(new Color(56, 66, 66));

		DustType = DustID.Lead;
		HitSound = SoundID.Tink;
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Vector2 worldPos = new Vector2(i, j).ToWorldCoordinates();
		int plr = Player.FindClosest(worldPos - new Vector2(8), 16, 16);
		float alpha = 1 - MathHelper.Clamp(Main.player[plr].Distance(worldPos) / 250f, 0, 1f);

		if (OpenExtensions.GetOpenings(i, j, false) != OpenFlags.None)
		{
			this.DrawSloped(i, j, Glow.Value, Color.White * alpha, Vector2.Zero);
		}
		else
		{
			float GetTime(float offset)
			{
				return MathF.Pow(MathF.Sin(Main.GameUpdateCount * 0.03f + offset + i * MathHelper.PiOver2 + j * MathHelper.PiOver2 + i + j * 0.3f), 2) * 0.75f;
			}

			var color = new Color((float)(GetTime(0.1f) % 1), (float)(GetTime(0.2f) % 1), (float)(GetTime(-0.3f) % 1));
			this.DrawSloped(i, j, Glow.Value, color * alpha, Vector2.Zero);
		}
	}
}