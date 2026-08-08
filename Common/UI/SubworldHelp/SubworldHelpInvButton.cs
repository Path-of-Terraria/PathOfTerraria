using System.Collections.Generic;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.Maps;
using PathOfTerraria.Common.Systems.MobSystem;
using PathOfTerraria.Common.Systems.Scarabs;
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
	internal const int ButtonX = 60;
	internal const int ButtonY = 260;
	internal const int ButtonWidth = 50;
	internal const int ButtonHeight = 52;

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
		bool hover = UIHelper.GetInvButtonInfo(ButtonY, out Vector2 pos, new Point16(ButtonWidth, ButtonHeight), ButtonX);

		if (hover)
		{
			List<DrawableTooltipLine> lines = GetInformation();

			Tooltip.Create(new TooltipDescription()
			{
				Identifier = "SubworldHelp",
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

		// Only add this line if there is a tier to begin with
		if (Content.Items.Consumables.Maps.Map.TierBasedOnWorldLevel(MappingWorld.AreaLevel) > 0)
		{
			AddLine(lines, "Tier", Language.GetTextValue("Mods.PathOfTerraria.UI.SubworldHelp.MapTier") + MappingWorld.MapTier, scale);
		}

		if (MappingWorld.Affixes is { Count: > 0 } affixes)
		{
			AffixTooltips tooltips = new();
			float totalStrength = 0;
			AddLine(lines, "AffixHeading", Language.GetTextValue("Mods.PathOfTerraria.UI.SubworldHelp.Affixes"), new Vector2(1f));

			foreach (MapAffix affix in affixes)
			{
				affix.ApplyTooltips(Main.LocalPlayer, ItemType.Map, MappingWorld.AreaLevel, tooltips);
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
			AddLine(lines, "RarityMod", Language.GetTextValue("Mods.PathOfTerraria.UI.SubworldHelp.DropRarityBoost") + (rarityModifier * 100f).ToString("#0.##") + "%", scale);
		}

		if (ScarabSystem.ActiveScarabs.Count > 0)
		{
			AddLine(lines, "ScarabHeading", Language.GetTextValue("Mods.PathOfTerraria.UI.SubworldHelp.Scarabs"), new Vector2(1f));
			for (int i = 0; i < ScarabSystem.ActiveScarabs.Count; i++)
			{
				ScarabEntry scarab = ScarabSystem.ActiveScarabs[i];
				AddLine(lines, "Scarab" + i, $"    [i:4348] {scarab.Grade} {scarab.Kind}", scale, ScarabCatalog.GetColor(scarab.Kind));
			}
		}

		CurrentWorld.ModifyHelpTooltips(lines, scale);
		return lines;
	}

	internal static void AddLine(List<DrawableTooltipLine> lines, string name, string text, Vector2 scale, Color? color = null, int index = -1)
	{
		var line = new DrawableTooltipLine(new TooltipLine(PoTMod.Instance, name, text), lines.Count, 0, lines.Count * 24, color ?? Color.White) { BaseScale = scale };

		if (index == -1)
		{
			lines.Add(line);
		}
		else
		{
			lines.Insert(index, line);
		}
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		if (!UIHelper.GetInvButtonInfo(ButtonY, out _, new Point16(ButtonWidth, ButtonHeight), ButtonX))
		{
			return;
		}

		SoundEngine.PlaySound(SoundID.MenuOpen);
	}
}
