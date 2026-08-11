using PathOfTerraria.Content.NPCs.Mapping.Sigils;
using ReLogic.Content;
using Terraria.ID;
using Terraria.Map;
using Terraria.UI;

namespace PathOfTerraria.Common.Systems.Sigils;

internal sealed class SigilMapLayer : ModMapLayer
{
	private static Asset<Texture2D> nemesisIcon;
	private static Asset<Texture2D> cacheIcon;
	private static Asset<Texture2D> shrineIcon;

	public override void Load()
	{
		if (Main.dedServ) { return; }
		nemesisIcon = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.CelestialSigil}");
		cacheIcon = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.GoldenKey}");
		shrineIcon = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.CrystalShard}");
	}

	public override void Draw(ref MapOverlayDrawContext context, ref string text)
	{
		if (!SigilSystem.IsExplorationMap()) { return; }

		foreach (NPC npc in Main.ActiveNPCs)
		{
			Asset<Texture2D> icon;
			string label;
			if (npc.GetGlobalNPC<SigilEncounterNPC>().Nemesis)
			{
				icon = nemesisIcon;
				label = "Nemesis Captain";
			}
			else if (npc.ModNPC is WardedCacheNPC)
			{
				icon = cacheIcon;
				label = "Warded Cache";
			}
			else if (npc.ModNPC is ConsecratedShrineNPC)
			{
				icon = shrineIcon;
				label = "Consecrated Shrine";
			}
			else
			{
				continue;
			}

			Point tile = npc.Center.ToTileCoordinates();
			bool nearby = Main.LocalPlayer.DistanceSQ(npc.Center) < 1600f * 1600f;
			if (!nearby && (!WorldGen.InWorld(tile.X, tile.Y) || !Main.Map.IsRevealed(tile.X, tile.Y)))
			{
				continue;
			}

			if (context.Draw(icon.Value, npc.Center / 16f, Alignment.Center).IsMouseOver)
			{
				text = label;
			}
		}
	}
}
