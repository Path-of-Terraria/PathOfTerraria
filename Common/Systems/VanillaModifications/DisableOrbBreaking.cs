using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.VanillaModifications;

public class DisableOrbBreaking : GlobalTile
{
	public static bool CanBreakOrb 
	{
		get => BreakableOrbSystem.CanBreakOrb;
		set => BreakableOrbSystem.CanBreakOrb = value;
	}

	public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
	{
		Tile tile = Main.tile[i, j];

		if (tile.HasTile && tile.TileType == TileID.ShadowOrbs)
		{
			return BreakableOrbSystem.CanBreakOrb;
		}

		return true;
	}

	public override bool CanExplode(int i, int j, int type)
	{
		Tile tile = Main.tile[i, j];

		if (tile.HasTile && tile.TileType == TileID.ShadowOrbs)
		{
			return BreakableOrbSystem.CanBreakOrb;
		}

		return true;
	}

	public override void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
	{
		Tile tile = Main.tile[i, j];

		if (tile.HasTile && tile.TileType == TileID.ShadowOrbs && !BreakableOrbSystem.CanBreakOrb)
		{
			float sine = MathF.Sin(Main.GameUpdateCount * 0.1f + (i + j) * MathHelper.PiOver4) * 0.15f;
			drawData.tileLight = Color.Lerp(WorldGen.crimson ? Color.Red : Color.Purple, Color.White, sine + 0.45f);

			if (Main.rand.NextBool(16) && tile.TileFrameX == 0 && tile.TileFrameY % 36 == 0)
			{
				Vector2 position = new Vector2(i, j).ToWorldCoordinates(16, 16);
				Dust.NewDustPerfect(position, WorldGen.crimson ? DustID.Clentaminator_Red : DustID.Clentaminator_Purple, Main.rand.NextVector2Circular(1, 1));
			}
		}
	}

	public class BreakableOrbSystem : ModSystem
	{
		internal static bool CanBreakOrb = false;

		public override void ClearWorld()
		{
			CanBreakOrb = false;
		}

		public override void SaveWorldData(TagCompound tag)
		{
			if (CanBreakOrb)
			{
				tag.Add("canBreakOrb", true);
			}
		}

		public override void LoadWorldData(TagCompound tag)
		{
			CanBreakOrb = tag.ContainsKey("canBreakOrb");
		}
	}
}
