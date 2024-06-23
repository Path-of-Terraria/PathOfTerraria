using PathOfTerraria.Content.Passives;
using PathOfTerraria.Core;
using PathOfTerraria.Core.Systems.TreeSystem;
using PathOfTerraria.Content.GUI.PassiveTree;
using PathOfTerraria.Content.GUI.SkillsTree;
using PathOfTerraria.Content.GUI.Utilities;
using PathOfTerraria.Core.Systems.ModPlayers;
using Terraria.Localization;

namespace PathOfTerraria.Content.GUI;

internal class TreeState : DraggableSmartUi
{
	private PassiveTreeInnerPanel _passiveTreeInner;
	private SkillsTreeInnerPanel _skillsTreeInner;

	protected static TreePlayer TreeSystem => Main.LocalPlayer.GetModPlayer<TreePlayer>();

	public Vector2 TopLeftTree;
	public Vector2 BotRightTree;
	
	public PlayerClass CurrentDisplayClass = PlayerClass.None;

	public void Toggle(PlayerClass newClass = PlayerClass.None)
	{
		if (newClass == PlayerClass.None || IsVisible)
		{
			IsVisible = false;
			return;
		}

		if (CurrentDisplayClass != newClass)
		{
			TopLeftTree = Vector2.Zero;
			BotRightTree = Vector2.Zero;
			CurrentDisplayClass = newClass;
			RemoveAllChildren();
			CreateMainPanel();
			AddPassiveTreeInnerPanel();
			AddSkillsTreeInnerPanel();
			AddCloseButton();

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

	protected void CreateMainPanel()
	{
		var localizedTexts = new (string key, LocalizedText text)[]
		{
			("PassiveTree", Language.GetText("Mods.PathOfTerraria.GUI.PassiveTreeTab")),
			("SkillTree", Language.GetText("Mods.PathOfTerraria.GUI.SkillTreeTab"))
		};
		Panel = new UIDraggablePanel(false, false, localizedTexts, DraggablePanelHeight);
		Panel.OnActiveTabChanged += HandleActiveTabChanged;
		Panel.Left.Set(LeftPadding, 0.5f);
		Panel.Top.Set(TopPadding, 0.5f);
		Panel.Width.Set(PanelWidth, 0);
		Panel.Height.Set(PanelHeight, 0);
		Append(Panel);
	}

	private void HandleActiveTabChanged()
	{
		switch (Panel.ActiveTab)
		{
			case "PassiveTree":
				_passiveTreeInner.Visible = true;
				_skillsTreeInner.Visible = false;
				break;
			case "SkillTree":
				_passiveTreeInner.Visible = false;
				_skillsTreeInner.Visible = true;
				break;
		}
	}

	protected void AddPassiveTreeInnerPanel()
	{
		_passiveTreeInner = new PassiveTreeInnerPanel();
		_passiveTreeInner.Left.Set(0, 0);
		_passiveTreeInner.Top.Set(DraggablePanelHeight, 0);
		_passiveTreeInner.Width.Set(0, 1f);
		_passiveTreeInner.Height.Set(-DraggablePanelHeight, 1f);
		Panel.Append(_passiveTreeInner);
	}
	
	protected void AddSkillsTreeInnerPanel()
	{
		_skillsTreeInner = new SkillsTreeInnerPanel();
		_skillsTreeInner.Left.Set(0, 0);
		_skillsTreeInner.Top.Set(DraggablePanelHeight, 0);
		_skillsTreeInner.Width.Set(0, 1f);
		_skillsTreeInner.Height.Set(-DraggablePanelHeight, 1f);
		Panel.Append(_skillsTreeInner);
	}
	
	protected void DrawPanelText(SpriteBatch spriteBatch)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/PassiveFrameSmall").Value;
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