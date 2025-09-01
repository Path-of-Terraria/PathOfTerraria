using System.Collections.Generic;
using PathOfTerraria.Api.Tooltips;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.MobSystem;
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

			Tooltips.Create(new()
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
		AddLine(lines, "Info", Language.GetTextValue("Mods.PathOfTerraria.UI.SubworldHelp.Info"), new Vector2(0.8f), Color.Gray);
		AddLine(lines, "Name", CurrentWorld.SubworldName.Value, new Vector2(1.1f));
		AddLine(lines, "Desc", CurrentWorld.SubworldDescription.Value, scale);
		AddLine(lines, "Mining", CurrentWorld.SubworldMining.Value, scale);
		AddLine(lines, "Placing", CurrentWorld.SubworldPlacing.Value, scale);
		AddLine(lines, "Tier", Language.GetTextValue("Mods.PathOfTerraria.UI.SubworldHelp.MapTier") + MappingWorld.MapTier, scale);

		if (MappingWorld.Affixes.Count > 0)
		{
			AffixTooltips tooltips = new();
			float totalStrength = 0;
			AddLine(lines, "AffixHeading", Language.GetTextValue("Mods.PathOfTerraria.UI.SubworldHelp.Affixes"), new Vector2(1f));

			foreach (MapAffix affix in MappingWorld.Affixes)
			{
				affix.ApplyTooltips(Main.LocalPlayer, MappingWorld.AreaLevel, tooltips);
				totalStrength += affix.Strength;
			}

			foreach (KeyValuePair<Type, AffixTooltipLine> affix in tooltips.Lines)
			{
				AffixTooltipLine tip = affix.Value;
				AddLine(lines, "MapAffix" + affixCount++, "    [i:278] " + tip.Text.Format(Math.Abs(tip.Value).ToString("#0.##"), tip.Value >= 0 ? "+" : "-"), scale);
			}

			AddLine(lines, "ModifierStrength", Language.GetTextValue("Mods.PathOfTerraria.UI.SubworldHelp.MapStrength") + totalStrength, scale);
			AddLine(lines, "ExpMod", Language.GetTextValue("Mods.PathOfTerraria.UI.SubworldHelp.ExperienceBoost") + (totalStrength / 2f).ToString("#0.###") + "%", scale);

			float rateModifier = ArpgNPC.DomainDropRateBoost(totalStrength);
			AddLine(lines, "RateMod", Language.GetTextValue("Mods.PathOfTerraria.UI.SubworldHelp.DropRateBoost") + (rateModifier * 100f).ToString("#0.###") + "%", scale);

			float rarityModifier = ArpgNPC.DomainRarityBoost(totalStrength);
			AddLine(lines, "RarityMod", Language.GetTextValue("Mods.PathOfTerraria.UI.SubworldHelp.DropRarityBoost") + rarityModifier.ToString("#0.##") + "%", scale);
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