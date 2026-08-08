#if DEBUG
using System.Collections.Generic;
using PathOfTerraria.Common.Config;
using PathOfTerraria.Common.Debugging;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Common.UI.SubworldHelp;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Core.UI.SmartUI;
using SubworldLibrary;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Debugging;

internal sealed class MapContentInspectorButton : SmartUiState
{
	private const int ButtonGap = 6;
	private static bool lastHover;

	public override bool Visible => Main.playerInventory
		&& ModContent.GetInstance<DeveloperConfig>().EnableMapContentInspector
		&& MapContentInspection.IsInExplorationMap;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	public override void SafeUpdate(GameTime gameTime)
	{
		base.SafeUpdate(gameTime);
		if (GetButtonInfo(out _))
		{
			Main.LocalPlayer.mouseInterface = true;
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Texture2D texture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/WorldInfoButton").Value;
		bool hover = GetButtonInfo(out Vector2 position);

		if (hover)
		{
			Player player = Main.LocalPlayer;
			player.cursorItemIconText = Language.GetTextValue("Mods.PathOfTerraria.UI.InvButtons.MapContentInspector");
			player.noThrow = 2;
			player.cursorItemIconID = -1;
			player.cursorItemIconEnabled = true;
		}

		if (hover != lastHover)
		{
			SoundEngine.PlaySound(lastHover ? SoundID.MenuTick with { Pitch = -0.3f } : SoundID.MenuTick);
		}

		lastHover = hover;
		Color color = hover ? Color.White : new Color(125, 230, 255);
		spriteBatch.Draw(texture, position, new Rectangle(0, hover ? 54 : 0, 50, 52), color, 0f,
			new Vector2(texture.Width / 1.125f, 0f), 1f, SpriteEffects.None, 0f);
		Utils.DrawBorderString(spriteBatch, "DBG", position + new Vector2(-34f, 31f), Color.Cyan, 0.55f);
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		if (!GetButtonInfo(out _))
		{
			return;
		}

		SoundEngine.PlaySound(SoundID.MenuOpen);
		SmartUiLoader.GetUiState<MapContentInspectorUI>().Toggle();
	}

	private static bool GetButtonInfo(out Vector2 position)
	{
		int x = SubworldHelpInvButton.ButtonX + SubworldHelpInvButton.ButtonWidth + ButtonGap;
		return UIHelper.GetInvButtonInfo(SubworldHelpInvButton.ButtonY, out position,
			new Point16(SubworldHelpInvButton.ButtonWidth, SubworldHelpInvButton.ButtonHeight), x);
	}
}

internal sealed class MapContentInspectorUI : TabsUiState, IMutuallyExclusiveUI
{
	private static readonly string[] TabNames =
	[
		MapContentInspection.OverviewTab,
		MapContentInspection.ContentTab,
		MapContentInspection.ContainersTab,
		MapContentInspection.ModifiersTab,
		MapContentInspection.ScarabsTab,
		MapContentInspection.DiagnosticsTab,
	];

	private readonly List<SmartUiElement> tabPanels = [];
	private ulong displayedTick = ulong.MaxValue;
	private int refreshTimer;

	protected override int TopPadding => -350;
	protected override int PanelHeight => 700;
	protected override int LeftPadding => -450;
	protected override int PanelWidth => 900;
	public override int DepthPriority => 4;
	public override List<SmartUiElement> TabPanels => tabPanels;

	public void Toggle()
	{
		if (IsVisible)
		{
			IsVisible = false;
			return;
		}

		if (!MapContentInspection.IsInExplorationMap || !ModContent.GetInstance<DeveloperConfig>().EnableMapContentInspector)
		{
			return;
		}

		ModContent.GetInstance<SmartUiLoader>().ClearMutuallyExclusive<MapContentInspectorUI>();
		RemoveAllChildren();
		tabPanels.Clear();

		(string key, LocalizedText text)[] tabs = new (string, LocalizedText)[TabNames.Length];
		for (int i = 0; i < TabNames.Length; i++)
		{
			string tab = TabNames[i];
			tabs[i] = (tab, Language.GetText($"Mods.PathOfTerraria.UI.MapContentInspector.Tabs.{tab}"));
			tabPanels.Add(new InspectionTabPanel(tab));
		}

		CreateMainPanel(tabs);
		AppendChildren();
		IsVisible = true;
		displayedTick = ulong.MaxValue;
		refreshTimer = 0;
		RequestRefresh();
		RebuildPanels();
		Recalculate();
	}

	public override void SafeUpdate(GameTime gameTime)
	{
		if (!Main.playerInventory || !MapContentInspection.IsInExplorationMap
			|| !ModContent.GetInstance<DeveloperConfig>().EnableMapContentInspector)
		{
			IsVisible = false;
			return;
		}

		if (refreshTimer-- <= 0)
		{
			refreshTimer = 60;
			RequestRefresh();
		}

		if (MapContentInspection.LatestSnapshot is { } snapshot && snapshot.CapturedTick != displayedTick)
		{
			RebuildPanels();
		}
	}

	private static void RequestRefresh()
	{
		MapContentInspectionHandler.Request();
	}

	private void RebuildPanels()
	{
		MapInspectionSnapshot snapshot = MapContentInspection.LatestSnapshot;
		if (snapshot is null)
		{
			foreach (InspectionTabPanel panel in tabPanels)
			{
				panel.SetEntries([new(panel.TabName, Language.GetTextValue("Mods.PathOfTerraria.UI.MapContentInspector.Loading"), "...", "")]);
			}

			return;
		}

		displayedTick = snapshot.CapturedTick;
		foreach (InspectionTabPanel panel in tabPanels)
		{
			panel.SetEntries(snapshot.ForTab(panel.TabName));
		}

		Recalculate();
	}

	private sealed class InspectionTabPanel : SmartUiElement
	{
		private readonly UIList list;
		private readonly string tabName;

		public override string TabName => tabName;

		internal InspectionTabPanel(string name)
		{
			tabName = name;
			list = new UIList
			{
				Left = StyleDimension.FromPixels(14f),
				Top = StyleDimension.FromPixels(14f),
				Width = new StyleDimension(-28f, 1f),
				Height = new StyleDimension(-28f, 1f),
				ListPadding = 5f,
			};
			list.SetScrollbar(new UIScrollbar());
			Append(list);
		}

		internal void SetEntries(IEnumerable<MapInspectionEntry> entries)
		{
			list.Clear();
			bool any = false;
			foreach (MapInspectionEntry entry in entries)
			{
				list.Add(new InspectionRow(entry));
				any = true;
			}

			if (!any)
			{
				list.Add(new InspectionRow(new(TabName,
					Language.GetTextValue("Mods.PathOfTerraria.UI.MapContentInspector.Empty"), "0", "")));
			}
		}
	}

	private sealed class InspectionRow : UIPanel
	{
		internal InspectionRow(MapInspectionEntry entry)
		{
			Height = StyleDimension.FromPixels(string.IsNullOrWhiteSpace(entry.Detail) ? 34f : 58f);
			Width = StyleDimension.Fill;
			SetPadding(7f);
			BackgroundColor = new Color(24, 32, 48, 210);
			BorderColor = new Color(70, 150, 175, 180);

			var label = new UIText(Trim(entry.Label, 54), 0.88f)
			{
				Left = StyleDimension.FromPixels(2f),
				Top = StyleDimension.FromPixels(-2f),
				TextColor = Color.LightCyan,
			};
			Append(label);

			var value = new UIText(Trim(entry.Value, 30), 0.88f)
			{
				HAlign = 1f,
				Top = StyleDimension.FromPixels(-2f),
				TextColor = Color.White,
			};
			Append(value);

			if (!string.IsNullOrWhiteSpace(entry.Detail))
			{
				var detail = new UIText(Trim(entry.Detail, 130), 0.72f)
				{
					Left = StyleDimension.FromPixels(2f),
					Top = StyleDimension.FromPixels(23f),
					TextColor = Color.SlateGray,
				};
				Append(detail);
			}
		}

		private static string Trim(string text, int length)
		{
			return text.Length <= length ? text : text[..Math.Max(0, length - 3)] + "...";
		}
	}
}
#endif
