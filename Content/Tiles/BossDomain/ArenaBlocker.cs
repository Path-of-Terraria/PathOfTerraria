using PathOfTerraria.Common.Subworlds.BossDomains.WoFDomain;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class ArenaBlocker : ModTile
{
	public override void SetStaticDefaults()
	{
		TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
		TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
		TileID.Sets.CanBeDugByShovel[Type] = true;

		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileLighted[Type] = true;

		Main.tileMerge[Type][TileID.ObsidianBrick] = true;
		Main.tileMerge[TileID.ObsidianBrick][Type] = true;

		AddMapEntry(new Color(175, 56, 76));

		DustType = DustID.PurpleMoss;
		HitSound = SoundID.NPCHit1;
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		float sine = MathF.Sin((i + j) * 20 + Main.GameUpdateCount * 0.06f) * 0.16f;
		(r, g, b) = (sine * 0.67f, sine * 0.2f, sine * 0.9f);
	}

	class BlockerSystem : ModSystem
	{
		public override void PreUpdateWorld()
		{
			bool anyArenas = false;

			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.GetGlobalNPC<ArenaEnemyNPC>().Arena)
				{
					anyArenas = true;
					break;
				}
			}

			Main.tileSolid[ModContent.TileType<ArenaBlocker>()] = anyArenas;
		}
	}
}
