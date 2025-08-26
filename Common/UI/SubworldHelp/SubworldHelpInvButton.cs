using System.Collections.Generic;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.UI.SmartUI;
using SubworldLibrary;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.SubworldHelp;

public class SubworldHelpInvButton : SmartUiState
{
	public override bool Visible => Main.playerInventory && SubworldSystem.Current is MappingWorld;

	private static MappingWorld CurrentWorld => SubworldSystem.Current as MappingWorld;

	private static bool LastHover = false;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Texture2D texture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/WorldInfoButton").Value;
		bool hover = UIHelper.GetInvButtonInfo(260, out Vector2 pos, new Point16(50, 52), 60);

		if (hover)
		{
			List<DrawableTooltipLine> lines = GetInformation();

			Tooltip.Create(new TooltipDescription()
			{
				Identifier = "SubworldHelp",
				//SimpleTitle = CurrentWorld.SubworldName.Value,
				Lines = lines,
				Stability = 5,
			});
		}

		if (hover != LastHover)
		{
			SoundEngine.PlaySound(LastHover ? SoundID.MenuTick with { Pitch = -0.3f } : SoundID.MenuTick);
		}

		LastHover = hover;

		spriteBatch.Draw(texture, pos, new Rectangle(0, hover ? 54 : 0, 50, 52), Color.White, 0, new Vector2(texture.Width / 1.125f, 0), 1, 0, 0);
	}

	private static List<DrawableTooltipLine> GetInformation()
	{
		List<DrawableTooltipLine> lines = [];
		var scale = new Vector2(0.9f);
		int affixCount = 0;
		AddLine(lines, "Info", Language.GetTextValue("Mods.PathOfTerraria.Subworlds.SubworldInformation"), new Vector2(0.8f), Color.Gray);
		AddLine(lines, "Name", CurrentWorld.SubworldName.Value, new Vector2(1.1f));
		AddLine(lines, "Desc", CurrentWorld.SubworldDescription.Value, scale);
		AddLine(lines, "Mining", CurrentWorld.SubworldMining.Value, scale);
		AddLine(lines, "Placing", CurrentWorld.SubworldPlacing.Value, scale);
		AddLine(lines, "Level", "[i:1343] Level:" + MappingWorld.AreaLevel, scale);
		AddLine(lines, "Level", "[i:4927] Tier:" + MappingWorld.MapTier, scale);

		if (MappingWorld.Affixes.Count > 0)
		{
			AddLine(lines, "Level", "[i:885] Affixes:", scale);

			foreach (MapAffix affix in MappingWorld.Affixes)
			{
				AddLine(lines, "MapAffix" + affixCount++, "    [i:278] " + affix.Name, scale);
			}
		}

		return lines;
	}

	private static void AddLine(List<DrawableTooltipLine> lines, string name, string text, Vector2 scale, Color? color = null)
	{
		lines.Add(new DrawableTooltipLine(new TooltipLine(PoTMod.Instance, name, text), lines.Count, 0, lines.Count * 24, color ?? Color.White) { BaseScale = scale });
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		if (!UIHelper.GetInvButtonInfo(260, out Vector2 pos, new Point16(50, 52), 60))
		{
			return;
		}

		SoundEngine.PlaySound(SoundID.MenuOpen);
	}
}