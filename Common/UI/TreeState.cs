﻿using System.Collections.Generic;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.UI.PassiveTree;
using PathOfTerraria.Common.UI.SkillsTree;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Content.Passives;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI;

/// <summary>
/// UI state for the Passive and Skill trees. Despite being a <see cref="DraggableSmartUi"/>, cannot be moved - left as-is for ease of use.
/// </summary>
internal class TreeState : DraggableSmartUi
{
	private const int ShrinkX = 80;
	private const int ShrinkY = 20;

	private PassiveTreeInnerPanel _passiveTreeInner;
	private SkillSelectionPanel _skillSelection;

	public override List<SmartUiElement> TabPanels => [_passiveTreeInner, _skillSelection];

	public override int DepthPriority => 1;

	protected static PassiveTreePlayer PassiveTreeSystem => Main.LocalPlayer.GetModPlayer<PassiveTreePlayer>();

	public Vector2 TopLeftTree;
	public Vector2 BotRightTree;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
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

		if (_passiveTreeInner == null || _skillSelection == null)
		{
			_passiveTreeInner = new PassiveTreeInnerPanel();
			_skillSelection = new SkillSelectionPanel();

			TopLeftTree = Vector2.Zero;
			BotRightTree = Vector2.Zero;
			var localizedTexts = new (string key, LocalizedText text)[]
			{
				(_passiveTreeInner.TabName, Language.GetText($"Mods.PathOfTerraria.GUI.{_passiveTreeInner.TabName}Tab")),
				(_skillSelection.TabName, Language.GetText($"Mods.PathOfTerraria.GUI.{_skillSelection.TabName}Tab"))
			};
			base.CreateMainPanel(localizedTexts, false, panelSize: new Point(Main.screenWidth - ShrinkX * 2, Main.screenHeight - ShrinkY * 2));
			base.AppendChildren();
			AddCloseButton();
			ResetTree();
		}

		IsVisible = true;
	}

	internal void ResetTree()
	{
		_passiveTreeInner.RemoveAllChildren();

		PassiveTreeSystem.CreateTree();
		PassiveTreeSystem.ActiveNodes.ForEach(n =>
		{
			if (n is JewelSocket socket)
			{
				_passiveTreeInner.Append(new PassiveSocket(socket));
			}
			else
			{
				_passiveTreeInner.Append(new PassiveElement(n));
			}
		});
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

	public override void SafeUpdate(GameTime gameTime)
	{
		if (Panel is not null)
		{
			Panel.Left = StyleDimension.FromPixels(ShrinkX);
			Panel.Top = StyleDimension.FromPixels(ShrinkY);
		}
	}

	protected void DrawPanelText(SpriteBatch spriteBatch)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/PassiveFrameSmall").Value;
		PassiveTreePlayer passiveTreePlayer = Main.LocalPlayer.GetModPlayer<PassiveTreePlayer>();
		SkillCombatPlayer skillCombatPlayer = Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>();

		Vector2 pointsDrawPoin = new Vector2(PointsAndExitPadding, PointsAndExitPadding + DraggablePanelHeight) +
		                         tex.Size() / 2;

		int points = Panel.ActiveTab switch
		{
			"PassiveTree" => passiveTreePlayer.Points,
			"SkillTree" => skillCombatPlayer.Points,
			_ => 0
		};
		if (Panel.ActiveTab != "PassiveTree") //Temp to only draw for passive tree
		{
			return;
		}
		
		AvailablePassivePointsText.DrawAvailablePassivePoint(spriteBatch, points, GetRectangle().TopLeft() + pointsDrawPoin);
	}

	public Rectangle GetRectangle()
	{
		return Panel.GetDimensions().ToRectangle();
	}

	private class StopInvPlayer : ModPlayer
	{
		public override void SetControls()
		{
			if (SmartUiLoader.GetUiState<TreeState>().IsVisible)
			{
				if (Player.controlInv && Player.releaseInventory)
				{
					SmartUiLoader.GetUiState<TreeState>().DefaultClose();
				}

				Player.controlInv = false;
				Player.releaseInventory = false;
			}
		}
	}
}