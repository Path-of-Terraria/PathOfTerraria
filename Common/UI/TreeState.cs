using System.Collections.Generic;
using System.Runtime.InteropServices;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.UI.Components;
using PathOfTerraria.Common.UI.Elements;
using PathOfTerraria.Common.UI.PassiveTree;
using PathOfTerraria.Common.UI.SkillsTree;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Content.Passives;
using PathOfTerraria.Core.UI;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI;

/// <summary>
/// UI state for the Passive and Skill trees.
/// </summary>
internal class TreeState : TabsUiState, IAutopauseUI
{
	private const int ShrinkX = 0;
	private const int ShrinkY = 0;
	private const int PassiveSearchHeight = 32;
	private const int PassiveSearchPadding = 8;
	private const int PassiveSearchReservedHeight = PassiveSearchHeight + PassiveSearchPadding * 2;
	private const int PassiveSearchWidth = 280; 

	private PassiveTreeInnerPanel _passiveTreeInner;
	private SkillSelectionPanel _skillSelection;
	private readonly List<PassiveElement> _passiveElements = [];
	private readonly Dictionary<PassiveElement, string> _passiveSearchableText = [];
	private UIPanel _passiveSearchBackground;
	private UIEditableText _passiveSearchInput;
	private string _passiveSearchQuery = string.Empty;
	private string _lastAppliedSearchQuery = string.Empty;
	private string[] _passiveSearchTerms = [];

	public override List<SmartUiElement> TabPanels => [_passiveTreeInner, _skillSelection];

	protected static PassiveTreePlayer LocalPassiveTreePlayer => Main.LocalPlayer.GetModPlayer<PassiveTreePlayer>();

	public override int DepthPriority => 1;

	private int _confirmTimer = 0;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	public override void SafeUpdate(GameTime gameTime)
	{
		_confirmTimer--;

		if (Panel is not null)
		{
			Panel.Left = StyleDimension.FromPixels(ShrinkX);
			Panel.Top = StyleDimension.FromPixels(ShrinkY);
			UpdatePassiveSearchVisibility();

			if (_passiveSearchInput is { } searchInput && !string.Equals(searchInput.CurrentValue, _lastAppliedSearchQuery, StringComparison.Ordinal))
			{
				ApplyPassiveSearchHighlight();
			}
		}
		
		// Always block mouse interface when TreeState is visible
		if (IsVisible)
		{
			Main.LocalPlayer.mouseInterface = true;
			Main.isMouseLeftConsumedByUI = true;
			Main.mouseText = false;
		}
	}

	public void Toggle()
	{
		if (IsVisible)
		{
			RemoveAllChildren(); //Temporary thing to update the GUI when toggling
			_passiveTreeInner = null;
			_skillSelection = null;
			_passiveSearchBackground = null;
			_passiveSearchInput = null;
			_passiveElements.Clear();
			_passiveSearchableText.Clear();
			IsVisible = false;
			return;
		}

		_passiveTreeInner = new PassiveTreeInnerPanel();
		_passiveTreeInner.AddComponent(new UIBlockMouse());

		_skillSelection = new SkillSelectionPanel();
		_skillSelection.AddComponent(new UIBlockMouse());

		var localizedTexts = new (string key, LocalizedText text)[]
		{
				(_passiveTreeInner.TabName, Language.GetText($"Mods.PathOfTerraria.UI.{_passiveTreeInner.TabName}Tab")),
				(_skillSelection.TabName, Language.GetText($"Mods.PathOfTerraria.UI.{_skillSelection.TabName}Tab"))
		};

		base.CreateMainPanel(localizedTexts, false, panelSize: (Width: new(-ShrinkX * 2, 1f), Height: new(-ShrinkY * 2, 1f)));
		Panel.BackgroundColor = new Color(16, 24, 65, 255); 
		base.AppendChildren();
		ReservePassiveSearchSpace();
		CreatePassiveSearch();
		AddCloseButton();
		ResetTree();
		Recalculate();

		IsVisible = true;
	}

	private new void AddCloseButton()
	{
		CloseButton = new UIImageButton(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CloseButton"));
		CloseButton.Left.Set(-38 - PointsAndExitPadding, 1f);
		CloseButton.Top.Set(10, 0f);
		CloseButton.Width.Set(38, 0);
		CloseButton.Height.Set(38, 0);
		CloseButton.OnLeftClick += (a, b) => Toggle();
		CloseButton.SetVisibility(1, 1);
		Panel.Append(CloseButton);
	}

	private void ResetTree()
	{
		_passiveTreeInner.RemoveAllChildren();
		_passiveElements.Clear();
		_passiveSearchableText.Clear();

		LocalPassiveTreePlayer.CreateTree();

		// Add nodes
		var mapping = new Dictionary<int, AllocatableElement>(capacity: LocalPassiveTreePlayer.ActiveNodes.Count);
		foreach (Passive passive in CollectionsMarshal.AsSpan(LocalPassiveTreePlayer.ActiveNodes))
		{
			if (passive.IsHidden)
			{
				continue;
			}

			PassiveElement element = passive switch
			{
				JewelSocket socket => new PassiveSocket(socket),
				_ when passive.IsChoiceNode => new MultiPassiveElement(passive),
				_ => new PassiveElement(passive),
			};

			if (element != null)
			{
				mapping[passive.ReferenceId] = element;
				_passiveElements.Add(element);
				_passiveSearchableText[element] = BuildSearchableText(passive);
				_passiveTreeInner.AppendAsDraggable(element);
			}
		}

		// Add edges
		_passiveTreeInner.Connections.Clear();
		_passiveTreeInner.Connections.EnsureCapacity(LocalPassiveTreePlayer.Edges.Count);
		foreach (Edge<Allocatable> edge in LocalPassiveTreePlayer.Edges)
		{
			if (edge is { Start: Passive start, End: Passive end }
			&& mapping.TryGetValue(start.ReferenceId, out AllocatableElement uiStart)
			&& mapping.TryGetValue(end.ReferenceId, out AllocatableElement uiEnd))
			{
				_passiveTreeInner.Connections.Add(new(uiStart, uiEnd, edge.Flags));
			}
		}

		ApplyPassiveSearchHighlight(force: true);
	}

	private void ReservePassiveSearchSpace()
	{
		_passiveTreeInner.Top.Set(DraggablePanelHeight + PassiveSearchReservedHeight, 0f);
		_passiveTreeInner.Height.Set(-(DraggablePanelHeight + PassiveSearchReservedHeight), 1f);
	}

	private void CreatePassiveSearch()
	{
		_passiveSearchBackground = new UIPanel();
		_passiveSearchBackground.Left.Set(-PassiveSearchWidth - PointsAndExitPadding - 62, 1f); 
		_passiveSearchBackground.Top.Set(DraggablePanelHeight + PassiveSearchPadding, 0f);
		_passiveSearchBackground.Width.Set(PassiveSearchWidth, 0f);
		_passiveSearchBackground.Height.Set(PassiveSearchHeight, 0f);

		_passiveSearchInput = new UIEditableText(backingText: Language.GetTextValue("Mods.PathOfTerraria.UI.PassiveTreeSearchPlaceholder"), maxChars: 25);
		_passiveSearchInput.Left.Set(4f, 0f); 
		_passiveSearchInput.Top.Set(6f, 0f);
		_passiveSearchInput.Width.Set(-4f, 1f); 
		_passiveSearchInput.Height.Set(10f, 0f); 
		_passiveSearchInput.CurrentValue = _passiveSearchQuery;

		_passiveSearchBackground.Append(_passiveSearchInput);
		UpdatePassiveSearchVisibility();
	}



	private void UpdatePassiveSearchVisibility()
	{
		if (_passiveSearchBackground is null || Panel is null || _passiveTreeInner is null)
		{
			return;
		}

		bool shouldShow = IsVisible && TabPanel?.ActiveTab == _passiveTreeInner.TabName;

		if (shouldShow)
		{
			if (_passiveSearchBackground.Parent != Panel)
			{
				Panel.Append(_passiveSearchBackground);
			}

			return;
		}

		if (_passiveSearchBackground.Parent is not null)
		{
			_passiveSearchBackground.Remove();
		}

		_passiveSearchInput?.SetNotTyping();
	}

	private void ApplyPassiveSearchHighlight(bool force = false)
	{
		if (_passiveSearchInput is null)
		{
			return;
		}

		string query = _passiveSearchInput.CurrentValue?.Trim() ?? string.Empty;

		if (!force && string.Equals(query, _lastAppliedSearchQuery, StringComparison.Ordinal))
		{
			return;
		}

		_passiveSearchQuery = query;
		_lastAppliedSearchQuery = query;
		bool hasQuery = !string.IsNullOrWhiteSpace(query);
		_passiveSearchTerms = hasQuery ? query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) : [];

		foreach (PassiveElement element in _passiveElements)
		{
			element.SearchHighlighted = hasQuery
				&& _passiveSearchableText.TryGetValue(element, out string searchable)
				&& PassiveMatchesSearch(searchable, _passiveSearchTerms);
		}
	}

	private static string BuildSearchableText(Passive passive)
	{
		return string.Concat(passive.DisplayName, " ", passive.DisplayTooltip, " ", passive.Name);
	}

	private static bool PassiveMatchesSearch(string searchable, ReadOnlySpan<string> terms)
	{
		if (terms.IsEmpty)
		{
			return false;
		}

		foreach (string term in terms)
		{
			if (!searchable.Contains(term, StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
		}

		return true;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Recalculate();
		base.Draw(spriteBatch);
		DrawPanelText(spriteBatch);

		if (GetRectangle().Contains(Main.MouseScreen.ToPoint()))
		{
			Main.isMouseLeftConsumedByUI = true;
			Main.LocalPlayer.mouseInterface = true;
			Main.mouseText = false;
		}
	}

	protected void DrawPanelText(SpriteBatch spriteBatch)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/PassiveFrameSmall").Value;
		PassiveTreePlayer passiveTreePlayer = Main.LocalPlayer.GetModPlayer<PassiveTreePlayer>();
		SkillCombatPlayer skillCombatPlayer = Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>();

		Vector2 pointsDrawPoin = new Vector2(PointsAndExitPadding, PointsAndExitPadding + DraggablePanelHeight) + tex.Size() / 2;

		int points = TabPanel.ActiveTab switch
		{
			"PassiveTree" => passiveTreePlayer.Points,
			"SkillTree" => skillCombatPlayer.Points,
			_ => 0
		};

		if (TabPanel.ActiveTab != "PassiveTree") //Temp to only draw for passive tree
		{
			return;
		}
		
		AvailablePassivePointsText.DrawResettablePoints(spriteBatch, points, GetRectangle().TopLeft() + pointsDrawPoin, ref _confirmTimer, LocalPassiveTreePlayer.ResetAllNodes);
	}

	internal void SetSkillTree(Skill skill)
	{
		_skillSelection.SelectedSkill = skill;
		_skillSelection.RebuildTree();
	}

	// ReSharper disable once UnusedType.Local
	private class StopInvPlayer : ModPlayer
	{
		public override void SetControls()
		{
			if (SmartUiLoader.GetUiState<TreeState>().IsVisible)
			{
				if (Player.controlInv && Player.releaseInventory)
				{
					SmartUiLoader.GetUiState<TreeState>().Toggle();
				}
			}
		}
	}
}