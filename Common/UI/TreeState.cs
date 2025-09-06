using System.Collections.Generic;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.UI.PassiveTree;
using PathOfTerraria.Common.UI.SkillsTree;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Content.Passives;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI;

/// <summary>
/// UI state for the Passive and Skill trees.
/// </summary>
internal class TreeState : TabsUiState
{
	private const int ShrinkX = 80;
	private const int ShrinkY = 20;

	private PassiveTreeInnerPanel _passiveTreeInner;
	private SkillSelectionPanel _skillSelection;

	public override List<SmartUiElement> TabPanels => [_passiveTreeInner, _skillSelection];
	protected static PassiveTreePlayer PassiveTreeSystem => Main.LocalPlayer.GetModPlayer<PassiveTreePlayer>();
	public override int DepthPriority => 1;


	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	public override void SafeUpdate(GameTime gameTime)
	{
		if (Panel is not null)
		{
			Panel.Left = StyleDimension.FromPixels(ShrinkX);
			Panel.Top = StyleDimension.FromPixels(ShrinkY);
		}
	}

	public void Toggle()
	{
		if (IsVisible)
		{
			RemoveAllChildren(); //Temporary thing to update the GUI when toggling
			_passiveTreeInner = null;
			_skillSelection = null;
			IsVisible = false;
			return;
		}

		_passiveTreeInner = new PassiveTreeInnerPanel();
		_skillSelection = new SkillSelectionPanel();

		var localizedTexts = new (string key, LocalizedText text)[]
		{
				(_passiveTreeInner.TabName, Language.GetText($"Mods.PathOfTerraria.GUI.{_passiveTreeInner.TabName}Tab")),
				(_skillSelection.TabName, Language.GetText($"Mods.PathOfTerraria.GUI.{_skillSelection.TabName}Tab"))
		};
		base.CreateMainPanel(localizedTexts, false, panelSize: new Point(Main.screenWidth - ShrinkX * 2, Main.screenHeight - ShrinkY * 2));
		base.AppendChildren();
		AddCloseButton();
		ResetTree();

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

		PassiveTreeSystem.CreateTree();

		// Add nodes
		var mapping = new Dictionary<int, AllocatableElement>(capacity: PassiveTreeSystem.ActiveNodes.Count);
		foreach (Passive passive in PassiveTreeSystem.ActiveNodes)
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
				_passiveTreeInner.AppendAsDraggable(element);
			}
		}

		// Add edges
		_passiveTreeInner.Connections.Clear();
		_passiveTreeInner.Connections.EnsureCapacity(PassiveTreeSystem.Edges.Count);
		foreach (Edge<Allocatable> edge in PassiveTreeSystem.Edges)
		{
			if (edge is { Start: Passive start, End: Passive end }
			&& mapping.TryGetValue(start.ReferenceId, out AllocatableElement uiStart)
			&& mapping.TryGetValue(end.ReferenceId, out AllocatableElement uiEnd))
			{
				_passiveTreeInner.Connections.Add(new(uiStart, uiEnd, edge.Flags));
			}
		}
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
		
		AvailablePassivePointsText.DrawAvailablePassivePoint(spriteBatch, points, GetRectangle().TopLeft() + pointsDrawPoin);
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