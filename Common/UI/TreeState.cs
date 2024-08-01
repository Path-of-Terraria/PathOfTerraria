using System.Collections.Generic;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.TreeSystem;
using PathOfTerraria.Common.UI.PassiveTree;
using PathOfTerraria.Common.UI.SkillsTree;
using PathOfTerraria.Content.Passives;
using PathOfTerraria.Core.UI;
using Terraria.Localization;

namespace PathOfTerraria.Common.UI;

internal class TreeState : DraggableSmartUi
{
	private PassiveTreeInnerPanel _passiveTreeInner;
	private SkillsTreeInnerPanel _skillsTreeInner;
	public override List<SmartUIElement> TabPanels => [_passiveTreeInner, _skillsTreeInner];

	public override int DepthPriority => 1;

	protected static TreePlayer TreeSystem => Main.LocalPlayer.GetModPlayer<TreePlayer>();

	public Vector2 TopLeftTree;
	public Vector2 BotRightTree;

	public void Toggle()
	{
		if (IsVisible)
		{
			RemoveAllChildren(); //Temporary thing to update the GUI when toggling
			_passiveTreeInner = null;
			IsVisible = false;
			return;
		}

		if (_passiveTreeInner == null)
		{
			_passiveTreeInner = new();
			_skillsTreeInner = new();

			TopLeftTree = Vector2.Zero;
			BotRightTree = Vector2.Zero;
			var localizedTexts = new (string key, LocalizedText text)[]
			{
				(_passiveTreeInner.TabName, Language.GetText($"Mods.PathOfTerraria.GUI.{_passiveTreeInner.TabName}Tab")),
				(_skillsTreeInner.TabName, Language.GetText($"Mods.PathOfTerraria.GUI.{_skillsTreeInner.TabName}Tab"))
			};
			base.CreateMainPanel(localizedTexts, false);
			base.AppendChildren();
			AddCloseButton();

			_passiveTreeInner.RemoveAllChildren();

			TreeSystem.CreateTree();
			TreeSystem.ActiveNodes.ForEach(n =>
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

		IsVisible = true;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Recalculate();
		base.Draw(spriteBatch);
		DrawPanelText(spriteBatch);
	}
	
	protected void DrawPanelText(SpriteBatch spriteBatch)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/PassiveFrameSmall").Value;
		TreePlayer treePlayer = Main.LocalPlayer.GetModPlayer<TreePlayer>();
		SkillPlayer skillPlayer = Main.LocalPlayer.GetModPlayer<SkillPlayer>();

		Vector2 pointsDrawPoin = new Vector2(PointsAndExitPadding, PointsAndExitPadding + DraggablePanelHeight) +
		                         tex.Size() / 2;

		int points = Panel.ActiveTab switch
		{
			"PassiveTree" => treePlayer.Points,
			"SkillTree" => skillPlayer.Points,
			_ => 0
		};
		spriteBatch.Draw(tex, GetRectangle().TopLeft() + pointsDrawPoin, null, Color.White, 0, tex.Size() / 2f, 1, 0,
			0);
		Utils.DrawBorderStringBig(spriteBatch, $"{points}", GetRectangle().TopLeft() + pointsDrawPoin,
			treePlayer.Points > 0 ? Color.Yellow : Color.Gray, 0.5f, 0.5f, 0.35f);
		Utils.DrawBorderStringBig(spriteBatch, "Points remaining",
			GetRectangle().TopLeft() + pointsDrawPoin + new Vector2(138, 0), Color.White, 0.6f, 0.5f, 0.35f);
	}

	public Rectangle GetRectangle()
	{
		return Panel.GetDimensions().ToRectangle();
	}
}