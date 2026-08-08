using PathOfTerraria.Content.NPCs.Runebound;
using ReLogic.Content;
using Terraria.Map;
using Terraria.UI;

namespace PathOfTerraria.Common.Systems.Runebound;

/// <summary>
/// Displays unopened Runebound encounters after the player discovers them.
/// The monolith is the authoritative marker, so the icon disappears with it when the binding is broken.
/// </summary>
internal sealed class RuneboundMapLayer : ModMapLayer
{
	private static Asset<Texture2D> icon;

	public override void Load()
	{
		if (Main.dedServ)
		{
			return;
		}

		icon = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/MapIcons/RuneboundSeal");
	}

	public override void Unload()
	{
		icon = null;
	}

	public override void Draw(ref MapOverlayDrawContext context, ref string text)
	{
		if (icon is null)
		{
			return;
		}

		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.ModNPC is not BindingMonolith monolith)
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
				text = $"{npc.FullName}: {monolith.Grade} {monolith.Family}";
			}
		}
	}
}
